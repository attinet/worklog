using System.ComponentModel.DataAnnotations;

namespace WorkLog.Shared.DTOs.Admin;

/// <summary>
/// 使用者管理清單項目
/// </summary>
public class UserListItemDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 變更使用者角色請求
/// </summary>
public class ChangeRoleRequest
{
    [Required(ErrorMessage = "角色必填")]
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// 新增使用者請求
/// </summary>
public class CreateUserRequest
{
    [Required(ErrorMessage = "使用者名稱必填")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "使用者名稱長度需在 3-50 個字元之間")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "電子郵件必填")]
    [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "密碼必填")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度至少需要 6 個字元")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "角色必填")]
    public string Role { get; set; } = "User";
}

/// <summary>
/// 修改密碼請求
/// </summary>
public class ChangePasswordRequest
{
    [Required(ErrorMessage = "新密碼必填")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度至少需要 6 個字元")]
    public string NewPassword { get; set; } = string.Empty;
}
