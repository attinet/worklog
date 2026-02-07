using WorkLog.Domain.Entities;
using WorkLog.Domain.Enums;

namespace WorkLog.Domain.Interfaces;

/// <summary>
/// 待辦事項查詢篩選參數
/// </summary>
public class TodoQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public TodoStatus? Status { get; set; }
    public TodoPriority? Priority { get; set; }
    public int? CategoryId { get; set; }
    public string? Keyword { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public string SortBy { get; set; } = "DueDate";
    public bool IsDescending { get; set; } = false;
    public int UserId { get; set; }
}

/// <summary>
/// 待辦事項儀表板摘要
/// </summary>
public class TodoDashboardSummary
{
    public int PendingCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int DueTodayCount { get; set; }
    public int DueThisWeekCount { get; set; }
    public int OverdueCount { get; set; }
    public IReadOnlyList<TodoItem> DueTodayItems { get; set; } = [];
    public IReadOnlyList<TodoItem> OverdueItems { get; set; } = [];
}

/// <summary>
/// 待辦事項 Repository 介面
/// </summary>
public interface ITodoRepository
{
    // 主要待辦事項操作
    Task<TodoItem?> GetByIdAsync(int id, int userId);
    Task<PagedResult<TodoItem>> GetAllAsync(TodoQueryParameters parameters);
    Task<TodoDashboardSummary> GetDashboardSummaryAsync(int userId);
    Task<TodoItem> CreateAsync(TodoItem todoItem);
    Task UpdateAsync(TodoItem todoItem);
    Task DeleteAsync(int id, int userId);

    // 子任務操作
    Task<TodoSubTask> AddSubTaskAsync(TodoSubTask subTask);
    Task UpdateSubTaskAsync(TodoSubTask subTask);
    Task DeleteSubTaskAsync(int id, int userId);

    // 附件操作
    Task<TodoAttachment> AddAttachmentAsync(TodoAttachment attachment);
    Task<TodoAttachment?> GetAttachmentAsync(int id, int userId);
    Task DeleteAttachmentAsync(int id, int userId);

    // 評論操作
    Task<TodoComment> AddCommentAsync(TodoComment comment);
    Task UpdateCommentAsync(TodoComment comment);
    Task DeleteCommentAsync(int id, int userId);
}
