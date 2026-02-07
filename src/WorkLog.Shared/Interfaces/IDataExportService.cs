using WorkLog.Shared.DTOs.Export;

namespace WorkLog.Shared.Interfaces;

/// <summary>
/// 資料匯出服務介面
/// </summary>
public interface IDataExportService
{
    /// <summary>
    /// 匯出使用者資料
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="startDate">開始日期 (選填)</param>
    /// <param name="endDate">結束日期 (選填)</param>
    /// <param name="includeAttachments">是否包含附件</param>
    /// <returns>匯出資料物件</returns>
    Task<DataExportDto> ExportDataAsync(int userId, DateTime? startDate, DateTime? endDate, bool includeAttachments);

    /// <summary>
    /// 產生 ZIP 檔案
    /// </summary>
    /// <param name="exportData">匯出資料</param>
    /// <param name="attachmentFiles">附件檔案資料 (OriginalId -> FileData)</param>
    /// <returns>ZIP 檔案位元組陣列</returns>
    Task<byte[]> GenerateZipFileAsync(DataExportDto exportData, Dictionary<int, byte[]>? attachmentFiles);
}
