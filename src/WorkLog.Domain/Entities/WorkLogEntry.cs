namespace WorkLog.Domain.Entities;

/// <summary>
/// 工作紀錄實體
/// </summary>
public class WorkLogEntry
{
    public int Id { get; set; }

    /// <summary>標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>內文</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>紀錄日期</summary>
    public DateOnly RecordDate { get; set; }

    /// <summary>工時（小時）</summary>
    public decimal WorkHours { get; set; }

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>最後更新時間</summary>
    public DateTime? UpdatedAt { get; set; }

    // Foreign keys
    public int UserId { get; set; }
    public int ProjectId { get; set; }
    public int ProcessStatusId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Project Project { get; set; } = null!;
    public ProcessStatus ProcessStatus { get; set; } = null!;
    public ICollection<WorkLogDepartment> WorkLogDepartments { get; set; } = new List<WorkLogDepartment>();
    public ICollection<WorkLogWorkType> WorkLogWorkTypes { get; set; } = new List<WorkLogWorkType>();
}
