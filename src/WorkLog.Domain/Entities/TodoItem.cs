using WorkLog.Domain.Enums;

namespace WorkLog.Domain.Entities;

/// <summary>
/// 待辦事項實體
/// </summary>
public class TodoItem
{
    public int Id { get; set; }

    /// <summary>標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>截止日期</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>狀態</summary>
    public TodoStatus Status { get; set; } = TodoStatus.Pending;

    /// <summary>優先順序</summary>
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>最後更新時間</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>完成時間</summary>
    public DateTime? CompletedAt { get; set; }

    // Foreign keys
    public int UserId { get; set; }
    public int? CategoryId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public TodoCategory? Category { get; set; }
    public ICollection<TodoSubTask> SubTasks { get; set; } = new List<TodoSubTask>();
    public ICollection<TodoAttachment> Attachments { get; set; } = new List<TodoAttachment>();
    public ICollection<TodoComment> Comments { get; set; } = new List<TodoComment>();
}
