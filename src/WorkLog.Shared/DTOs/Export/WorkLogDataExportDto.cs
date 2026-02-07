namespace WorkLog.Shared.DTOs.Export;

/// <summary>
/// 工作紀錄資料匯出物件（工作紀錄 + 待辦事項）
/// </summary>
public class WorkLogDataExportDto
{
    /// <summary>匯出格式版本號</summary>
    public string Version { get; set; } = "1.0";

    /// <summary>匯出類型</summary>
    public string ExportType { get; set; } = "WorkLogData";

    /// <summary>匯出時間 (UTC)</summary>
    public DateTime ExportedAt { get; set; }

    /// <summary>匯出使用者名稱</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>匯出日期範圍 - 起始日</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>匯出日期範圍 - 結束日</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>是否包含附件</summary>
    public bool IncludesAttachments { get; set; }

    /// <summary>工作紀錄</summary>
    public List<WorkLogExportDto> WorkLogs { get; set; } = new();

    /// <summary>待辦事項</summary>
    public List<TodoExportDto> Todos { get; set; } = new();
}
