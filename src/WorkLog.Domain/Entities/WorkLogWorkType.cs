namespace WorkLog.Domain.Entities;

/// <summary>
/// 工作紀錄與工作類型的多對多關聯表
/// </summary>
public class WorkLogWorkType
{
    public int WorkLogEntryId { get; set; }
    public int WorkTypeId { get; set; }

    // Navigation properties
    public WorkLogEntry WorkLogEntry { get; set; } = null!;
    public WorkType WorkType { get; set; } = null!;
}
