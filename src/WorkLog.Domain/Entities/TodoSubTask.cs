namespace WorkLog.Domain.Entities;

/// <summary>
/// 待辦事項子任務實體
/// </summary>
public class TodoSubTask
{
    public int Id { get; set; }

    /// <summary>子任務標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>是否完成</summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>排序順序</summary>
    public int SortOrder { get; set; }

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public int TodoItemId { get; set; }

    // Navigation properties
    public TodoItem TodoItem { get; set; } = null!;
}
