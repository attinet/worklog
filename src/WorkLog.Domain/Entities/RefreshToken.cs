namespace WorkLog.Domain.Entities;

/// <summary>
/// JWT Refresh Token 實體
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    /// <summary>Token 值</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>過期時間</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>是否已撤銷</summary>
    public bool IsRevoked { get; set; }

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public int UserId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
