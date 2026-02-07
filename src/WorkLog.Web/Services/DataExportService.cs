using System.Net.Http.Json;
using WorkLog.Shared.DTOs.Export;

namespace WorkLog.Web.Services;

/// <summary>
/// 資料匯出匯入服務
/// </summary>
public class DataExportService
{
    private readonly HttpClient _httpClient;

    public DataExportService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// 匯出資料
    /// </summary>
    public async Task<byte[]?> ExportDataAsync(DateTime? startDate, DateTime? endDate, bool includeAttachments)
    {
        var queryString = $"?includeAttachments={includeAttachments}";
        
        if (startDate.HasValue)
            queryString += $"&startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}";
        
        if (endDate.HasValue)
            queryString += $"&endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}";

        var response = await _httpClient.GetAsync($"api/dataexport/export{queryString}");
        
        return response.IsSuccessStatusCode
            ? await response.Content.ReadAsByteArrayAsync()
            : null;
    }

    /// <summary>
    /// 驗證匯入檔案
    /// </summary>
    public async Task<DataImportResultDto?> ValidateImportFileAsync(byte[] fileBytes)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
        content.Add(fileContent, "file", "import.zip");

        var response = await _httpClient.PostAsync("api/dataexport/validate", content);
        
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<DataImportResultDto>()
            : null;
    }

    /// <summary>
    /// 匯入資料
    /// </summary>
    public async Task<DataImportResultDto?> ImportDataAsync(byte[] fileBytes)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
        content.Add(fileContent, "file", "import.zip");

        var response = await _httpClient.PostAsync("api/dataexport/import", content);
        
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<DataImportResultDto>()
            : null;
    }

    /// <summary>
    /// 匯出工作紀錄資料（工作紀錄 + 待辦事項）
    /// </summary>
    public async Task<byte[]?> ExportWorkLogDataAsync(DateTime? startDate, DateTime? endDate, bool includeAttachments)
    {
        var queryString = $"?includeAttachments={includeAttachments}";
        
        if (startDate.HasValue)
            queryString += $"&startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}";
        
        if (endDate.HasValue)
            queryString += $"&endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}";

        var response = await _httpClient.GetAsync($"api/dataexport/export-worklog{queryString}");
        
        return response.IsSuccessStatusCode
            ? await response.Content.ReadAsByteArrayAsync()
            : null;
    }

    /// <summary>
    /// 匯出系統管理資料（參照資料）
    /// </summary>
    public async Task<byte[]?> ExportSystemDataAsync()
    {
        var response = await _httpClient.GetAsync("api/dataexport/export-system");
        
        return response.IsSuccessStatusCode
            ? await response.Content.ReadAsByteArrayAsync()
            : null;
    }

    /// <summary>
    /// 驗證工作紀錄資料匯入檔案
    /// </summary>
    public async Task<DataImportResultDto?> ValidateWorkLogImportFileAsync(byte[] fileBytes)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
        content.Add(fileContent, "file", "worklog-import.zip");

        var response = await _httpClient.PostAsync("api/dataexport/validate-worklog", content);
        
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<DataImportResultDto>()
            : null;
    }

    /// <summary>
    /// 匯入工作紀錄資料
    /// </summary>
    public async Task<DataImportResultDto?> ImportWorkLogDataAsync(byte[] fileBytes)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
        content.Add(fileContent, "file", "worklog-import.zip");

        var response = await _httpClient.PostAsync("api/dataexport/import-worklog", content);
        
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<DataImportResultDto>()
            : null;
    }

    /// <summary>
    /// 驗證系統管理資料匯入檔案
    /// </summary>
    public async Task<DataImportResultDto?> ValidateSystemDataImportFileAsync(byte[] fileBytes)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
        content.Add(fileContent, "file", "system-import.zip");

        var response = await _httpClient.PostAsync("api/dataexport/validate-system", content);
        
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<DataImportResultDto>()
            : null;
    }

    /// <summary>
    /// 匯入系統管理資料
    /// </summary>
    public async Task<DataImportResultDto?> ImportSystemDataAsync(byte[] fileBytes)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
        content.Add(fileContent, "file", "system-import.zip");

        var response = await _httpClient.PostAsync("api/dataexport/import-system", content);
        
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<DataImportResultDto>()
            : null;
    }
}
