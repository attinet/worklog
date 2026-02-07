using System.ComponentModel.DataAnnotations;

namespace WorkLog.Shared.DTOs.Auth;

/// <summary>
/// 註冊請求
/// </summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "使用者名稱為必填")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "使用者名稱長度需介於 3-50 字元")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "電子郵件為必填")]
    [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "密碼為必填")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度至少需 6 個字元")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "確認密碼為必填")]
    [Compare(nameof(Password), ErrorMessage = "確認密碼與密碼不一致")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// 登入請求
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "使用者名稱為必填")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密碼為必填")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 認證回應（含 Token）
/// </summary>
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo User { get; set; } = null!;
}

/// <summary>
/// 使用者基本資訊
/// </summary>
public class UserInfo
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// 刷新 Token 請求
/// </summary>
public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
