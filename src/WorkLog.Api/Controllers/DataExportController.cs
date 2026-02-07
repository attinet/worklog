using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkLog.Infrastructure.Data;
using WorkLog.Shared.DTOs.Export;
using WorkLog.Shared.Interfaces;

namespace WorkLog.Api.Controllers;

/// <summary>
/// 資料匯出匯入 API
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DataExportController : ControllerBase
{
    private readonly IDataExportService _exportService;
    private readonly IDataImportService _importService;
    private readonly AppDbContext _context;
    private readonly ILogger<DataExportController> _logger;

    public DataExportController(
        IDataExportService exportService,
        IDataImportService importService,
        AppDbContext context,
        ILogger<DataExportController> logger)
    {
        _exportService = exportService;
        _importService = importService;
        _context = context;
        _logger = logger;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>
    /// 匯出使用者資料為 ZIP 檔案
    /// </summary>
    /// <param name="startDate">開始日期 (選填)</param>
    /// <param name="endDate">結束日期 (選填)</param>
    /// <param name="includeAttachments">是否包含附件檔案</param>
    /// <returns>ZIP 檔案</returns>
    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportData(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] bool includeAttachments = false)
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation("使用者 {UserId} 請求匯出資料", userId);

            // 匯出資料
            var exportData = await _exportService.ExportDataAsync(userId, startDate, endDate, includeAttachments);

            // 如果包含附件，需要取得附件檔案資料
            Dictionary<int, byte[]>? attachmentFiles = null;
            if (includeAttachments)
            {
                attachmentFiles = new Dictionary<int, byte[]>();
                
                // 取得所有待辦事項的附件資料
                var attachmentIds = exportData.Todos
                    .SelectMany(t => t.Attachments)
                    .Select(a => a.OriginalId)
                    .ToList();

                var attachments = await _context.TodoAttachments
                    .Where(a => attachmentIds.Contains(a.Id))
                    .Select(a => new { a.Id, a.FileData })
                    .ToListAsync();

                foreach (var attachment in attachments)
                {
                    attachmentFiles[attachment.Id] = attachment.FileData;
                }
            }

            // 產生 ZIP 檔案
            var zipBytes = await _exportService.GenerateZipFileAsync(exportData, attachmentFiles);

            var fileName = $"worklog-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip";
            return File(zipBytes, "application/zip", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯出資料時發生錯誤");
            return BadRequest(new { error = "匯出失敗", message = ex.Message });
        }
    }

    /// <summary>
    /// 驗證匯入檔案
    /// </summary>
    /// <param name="file">ZIP 檔案</param>
    /// <returns>驗證結果</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(DataImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateImportFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "請提供有效的 ZIP 檔案" });
        }

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "只接受 ZIP 檔案" });
        }

        try
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var zipBytes = memoryStream.ToArray();

            var result = await _importService.ValidateImportFileAsync(zipBytes);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "驗證匯入檔案時發生錯誤");
            return BadRequest(new { error = "驗證失敗", message = ex.Message });
        }
    }

    /// <summary>
    /// 匯入資料
    /// </summary>
    /// <param name="file">ZIP 檔案</param>
    /// <returns>匯入結果</returns>
    [HttpPost("import")]
    [ProducesResponseType(typeof(DataImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(52428800)] // 50 MB
    public async Task<IActionResult> ImportData(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "請提供有效的 ZIP 檔案" });
        }

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "只接受 ZIP 檔案" });
        }

        // 檢查檔案大小 (50 MB)
        if (file.Length > 52428800)
        {
            return BadRequest(new { error = "檔案大小不可超過 50 MB" });
        }

        try
        {
            var userId = GetUserId();
            _logger.LogInformation("使用者 {UserId} 請求匯入資料, 檔案大小: {FileSize} bytes", userId, file.Length);

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var zipBytes = memoryStream.ToArray();

            var result = await _importService.ImportDataAsync(userId, zipBytes);
            
            if (result.Success)
            {
                _logger.LogInformation("資料匯入成功");
            }
            else
            {
                _logger.LogWarning("資料匯入失敗: {Errors}", string.Join(", ", result.Errors));
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯入資料時發生錯誤");
            return BadRequest(new { error = "匯入失敗", message = ex.Message });
        }
    }

    /// <summary>
    /// 匯出工作紀錄資料為 ZIP 檔案（工作紀錄 + 待辦事項）
    /// </summary>
    /// <param name="startDate">開始日期 (選填)</param>
    /// <param name="endDate">結束日期 (選填)</param>
    /// <param name="includeAttachments">是否包含附件檔案</param>
    /// <returns>ZIP 檔案</returns>
    [HttpGet("export-worklog")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportWorkLogData(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] bool includeAttachments = false)
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation("使用者 {UserId} 請求匯出工作紀錄資料", userId);

            // 匯出資料
            var exportData = await _exportService.ExportWorkLogDataAsync(userId, startDate, endDate, includeAttachments);

            // 如果包含附件，需要取得附件檔案資料
            Dictionary<int, byte[]>? attachmentFiles = null;
            if (includeAttachments)
            {
                attachmentFiles = new Dictionary<int, byte[]>();
                
                // 取得所有待辦事項的附件資料
                var attachmentIds = exportData.Todos
                    .SelectMany(t => t.Attachments)
                    .Select(a => a.OriginalId)
                    .ToList();

                var attachments = await _context.TodoAttachments
                    .Where(a => attachmentIds.Contains(a.Id))
                    .Select(a => new { a.Id, a.FileData })
                    .ToListAsync();

                foreach (var attachment in attachments)
                {
                    attachmentFiles[attachment.Id] = attachment.FileData;
                }
            }

            // 產生 ZIP 檔案
            var zipBytes = await _exportService.GenerateWorkLogZipFileAsync(exportData, attachmentFiles);

            var fileName = $"worklog-data-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip";
            return File(zipBytes, "application/zip", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯出工作紀錄資料時發生錯誤");
            return BadRequest(new { error = "匯出失敗", message = ex.Message });
        }
    }

    /// <summary>
    /// 匯出系統管理資料為 ZIP 檔案（參照資料）
    /// </summary>
    /// <returns>ZIP 檔案</returns>
    [HttpGet("export-system")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportSystemData()
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation("使用者 {UserId} 請求匯出系統管理資料", userId);

            // 匯出資料
            var exportData = await _exportService.ExportSystemDataAsync(userId);

            // 產生 ZIP 檔案
            var zipBytes = await _exportService.GenerateSystemDataZipFileAsync(exportData);

            var fileName = $"system-data-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip";
            return File(zipBytes, "application/zip", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯出系統管理資料時發生錯誤");
            return BadRequest(new { error = "匯出失敗", message = ex.Message });
        }
    }

    /// <summary>
    /// 驗證工作紀錄資料匯入檔案
    /// </summary>
    /// <param name="file">ZIP 檔案</param>
    /// <returns>驗證結果</returns>
    [HttpPost("validate-worklog")]
    [ProducesResponseType(typeof(DataImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateWorkLogImportFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "請提供有效的 ZIP 檔案" });
        }

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "只接受 ZIP 檔案" });
        }

        try
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var zipBytes = memoryStream.ToArray();

            var result = await _importService.ValidateWorkLogImportFileAsync(zipBytes);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "驗證工作紀錄匯入檔案時發生錯誤");
            return BadRequest(new { error = "驗證失敗", message = ex.Message });
        }
    }

    /// <summary>
    /// 匯入工作紀錄資料
    /// </summary>
    /// <param name="file">ZIP 檔案</param>
    /// <returns>匯入結果</returns>
    [HttpPost("import-worklog")]
    [ProducesResponseType(typeof(DataImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(52428800)] // 50 MB
    public async Task<IActionResult> ImportWorkLogData(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "請提供有效的 ZIP 檔案" });
        }

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "只接受 ZIP 檔案" });
        }

        // 檢查檔案大小 (50 MB)
        if (file.Length > 52428800)
        {
            return BadRequest(new { error = "檔案大小不可超過 50 MB" });
        }

        try
        {
            var userId = GetUserId();
            _logger.LogInformation("使用者 {UserId} 請求匯入工作紀錄資料, 檔案大小: {FileSize} bytes", userId, file.Length);

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var zipBytes = memoryStream.ToArray();

            var result = await _importService.ImportWorkLogDataAsync(userId, zipBytes);
            
            if (result.Success)
            {
                _logger.LogInformation("工作紀錄資料匯入成功");
            }
            else
            {
                _logger.LogWarning("工作紀錄資料匯入失敗: {Errors}", string.Join(", ", result.Errors));
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯入工作紀錄資料時發生錯誤");
            return BadRequest(new { error = "匯入失敗", message = ex.Message });
        }
    }

    /// <summary>
    /// 驗證系統管理資料匯入檔案
    /// </summary>
    /// <param name="file">ZIP 檔案</param>
    /// <returns>驗證結果</returns>
    [HttpPost("validate-system")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DataImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateSystemDataImportFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "請提供有效的 ZIP 檔案" });
        }

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "只接受 ZIP 檔案" });
        }

        try
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var zipBytes = memoryStream.ToArray();

            var result = await _importService.ValidateSystemDataImportFileAsync(zipBytes);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "驗證系統管理資料匯入檔案時發生錯誤");
            return BadRequest(new { error = "驗證失敗", message = ex.Message });
        }
    }

    /// <summary>
    /// 匯入系統管理資料（需要管理員權限）
    /// </summary>
    /// <param name="file">ZIP 檔案</param>
    /// <returns>匯入結果</returns>
    [HttpPost("import-system")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DataImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(10485760)] // 10 MB
    public async Task<IActionResult> ImportSystemData(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "請提供有效的 ZIP 檔案" });
        }

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "只接受 ZIP 檔案" });
        }

        // 檢查檔案大小 (10 MB)
        if (file.Length > 10485760)
        {
            return BadRequest(new { error = "檔案大小不可超過 10 MB" });
        }

        try
        {
            var userId = GetUserId();
            _logger.LogInformation("使用者 {UserId} 請求匯入系統管理資料, 檔案大小: {FileSize} bytes", userId, file.Length);

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var zipBytes = memoryStream.ToArray();

            var result = await _importService.ImportSystemDataAsync(userId, zipBytes);
            
            if (result.Success)
            {
                _logger.LogInformation("系統管理資料匯入成功");
            }
            else
            {
                _logger.LogWarning("系統管理資料匯入失敗: {Errors}", string.Join(", ", result.Errors));
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯入系統管理資料時發生錯誤");
            return BadRequest(new { error = "匯入失敗", message = ex.Message });
        }
    }
}
