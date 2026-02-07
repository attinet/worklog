namespace WorkLog.Domain.Entities;

/// <summary>
/// 待辦事項評論實體
/// </summary>
public class TodoComment
{
    public int Id { get; set; }

    /// <summary>評論內容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>最後更新時間</summary>
    public DateTime? UpdatedAt { get; set; }

    // Foreign keys
    public int TodoItemId { get; set; }
    public int UserId { get; set; }

    // Navigation properties
    public TodoItem TodoItem { get; set; } = null!;
    public User User { get; set; } = null!;
}
