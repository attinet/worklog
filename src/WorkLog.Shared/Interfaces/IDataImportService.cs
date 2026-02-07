using WorkLog.Shared.DTOs.Export;

namespace WorkLog.Shared.Interfaces;

/// <summary>
/// 資料匯入服務介面
/// </summary>
public interface IDataImportService
{
    /// <summary>
    /// 驗證匯入檔案
    /// </summary>
    /// <param name="zipFileBytes">ZIP 檔案位元組陣列</param>
    /// <returns>驗證結果</returns>
    Task<DataImportResultDto> ValidateImportFileAsync(byte[] zipFileBytes);

    /// <summary>
    /// 匯入資料
    /// </summary>
    /// <param name="userId">目標使用者 ID</param>
    /// <param name="zipFileBytes">ZIP 檔案位元組陣列</param>
    /// <returns>匯入結果</returns>
    Task<DataImportResultDto> ImportDataAsync(int userId, byte[] zipFileBytes);
}
