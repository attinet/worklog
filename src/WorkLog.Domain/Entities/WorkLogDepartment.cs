namespace WorkLog.Domain.Entities;

/// <summary>
/// 工作紀錄與業管單位的多對多關聯表
/// </summary>
public class WorkLogDepartment
{
    public int WorkLogEntryId { get; set; }
    public int DepartmentId { get; set; }

    // Navigation properties
    public WorkLogEntry WorkLogEntry { get; set; } = null!;
    public Department Department { get; set; } = null!;
}
