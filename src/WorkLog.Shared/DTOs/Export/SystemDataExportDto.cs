namespace WorkLog.Shared.DTOs.Export;

/// <summary>
/// 系統管理資料匯出物件（參照資料）
/// </summary>
public class SystemDataExportDto
{
    /// <summary>匯出格式版本號</summary>
    public string Version { get; set; } = "1.0";

    /// <summary>匯出類型</summary>
    public string ExportType { get; set; } = "SystemData";

    /// <summary>匯出時間 (UTC)</summary>
    public DateTime ExportedAt { get; set; }

    /// <summary>匯出使用者名稱</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>參照資料（系統管理資料）</summary>
    public ReferenceDataExportDto ReferenceData { get; set; } = new();
}
