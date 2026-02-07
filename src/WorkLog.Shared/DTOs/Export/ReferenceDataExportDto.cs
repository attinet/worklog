namespace WorkLog.Shared.DTOs.Export;

/// <summary>
/// 參照資料匯出
/// </summary>
public class ReferenceDataExportDto
{
    public List<ReferenceItemDto> Projects { get; set; } = new();
    public List<ReferenceItemDto> Departments { get; set; } = new();
    public List<ReferenceItemDto> WorkTypes { get; set; } = new();
    public List<ReferenceItemDto> ProcessStatuses { get; set; } = new();
    public List<TodoCategoryReferenceDto> TodoCategories { get; set; } = new();
}

/// <summary>
/// 基本參照項目
/// </summary>
public class ReferenceItemDto
{
    public int OriginalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// 待辦分類參照項目
/// </summary>
public class TodoCategoryReferenceDto : ReferenceItemDto
{
    public string? ColorCode { get; set; }
    public string? Icon { get; set; }
}
