using WorkLog.Domain.Enums;

namespace WorkLog.Domain.Entities;

/// <summary>
/// 使用者實體
/// </summary>
public class User
{
    public int Id { get; set; }

    /// <summary>使用者名稱（登入帳號）</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>電子郵件</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>密碼雜湊值</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>使用者角色</summary>
    public UserRole Role { get; set; } = UserRole.User;

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>最後更新時間</summary>
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<WorkLogEntry> WorkLogEntries { get; set; } = new List<WorkLogEntry>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
