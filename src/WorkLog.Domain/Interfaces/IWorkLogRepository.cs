using WorkLog.Domain.Entities;

namespace WorkLog.Domain.Interfaces;

/// <summary>
/// 工作紀錄查詢篩選參數
/// </summary>
public class WorkLogQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? ProjectId { get; set; }
    public int? ProcessStatusId { get; set; }
    public string? Keyword { get; set; }
    public int UserId { get; set; }
}

/// <summary>
/// 分頁結果
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// 工作紀錄 Repository 介面
/// </summary>
public interface IWorkLogRepository
{
    Task<WorkLogEntry?> GetByIdAsync(int id);
    Task<WorkLogEntry?> GetByIdWithDetailsAsync(int id);
    Task<PagedResult<WorkLogEntry>> GetPagedAsync(WorkLogQueryParameters parameters);
    Task<WorkLogEntry> CreateAsync(WorkLogEntry entry);
    Task UpdateAsync(WorkLogEntry entry);
    Task DeleteAsync(int id);
}
