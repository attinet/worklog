using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkLog.Domain.Entities;
using WorkLog.Infrastructure.Data;
using WorkLog.Shared.DTOs.Export;
using WorkLog.Shared.Interfaces;

namespace WorkLog.Infrastructure.Services;

/// <summary>
/// 資料匯出服務實作
/// </summary>
public class DataExportService : IDataExportService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataExportService> _logger;

    public DataExportService(AppDbContext context, ILogger<DataExportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DataExportDto> ExportDataAsync(int userId, DateTime? startDate, DateTime? endDate, bool includeAttachments)
    {
        _logger.LogInformation("開始匯出使用者 {UserId} 的資料 (包含附件: {IncludeAttachments})", userId, includeAttachments);

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"找不到使用者 ID: {userId}");
        }

        var exportData = new DataExportDto
        {
            ExportedAt = DateTime.UtcNow,
            Username = user.Username,
            StartDate = startDate,
            EndDate = endDate,
            IncludesAttachments = includeAttachments
        };

        // 匯出參照資料
        exportData.ReferenceData = await ExportReferenceDataAsync();

        // 匯出工作紀錄
        exportData.WorkLogs = await ExportWorkLogsAsync(userId, startDate, endDate);

        // 匯出待辦事項
        exportData.Todos = await ExportTodosAsync(userId, startDate, endDate, includeAttachments);

        _logger.LogInformation("匯出完成: {WorkLogCount} 筆工作紀錄, {TodoCount} 筆待辦事項", 
            exportData.WorkLogs.Count, exportData.Todos.Count);

        return exportData;
    }

    private async Task<ReferenceDataExportDto> ExportReferenceDataAsync()
    {
        var referenceData = new ReferenceDataExportDto
        {
            Projects = await _context.Projects
                .Select(p => new ReferenceItemDto
                {
                    OriginalId = p.Id,
                    Name = p.Name,
                    IsActive = p.IsActive,
                    SortOrder = p.SortOrder
                })
                .ToListAsync(),

            Departments = await _context.Departments
                .Select(d => new ReferenceItemDto
                {
                    OriginalId = d.Id,
                    Name = d.Name,
                    IsActive = d.IsActive,
                    SortOrder = d.SortOrder
                })
                .ToListAsync(),

            WorkTypes = await _context.WorkTypes
                .Select(w => new ReferenceItemDto
                {
                    OriginalId = w.Id,
                    Name = w.Name,
                    IsActive = w.IsActive,
                    SortOrder = w.SortOrder
                })
                .ToListAsync(),

            ProcessStatuses = await _context.ProcessStatuses
                .Select(p => new ReferenceItemDto
                {
                    OriginalId = p.Id,
                    Name = p.Name,
                    IsActive = p.IsActive,
                    SortOrder = p.SortOrder
                })
                .ToListAsync(),

            TodoCategories = await _context.TodoCategories
                .Select(c => new TodoCategoryReferenceDto
                {
                    OriginalId = c.Id,
                    Name = c.Name,
                    IsActive = c.IsActive,
                    SortOrder = c.SortOrder,
                    ColorCode = c.ColorCode,
                    Icon = c.Icon
                })
                .ToListAsync()
        };

        return referenceData;
    }

    private async Task<List<WorkLogExportDto>> ExportWorkLogsAsync(int userId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.WorkLogEntries
            .Include(w => w.Project)
            .Include(w => w.ProcessStatus)
            .Include(w => w.WorkLogDepartments).ThenInclude(wd => wd.Department)
            .Include(w => w.WorkLogWorkTypes).ThenInclude(wt => wt.WorkType)
            .Where(w => w.UserId == userId);

        if (startDate.HasValue)
        {
            var startDateOnly = DateOnly.FromDateTime(startDate.Value);
            query = query.Where(w => w.RecordDate >= startDateOnly);
        }

        if (endDate.HasValue)
        {
            var endDateOnly = DateOnly.FromDateTime(endDate.Value);
            query = query.Where(w => w.RecordDate <= endDateOnly);
        }

        var workLogs = await query
            .OrderBy(w => w.RecordDate)
            .AsNoTracking()
            .ToListAsync();

        return workLogs.Select(w => new WorkLogExportDto
        {
            OriginalId = w.Id,
            Title = w.Title,
            Content = w.Content,
            RecordDate = w.RecordDate,
            WorkHours = w.WorkHours,
            CreatedAt = w.CreatedAt,
            UpdatedAt = w.UpdatedAt,
            ProjectId = w.ProjectId,
            ProjectName = w.Project.Name,
            ProcessStatusId = w.ProcessStatusId,
            ProcessStatusName = w.ProcessStatus.Name,
            Departments = w.WorkLogDepartments.Select(wd => new ReferenceItemDto
            {
                OriginalId = wd.Department.Id,
                Name = wd.Department.Name,
                IsActive = wd.Department.IsActive,
                SortOrder = wd.Department.SortOrder
            }).ToList(),
            WorkTypes = w.WorkLogWorkTypes.Select(wt => new ReferenceItemDto
            {
                OriginalId = wt.WorkType.Id,
                Name = wt.WorkType.Name,
                IsActive = wt.WorkType.IsActive,
                SortOrder = wt.WorkType.SortOrder
            }).ToList()
        }).ToList();
    }

    private async Task<List<TodoExportDto>> ExportTodosAsync(int userId, DateTime? startDate, DateTime? endDate, bool includeAttachments)
    {
        var query = _context.TodoItems
            .Include(t => t.Category)
            .Include(t => t.SubTasks)
            .Include(t => t.Comments).ThenInclude(c => c.User)
            .Include(t => t.Attachments)
            .Where(t => t.UserId == userId);

        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= endDate.Value);
        }

        var todos = await query
            .OrderBy(t => t.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return todos.Select(t => new TodoExportDto
        {
            OriginalId = t.Id,
            Title = t.Title,
            Description = t.Description,
            DueDate = t.DueDate,
            Status = t.Status,
            Priority = t.Priority,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            CompletedAt = t.CompletedAt,
            CategoryId = t.CategoryId,
            CategoryName = t.Category?.Name,
            SubTasks = t.SubTasks.Select(st => new TodoSubTaskExportDto
            {
                OriginalId = st.Id,
                Title = st.Title,
                IsCompleted = st.IsCompleted,
                SortOrder = st.SortOrder,
                CreatedAt = st.CreatedAt
            }).ToList(),
            Comments = t.Comments.Select(c => new TodoCommentExportDto
            {
                OriginalId = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Username = c.User.Username
            }).ToList(),
            Attachments = t.Attachments.Select(a => new TodoAttachmentExportDto
            {
                OriginalId = a.Id,
                FileName = a.FileName,
                FileSize = a.FileSize,
                ContentType = a.ContentType,
                UploadedAt = a.UploadedAt,
                FilePath = includeAttachments ? $"attachments/{a.Id}_{a.FileName}" : null
            }).ToList()
        }).ToList();
    }

    public async Task<byte[]> GenerateZipFileAsync(DataExportDto exportData, Dictionary<int, byte[]>? attachmentFiles)
    {
        _logger.LogInformation("產生 ZIP 檔案...");

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // 加入 data.json
            var dataJsonEntry = archive.CreateEntry("data.json", CompressionLevel.Optimal);
            using (var entryStream = dataJsonEntry.Open())
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                await JsonSerializer.SerializeAsync(entryStream, exportData, jsonOptions);
            }

            // 加入附件檔案
            if (attachmentFiles != null && attachmentFiles.Count > 0)
            {
                _logger.LogInformation("加入 {AttachmentCount} 個附件到 ZIP", attachmentFiles.Count);
                
                foreach (var todo in exportData.Todos)
                {
                    foreach (var attachment in todo.Attachments.Where(a => a.FilePath != null))
                    {
                        if (attachmentFiles.TryGetValue(attachment.OriginalId, out var fileData))
                        {
                            var attachmentEntry = archive.CreateEntry(attachment.FilePath!, CompressionLevel.Optimal);
                            using var attachmentStream = attachmentEntry.Open();
                            await attachmentStream.WriteAsync(fileData);
                        }
                    }
                }
            }
        }

        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }

    public async Task<WorkLogDataExportDto> ExportWorkLogDataAsync(int userId, DateTime? startDate, DateTime? endDate, bool includeAttachments)
    {
        _logger.LogInformation("開始匯出使用者 {UserId} 的工作紀錄資料 (包含附件: {IncludeAttachments})", userId, includeAttachments);

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"找不到使用者 ID: {userId}");
        }

        var exportData = new WorkLogDataExportDto
        {
            ExportedAt = DateTime.UtcNow,
            Username = user.Username,
            StartDate = startDate,
            EndDate = endDate,
            IncludesAttachments = includeAttachments
        };

        // 匯出工作紀錄
        exportData.WorkLogs = await ExportWorkLogsAsync(userId, startDate, endDate);

        // 匯出待辦事項
        exportData.Todos = await ExportTodosAsync(userId, startDate, endDate, includeAttachments);

        _logger.LogInformation("工作紀錄資料匯出完成: {WorkLogCount} 筆工作紀錄, {TodoCount} 筆待辦事項", 
            exportData.WorkLogs.Count, exportData.Todos.Count);

        return exportData;
    }

    public async Task<SystemDataExportDto> ExportSystemDataAsync(int userId)
    {
        _logger.LogInformation("開始匯出系統管理資料");

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"找不到使用者 ID: {userId}");
        }

        var exportData = new SystemDataExportDto
        {
            ExportedAt = DateTime.UtcNow,
            Username = user.Username,
            ReferenceData = await ExportReferenceDataAsync()
        };

        _logger.LogInformation("系統管理資料匯出完成");

        return exportData;
    }

    public async Task<byte[]> GenerateWorkLogZipFileAsync(WorkLogDataExportDto exportData, Dictionary<int, byte[]>? attachmentFiles)
    {
        _logger.LogInformation("產生工作紀錄資料 ZIP 檔案...");

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // 加入 data.json
            var dataJsonEntry = archive.CreateEntry("data.json", CompressionLevel.Optimal);
            using (var entryStream = dataJsonEntry.Open())
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                await JsonSerializer.SerializeAsync(entryStream, exportData, jsonOptions);
            }

            // 加入附件檔案
            if (attachmentFiles != null && attachmentFiles.Count > 0)
            {
                _logger.LogInformation("加入 {AttachmentCount} 個附件到 ZIP", attachmentFiles.Count);
                
                foreach (var todo in exportData.Todos)
                {
                    foreach (var attachment in todo.Attachments.Where(a => a.FilePath != null))
                    {
                        if (attachmentFiles.TryGetValue(attachment.OriginalId, out var fileData))
                        {
                            var attachmentEntry = archive.CreateEntry(attachment.FilePath!, CompressionLevel.Optimal);
                            using var attachmentStream = attachmentEntry.Open();
                            await attachmentStream.WriteAsync(fileData);
                        }
                    }
                }
            }
        }

        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }

    public async Task<byte[]> GenerateSystemDataZipFileAsync(SystemDataExportDto exportData)
    {
        _logger.LogInformation("產生系統管理資料 ZIP 檔案...");

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // 加入 data.json
            var dataJsonEntry = archive.CreateEntry("data.json", CompressionLevel.Optimal);
            using (var entryStream = dataJsonEntry.Open())
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                await JsonSerializer.SerializeAsync(entryStream, exportData, jsonOptions);
            }
        }

        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }
}
