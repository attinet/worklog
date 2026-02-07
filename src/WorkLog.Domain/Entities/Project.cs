namespace WorkLog.Domain.Entities;

/// <summary>
/// 專案名稱實體
/// </summary>
public class Project
{
    public int Id { get; set; }

    /// <summary>專案名稱</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>是否啟用</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>排序順序</summary>
    public int SortOrder { get; set; }

    // Navigation properties
    public ICollection<WorkLogEntry> WorkLogEntries { get; set; } = new List<WorkLogEntry>();
}
