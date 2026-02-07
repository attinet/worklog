namespace WorkLog.Shared.DTOs.Export;

/// <summary>
/// 工作紀錄匯出物件
/// </summary>
public class WorkLogExportDto
{
    public int OriginalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateOnly RecordDate { get; set; }
    public decimal WorkHours { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // 參照資料 (ID + Name 以便匯入時比對)
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;

    public int ProcessStatusId { get; set; }
    public string ProcessStatusName { get; set; } = string.Empty;

    // 多對多關聯
    public List<ReferenceItemDto> Departments { get; set; } = new();
    public List<ReferenceItemDto> WorkTypes { get; set; } = new();
}
