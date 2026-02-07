using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkLog.Domain.Entities;
using WorkLog.Domain.Enums;
using WorkLog.Domain.Interfaces;
using WorkLog.Shared.DTOs;
using WorkLog.Shared.DTOs.Auth;

namespace WorkLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    /// <summary>
    /// 使用者註冊
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserInfo>>> Register([FromBody] RegisterRequest request)
    {
        // 檢查使用者名稱是否已存在
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
            return Conflict(new ApiErrorResponse { Message = "使用者名稱已存在" });

        // 檢查電子郵件是否已存在
        var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
            return Conflict(new ApiErrorResponse { Message = "電子郵件已被註冊" });

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        return Ok(new ApiResponse<UserInfo>
        {
            Data = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString()
            },
            Message = "註冊成功"
        });
    }

    /// <summary>
    /// 使用者登入
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        
        // 調試日誌
        Console.WriteLine($"[Login] Username: {request.Username}");
        Console.WriteLine($"[Login] User found: {user != null}");
        if (user != null)
        {
            Console.WriteLine($"[Login] User ID: {user.Id}");
            Console.WriteLine($"[Login] Password hash: {user.PasswordHash}");
            var verifyResult = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            Console.WriteLine($"[Login] Password verify: {verifyResult}");
        }
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new ApiErrorResponse { Message = "使用者名稱或密碼錯誤" });

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiresDay = int.Parse(
            _configuration["Jwt:RefreshTokenExpiresDay"] ?? "7");

        await _refreshTokenRepository.CreateAsync(new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiresDay),
            CreatedAt = DateTime.UtcNow
        });

        var expiresMinutes = int.Parse(_configuration["Jwt:ExpiresMinutes"] ?? "60");

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes),
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        });
    }

    /// <summary>
    /// 刷新 Token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            return Unauthorized(new ApiErrorResponse { Message = "Refresh Token 無效或已過期" });

        // 撤銷舊 Token
        await _refreshTokenRepository.RevokeAsync(request.RefreshToken);

        var user = storedToken.User;
        var accessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiresDay = int.Parse(
            _configuration["Jwt:RefreshTokenExpiresDay"] ?? "7");

        await _refreshTokenRepository.CreateAsync(new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiresDay),
            CreatedAt = DateTime.UtcNow
        });

        var expiresMinutes = int.Parse(_configuration["Jwt:ExpiresMinutes"] ?? "60");

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes),
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        });
    }

    /// <summary>
    /// 取得目前使用者資訊
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserInfo>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return NotFound();

        return Ok(new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString()
        });
    }

    /// <summary>
    /// 產生密碼哈希 (僅供開發測試用)
    /// </summary>
    [HttpPost("generate-hash")]
    public ActionResult<string> GenerateHash([FromBody] string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash: {hash}");
        Console.WriteLine($"Verify: {BCrypt.Net.BCrypt.Verify(password, hash)}");
        return Ok(new { password, hash });
    }

    /// <summary>
    /// 更新管理員密碼 (僅供開發測試用)
    /// </summary>
    [HttpPost("update-admin-password")]
    public async Task<ActionResult> UpdateAdminPassword()
    {
        var admin = await _userRepository.GetByUsernameAsync("admin");
        if (admin == null) return NotFound("Admin user not found");

        var newHash = "$2a$11$PXJswxf4o10iq2vzhkCJE.E7D3iWySGWips2a7SJAXsv0P0Zt2dvm";
        admin.PasswordHash = newHash;
        await _userRepository.UpdateAsync(admin);

        Console.WriteLine($"Admin password updated!");
        Console.WriteLine($"Old verify: {BCrypt.Net.BCrypt.Verify("Admin@123", "$2a$11$8zK5N5rJc1GqvFjXh8yXPO9ZQXqM3gYxN3jB5lZ9vXhN1qW5qO5Mu")}");
        Console.WriteLine($"New verify: {BCrypt.Net.BCrypt.Verify("Admin@123", newHash)}");

        return Ok(new { message = "Password updated", newHash });
    }
}
