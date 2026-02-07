namespace WorkLog.Domain.Entities;

/// <summary>
/// 工作類型實體
/// </summary>
public class WorkType
{
    public int Id { get; set; }

    /// <summary>工作類型名稱</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>是否啟用</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>排序順序</summary>
    public int SortOrder { get; set; }

    // Navigation properties
    public ICollection<WorkLogWorkType> WorkLogWorkTypes { get; set; } = new List<WorkLogWorkType>();
}
