using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using WorkLog.Shared.DTOs.Auth;

namespace WorkLog.Web.Services;

/// <summary>
/// JWT 認證狀態提供者 + 登入/登出/註冊邏輯
/// </summary>
public class AuthService : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsStringAsync("accessToken");
            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(_anonymous);

            token = token.Trim('"');
            var claims = ParseClaimsFromJwt(token);
            if (claims == null)
                return new AuthenticationState(_anonymous);

            // 檢查 Token 是否過期
            var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
            if (expClaim != null)
            {
                var expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value));
                if (expTime <= DateTimeOffset.UtcNow)
                {
                    await Logout();
                    return new AuthenticationState(_anonymous);
                }
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task<AuthResponse?> Login(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        if (!response.IsSuccessStatusCode)
            return null;

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (authResponse == null) return null;

        await _localStorage.SetItemAsStringAsync("accessToken", authResponse.AccessToken);
        await _localStorage.SetItemAsStringAsync("refreshToken", authResponse.RefreshToken);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return authResponse;
    }

    public async Task<bool> Register(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
        return response.IsSuccessStatusCode;
    }

    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("accessToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<string?> GetTokenAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("accessToken");
        return token?.Trim('"');
    }

    public async Task<string?> TryRefreshToken()
    {
        var refreshToken = await _localStorage.GetItemAsStringAsync("refreshToken");
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;

        refreshToken = refreshToken.Trim('"');
        var response = await _httpClient.PostAsJsonAsync("api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = refreshToken });

        if (!response.IsSuccessStatusCode)
        {
            await Logout();
            return null;
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (authResponse == null)
        {
            await Logout();
            return null;
        }

        await _localStorage.SetItemAsStringAsync("accessToken", authResponse.AccessToken);
        await _localStorage.SetItemAsStringAsync("refreshToken", authResponse.RefreshToken);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return authResponse.AccessToken;
    }

    private static IEnumerable<Claim>? ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            return token.Claims;
        }
        catch
        {
            return null;
        }
    }
}
