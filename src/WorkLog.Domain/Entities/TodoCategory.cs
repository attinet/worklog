namespace WorkLog.Domain.Entities;

/// <summary>
/// 待辦事項分類實體
/// </summary>
public class TodoCategory
{
    public int Id { get; set; }

    /// <summary>分類名稱</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>顏色代碼（HEX格式）</summary>
    public string ColorCode { get; set; } = "#808080";

    /// <summary>圖示名稱</summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>是否啟用</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>排序順序</summary>
    public int SortOrder { get; set; }

    // Navigation properties
    public ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
}
