namespace WorkLog.Domain.Entities;

/// <summary>
/// 業管單位實體
/// </summary>
public class Department
{
    public int Id { get; set; }

    /// <summary>業管單位名稱</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>是否啟用</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>排序順序</summary>
    public int SortOrder { get; set; }

    // Navigation properties
    public ICollection<WorkLogDepartment> WorkLogDepartments { get; set; } = new List<WorkLogDepartment>();
}
