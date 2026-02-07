using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using WorkLog.Domain.Entities;
using WorkLog.Infrastructure.Data;
using WorkLog.Shared.DTOs.Export;
using WorkLog.Shared.Interfaces;

namespace WorkLog.Infrastructure.Services;

/// <summary>
/// 資料匯入服務實作
/// </summary>
public class DataImportService : IDataImportService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataImportService> _logger;

    public DataImportService(AppDbContext context, ILogger<DataImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DataImportResultDto> ValidateImportFileAsync(byte[] zipFileBytes)
    {
        var result = new DataImportResultDto { Success = true };

        try
        {
            using var memoryStream = new MemoryStream(zipFileBytes);
            using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

            // 檢查 data.json 是否存在
            var dataEntry = archive.GetEntry("data.json");
            if (dataEntry == null)
            {
                result.Success = false;
                result.Errors.Add("ZIP 檔案中找不到 data.json");
                return result;
            }

            // 嘗試解析 JSON
            using var entryStream = dataEntry.Open();
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var exportData = await JsonSerializer.DeserializeAsync<DataExportDto>(entryStream, jsonOptions);

            if (exportData == null)
            {
                result.Success = false;
                result.Errors.Add("無法解析 data.json");
                return result;
            }

            // 檢查版本相容性
            if (exportData.Version != "1.0")
            {
                result.Warnings.Add($"匯出版本 {exportData.Version} 可能與當前版本不相容");
            }

            result.Message = $"驗證成功。包含 {exportData.WorkLogs.Count} 筆工作紀錄和 {exportData.Todos.Count} 筆待辦事項";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "驗證匯入檔案時發生錯誤");
            result.Success = false;
            result.Errors.Add($"驗證失敗: {ex.Message}");
        }

        return result;
    }

    public async Task<DataImportResultDto> ImportDataAsync(int userId, byte[] zipFileBytes)
    {
        var result = new DataImportResultDto { Success = false };

        // 先驗證
        var validationResult = await ValidateImportFileAsync(zipFileBytes);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        IDbContextTransaction? transaction = null;

        try
        {
            _logger.LogInformation("開始匯入資料到使用者 {UserId}", userId);

            // 解析資料
            using var memoryStream = new MemoryStream(zipFileBytes);
            using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

            var dataEntry = archive.GetEntry("data.json")!;
            DataExportDto exportData;
            
            using (var entryStream = dataEntry.Open())
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                exportData = (await JsonSerializer.DeserializeAsync<DataExportDto>(entryStream, jsonOptions))!;
            }

            // 開始交易
            transaction = await _context.Database.BeginTransactionAsync();

            // 準備附件資料字典
            var attachmentDataDict = new Dictionary<int, byte[]>();
            if (exportData.IncludesAttachments)
            {
                foreach (var todo in exportData.Todos)
                {
                    foreach (var attachment in todo.Attachments.Where(a => a.FilePath != null))
                    {
                        var attachmentEntry = archive.GetEntry(attachment.FilePath!);
                        if (attachmentEntry != null)
                        {
                            using var attachmentStream = attachmentEntry.Open();
                            using var ms = new MemoryStream();
                            await attachmentStream.CopyToAsync(ms);
                            attachmentDataDict[attachment.OriginalId] = ms.ToArray();
                        }
                    }
                }
            }

            // 1. 匯入參照資料
            var referenceIdMappings = await ImportReferenceDataAsync(exportData.ReferenceData, result.Statistics);

            // 2. 匯入工作紀錄
            await ImportWorkLogsAsync(userId, exportData.WorkLogs, referenceIdMappings, result.Statistics, result.Errors);

            // 3. 匯入待辦事項
            await ImportTodosAsync(userId, exportData.Todos, referenceIdMappings, attachmentDataDict, result.Statistics, result.Errors);

            // 提交交易
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            result.Success = true;
            result.Message = "匯入成功完成";
            
            _logger.LogInformation("資料匯入完成: {WorkLogs} 筆工作紀錄, {Todos} 筆待辦事項", 
                result.Statistics.WorkLogsImported, result.Statistics.TodosImported);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯入資料時發生錯誤");
            
            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }

            result.Success = false;
            result.Errors.Add($"匯入失敗: {ex.Message}");
        }
        finally
        {
            transaction?.Dispose();
        }

        return result;
    }

    private async Task<ReferenceIdMappings> ImportReferenceDataAsync(
        ReferenceDataExportDto referenceData, 
        ImportStatisticsDto statistics)
    {
        var mappings = new ReferenceIdMappings();

        // 匯入專案
        foreach (var item in referenceData.Projects)
        {
            var existing = await _context.Projects.FirstOrDefaultAsync(p => p.Name == item.Name);
            if (existing != null)
            {
                mappings.ProjectIdMap[item.OriginalId] = existing.Id;
                statistics.ProjectsSkipped++;
            }
            else
            {
                var newProject = new Project
                {
                    Name = item.Name,
                    IsActive = item.IsActive,
                    SortOrder = item.SortOrder
                };
                _context.Projects.Add(newProject);
                await _context.SaveChangesAsync();
                mappings.ProjectIdMap[item.OriginalId] = newProject.Id;
                statistics.ProjectsCreated++;
            }
        }

        // 匯入部門
        foreach (var item in referenceData.Departments)
        {
            var existing = await _context.Departments.FirstOrDefaultAsync(d => d.Name == item.Name);
            if (existing != null)
            {
                mappings.DepartmentIdMap[item.OriginalId] = existing.Id;
                statistics.DepartmentsSkipped++;
            }
            else
            {
                var newDept = new Department
                {
                    Name = item.Name,
                    IsActive = item.IsActive,
                    SortOrder = item.SortOrder
                };
                _context.Departments.Add(newDept);
                await _context.SaveChangesAsync();
                mappings.DepartmentIdMap[item.OriginalId] = newDept.Id;
                statistics.DepartmentsCreated++;
            }
        }

        // 匯入工作類型
        foreach (var item in referenceData.WorkTypes)
        {
            var existing = await _context.WorkTypes.FirstOrDefaultAsync(w => w.Name == item.Name);
            if (existing != null)
            {
                mappings.WorkTypeIdMap[item.OriginalId] = existing.Id;
                statistics.WorkTypesSkipped++;
            }
            else
            {
                var newWorkType = new WorkType
                {
                    Name = item.Name,
                    IsActive = item.IsActive,
                    SortOrder = item.SortOrder
                };
                _context.WorkTypes.Add(newWorkType);
                await _context.SaveChangesAsync();
                mappings.WorkTypeIdMap[item.OriginalId] = newWorkType.Id;
                statistics.WorkTypesCreated++;
            }
        }

        // 匯入處理狀態
        foreach (var item in referenceData.ProcessStatuses)
        {
            var existing = await _context.ProcessStatuses.FirstOrDefaultAsync(p => p.Name == item.Name);
            if (existing != null)
            {
                mappings.ProcessStatusIdMap[item.OriginalId] = existing.Id;
                statistics.ProcessStatusesSkipped++;
            }
            else
            {
                var newStatus = new ProcessStatus
                {
                    Name = item.Name,
                    IsActive = item.IsActive,
                    SortOrder = item.SortOrder
                };
                _context.ProcessStatuses.Add(newStatus);
                await _context.SaveChangesAsync();
                mappings.ProcessStatusIdMap[item.OriginalId] = newStatus.Id;
                statistics.ProcessStatusesCreated++;
            }
        }

        // 匯入待辦分類
        foreach (var item in referenceData.TodoCategories)
        {
            var existing = await _context.TodoCategories.FirstOrDefaultAsync(c => c.Name == item.Name);
            if (existing != null)
            {
                mappings.TodoCategoryIdMap[item.OriginalId] = existing.Id;
                statistics.TodoCategoriesSkipped++;
            }
            else
            {
                var newCategory = new TodoCategory
                {
                    Name = item.Name,
                    ColorCode = item.ColorCode ?? string.Empty,
                    Icon = item.Icon ?? string.Empty,
                    IsActive = item.IsActive,
                    SortOrder = item.SortOrder
                };
                _context.TodoCategories.Add(newCategory);
                await _context.SaveChangesAsync();
                mappings.TodoCategoryIdMap[item.OriginalId] = newCategory.Id;
                statistics.TodoCategoriesCreated++;
            }
        }

        return mappings;
    }

    private async Task ImportWorkLogsAsync(
        int userId,
        List<WorkLogExportDto> workLogs,
        ReferenceIdMappings mappings,
        ImportStatisticsDto statistics,
        List<string> errors)
    {
        foreach (var workLogDto in workLogs)
        {
            try
            {
                // 解析參照 ID
                if (!mappings.ProjectIdMap.TryGetValue(workLogDto.ProjectId, out var projectId))
                {
                    errors.Add($"工作紀錄 '{workLogDto.Title}': 找不到專案 '{workLogDto.ProjectName}'");
                    statistics.WorkLogsFailed++;
                    continue;
                }

                if (!mappings.ProcessStatusIdMap.TryGetValue(workLogDto.ProcessStatusId, out var statusId))
                {
                    errors.Add($"工作紀錄 '{workLogDto.Title}': 找不到處理狀態 '{workLogDto.ProcessStatusName}'");
                    statistics.WorkLogsFailed++;
                    continue;
                }

                var workLog = new WorkLogEntry
                {
                    UserId = userId,
                    Title = workLogDto.Title,
                    Content = workLogDto.Content,
                    RecordDate = workLogDto.RecordDate,
                    WorkHours = workLogDto.WorkHours,
                    CreatedAt = workLogDto.CreatedAt,
                    UpdatedAt = workLogDto.UpdatedAt,
                    ProjectId = projectId,
                    ProcessStatusId = statusId
                };

                _context.WorkLogEntries.Add(workLog);
                await _context.SaveChangesAsync();

                // 處理部門關聯
                foreach (var dept in workLogDto.Departments)
                {
                    if (mappings.DepartmentIdMap.TryGetValue(dept.OriginalId, out var deptId))
                    {
                        _context.WorkLogDepartments.Add(new WorkLogDepartment
                        {
                            WorkLogEntryId = workLog.Id,
                            DepartmentId = deptId
                        });
                    }
                }

                // 處理工作類型關聯
                foreach (var workType in workLogDto.WorkTypes)
                {
                    if (mappings.WorkTypeIdMap.TryGetValue(workType.OriginalId, out var workTypeId))
                    {
                        _context.WorkLogWorkTypes.Add(new WorkLogWorkType
                        {
                            WorkLogEntryId = workLog.Id,
                            WorkTypeId = workTypeId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                statistics.WorkLogsImported++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "匯入工作紀錄 '{Title}' 時發生錯誤", workLogDto.Title);
                errors.Add($"工作紀錄 '{workLogDto.Title}': {ex.Message}");
                statistics.WorkLogsFailed++;
            }
        }
    }

    private async Task ImportTodosAsync(
        int userId,
        List<TodoExportDto> todos,
        ReferenceIdMappings mappings,
        Dictionary<int, byte[]> attachmentDataDict,
        ImportStatisticsDto statistics,
        List<string> errors)
    {
        foreach (var todoDto in todos)
        {
            try
            {
                // 解析分類 ID (nullable)
                int? categoryId = null;
                if (todoDto.CategoryId.HasValue)
                {
                    if (mappings.TodoCategoryIdMap.TryGetValue(todoDto.CategoryId.Value, out var catId))
                    {
                        categoryId = catId;
                    }
                }

                var todo = new TodoItem
                {
                    UserId = userId,
                    Title = todoDto.Title,
                    Description = todoDto.Description,
                    DueDate = todoDto.DueDate,
                    Status = todoDto.Status,
                    Priority = todoDto.Priority,
                    CreatedAt = todoDto.CreatedAt,
                    UpdatedAt = todoDto.UpdatedAt,
                    CompletedAt = todoDto.CompletedAt,
                    CategoryId = categoryId
                };

                _context.TodoItems.Add(todo);
                await _context.SaveChangesAsync();

                // 匯入子任務
                foreach (var subTaskDto in todoDto.SubTasks)
                {
                    var subTask = new TodoSubTask
                    {
                        TodoItemId = todo.Id,
                        Title = subTaskDto.Title,
                        IsCompleted = subTaskDto.IsCompleted,
                        SortOrder = subTaskDto.SortOrder,
                        CreatedAt = subTaskDto.CreatedAt
                    };
                    _context.TodoSubTasks.Add(subTask);
                    statistics.SubTasksImported++;
                }

                // 匯入評論
                foreach (var commentDto in todoDto.Comments)
                {
                    var comment = new TodoComment
                    {
                        TodoItemId = todo.Id,
                        UserId = userId, // 使用當前使用者 ID
                        Content = commentDto.Content,
                        CreatedAt = commentDto.CreatedAt,
                        UpdatedAt = commentDto.UpdatedAt
                    };
                    _context.TodoComments.Add(comment);
                    statistics.CommentsImported++;
                }

                // 匯入附件
                foreach (var attachmentDto in todoDto.Attachments)
                {
                    byte[] fileData = Array.Empty<byte>();
                    if (attachmentDataDict.TryGetValue(attachmentDto.OriginalId, out var data))
                    {
                        fileData = data;
                    }

                    var attachment = new TodoAttachment
                    {
                        TodoItemId = todo.Id,
                        FileName = attachmentDto.FileName,
                        FileSize = attachmentDto.FileSize,
                        ContentType = attachmentDto.ContentType,
                        FileData = fileData,
                        UploadedAt = attachmentDto.UploadedAt
                    };
                    _context.TodoAttachments.Add(attachment);
                    statistics.AttachmentsImported++;
                }

                await _context.SaveChangesAsync();
                statistics.TodosImported++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "匯入待辦事項 '{Title}' 時發生錯誤", todoDto.Title);
                errors.Add($"待辦事項 '{todoDto.Title}': {ex.Message}");
                statistics.TodosFailed++;
            }
        }
    }

    private class ReferenceIdMappings
    {
        public Dictionary<int, int> ProjectIdMap { get; set; } = new();
        public Dictionary<int, int> DepartmentIdMap { get; set; } = new();
        public Dictionary<int, int> WorkTypeIdMap { get; set; } = new();
        public Dictionary<int, int> ProcessStatusIdMap { get; set; } = new();
        public Dictionary<int, int> TodoCategoryIdMap { get; set; } = new();
    }

    public async Task<DataImportResultDto> ValidateWorkLogImportFileAsync(byte[] zipFileBytes)
    {
        var result = new DataImportResultDto { Success = true };

        try
        {
            using var memoryStream = new MemoryStream(zipFileBytes);
            using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

            // 檢查 data.json 是否存在
            var dataEntry = archive.GetEntry("data.json");
            if (dataEntry == null)
            {
                result.Success = false;
                result.Errors.Add("ZIP 檔案中找不到 data.json");
                return result;
            }

            // 嘗試解析 JSON
            using var entryStream = dataEntry.Open();
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var exportData = await JsonSerializer.DeserializeAsync<WorkLogDataExportDto>(entryStream, jsonOptions);

            if (exportData == null)
            {
                result.Success = false;
                result.Errors.Add("無法解析 data.json");
                return result;
            }

            // 檢查版本相容性
            if (exportData.Version != "1.0")
            {
                result.Warnings.Add($"匯出版本 {exportData.Version} 可能與當前版本不相容");
            }

            // 檢查匯出類型
            if (exportData.ExportType != "WorkLogData")
            {
                result.Warnings.Add($"匯出類型為 {exportData.ExportType}，可能不是工作紀錄備份檔案");
            }

            result.Message = $"驗證成功。包含 {exportData.WorkLogs.Count} 筆工作紀錄和 {exportData.Todos.Count} 筆待辦事項";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "驗證工作紀錄匯入檔案時發生錯誤");
            result.Success = false;
            result.Errors.Add($"驗證失敗: {ex.Message}");
        }

        return result;
    }

    public async Task<DataImportResultDto> ImportWorkLogDataAsync(int userId, byte[] zipFileBytes)
    {
        var result = new DataImportResultDto { Success = false };

        // 先驗證
        var validationResult = await ValidateWorkLogImportFileAsync(zipFileBytes);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        IDbContextTransaction? transaction = null;

        try
        {
            _logger.LogInformation("開始匯入工作紀錄資料到使用者 {UserId}", userId);

            // 解析資料
            using var memoryStream = new MemoryStream(zipFileBytes);
            using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

            var dataEntry = archive.GetEntry("data.json")!;
            WorkLogDataExportDto exportData;
            
            using (var entryStream = dataEntry.Open())
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                exportData = (await JsonSerializer.DeserializeAsync<WorkLogDataExportDto>(entryStream, jsonOptions))!;
            }

            // 開始交易
            transaction = await _context.Database.BeginTransactionAsync();

            // 準備附件資料字典
            var attachmentDataDict = new Dictionary<int, byte[]>();
            if (exportData.IncludesAttachments)
            {
                foreach (var todo in exportData.Todos)
                {
                    foreach (var attachment in todo.Attachments.Where(a => a.FilePath != null))
                    {
                        var attachmentEntry = archive.GetEntry(attachment.FilePath!);
                        if (attachmentEntry != null)
                        {
                            using var attachmentStream = attachmentEntry.Open();
                            using var ms = new MemoryStream();
                            await attachmentStream.CopyToAsync(ms);
                            attachmentDataDict[attachment.OriginalId] = ms.ToArray();
                        }
                    }
                }
            }

            // 建立 ID 映射（用於處理工作紀錄中的參照）
            var referenceIdMappings = await BuildReferenceIdMappingsAsync(exportData.WorkLogs, exportData.Todos);

            // 匯入工作紀錄
            await ImportWorkLogsAsync(userId, exportData.WorkLogs, referenceIdMappings, result.Statistics, result.Errors);

            // 匯入待辦事項
            await ImportTodosAsync(userId, exportData.Todos, referenceIdMappings, attachmentDataDict, result.Statistics, result.Errors);

            // 提交交易
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            result.Success = true;
            result.Message = "匯入成功完成";
            
            _logger.LogInformation("工作紀錄資料匯入完成: {WorkLogs} 筆工作紀錄, {Todos} 筆待辦事項", 
                result.Statistics.WorkLogsImported, result.Statistics.TodosImported);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯入工作紀錄資料時發生錯誤");
            
            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }

            result.Success = false;
            result.Errors.Add($"匯入失敗: {ex.Message}");
        }
        finally
        {
            transaction?.Dispose();
        }

        return result;
    }

    public async Task<DataImportResultDto> ValidateSystemDataImportFileAsync(byte[] zipFileBytes)
    {
        var result = new DataImportResultDto { Success = true };

        try
        {
            using var memoryStream = new MemoryStream(zipFileBytes);
            using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

            // 檢查 data.json 是否存在
            var dataEntry = archive.GetEntry("data.json");
            if (dataEntry == null)
            {
                result.Success = false;
                result.Errors.Add("ZIP 檔案中找不到 data.json");
                return result;
            }

            // 嘗試解析 JSON
            using var entryStream = dataEntry.Open();
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var exportData = await JsonSerializer.DeserializeAsync<SystemDataExportDto>(entryStream, jsonOptions);

            if (exportData == null)
            {
                result.Success = false;
                result.Errors.Add("無法解析 data.json");
                return result;
            }

            // 檢查版本相容性
            if (exportData.Version != "1.0")
            {
                result.Warnings.Add($"匯出版本 {exportData.Version} 可能與當前版本不相容");
            }

            // 檢查匯出類型
            if (exportData.ExportType != "SystemData")
            {
                result.Warnings.Add($"匯出類型為 {exportData.ExportType}，可能不是系統管理資料備份檔案");
            }

            var refData = exportData.ReferenceData;
            result.Message = $"驗證成功。包含 {refData.Projects.Count} 個專案、" +
                           $"{refData.Departments.Count} 個部門、" +
                           $"{refData.WorkTypes.Count} 個工作類型、" +
                           $"{refData.ProcessStatuses.Count} 個處理狀態、" +
                           $"{refData.TodoCategories.Count} 個待辦分類";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "驗證系統管理資料匯入檔案時發生錯誤");
            result.Success = false;
            result.Errors.Add($"驗證失敗: {ex.Message}");
        }

        return result;
    }

    public async Task<DataImportResultDto> ImportSystemDataAsync(int userId, byte[] zipFileBytes)
    {
        var result = new DataImportResultDto { Success = false };

        // 先驗證
        var validationResult = await ValidateSystemDataImportFileAsync(zipFileBytes);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        IDbContextTransaction? transaction = null;

        try
        {
            _logger.LogInformation("開始匯入系統管理資料（執行者: 使用者 {UserId}）", userId);

            // 解析資料
            using var memoryStream = new MemoryStream(zipFileBytes);
            using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

            var dataEntry = archive.GetEntry("data.json")!;
            SystemDataExportDto exportData;
            
            using (var entryStream = dataEntry.Open())
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                exportData = (await JsonSerializer.DeserializeAsync<SystemDataExportDto>(entryStream, jsonOptions))!;
            }

            // 開始交易
            transaction = await _context.Database.BeginTransactionAsync();

            // 匯入參照資料
            await ImportReferenceDataAsync(exportData.ReferenceData, result.Statistics);

            // 提交交易
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            result.Success = true;
            result.Message = "系統管理資料匯入成功完成";
            
            _logger.LogInformation("系統管理資料匯入完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯入系統管理資料時發生錯誤");
            
            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }

            result.Success = false;
            result.Errors.Add($"匯入失敗: {ex.Message}");
        }
        finally
        {
            transaction?.Dispose();
        }

        return result;
    }

    /// <summary>
    /// 從工作紀錄和待辦事項中建立參照 ID 映射
    /// </summary>
    private async Task<ReferenceIdMappings> BuildReferenceIdMappingsAsync(
        List<WorkLogExportDto> workLogs,
        List<TodoExportDto> todos)
    {
        var mappings = new ReferenceIdMappings();

        // 收集所有參照的名稱
        var projectNames = workLogs.Select(w => w.ProjectName).Distinct().ToList();
        var statusNames = workLogs.Select(w => w.ProcessStatusName).Distinct().ToList();
        var deptNames = workLogs.SelectMany(w => w.Departments.Select(d => d.Name)).Distinct().ToList();
        var workTypeNames = workLogs.SelectMany(w => w.WorkTypes.Select(t => t.Name)).Distinct().ToList();
        var categoryNames = todos.Where(t => t.CategoryName != null).Select(t => t.CategoryName!).Distinct().ToList();

        // 從資料庫查詢現有的參照資料
        var projects = await _context.Projects.Where(p => projectNames.Contains(p.Name)).ToListAsync();
        var statuses = await _context.ProcessStatuses.Where(s => statusNames.Contains(s.Name)).ToListAsync();
        var departments = await _context.Departments.Where(d => deptNames.Contains(d.Name)).ToListAsync();
        var workTypes = await _context.WorkTypes.Where(w => workTypeNames.Contains(w.Name)).ToListAsync();
        var categories = await _context.TodoCategories.Where(c => categoryNames.Contains(c.Name)).ToListAsync();

        // 建立名稱到 ID 的映射，然後根據原始 ID 建立映射
        foreach (var workLog in workLogs)
        {
            var project = projects.FirstOrDefault(p => p.Name == workLog.ProjectName);
            if (project != null)
            {
                mappings.ProjectIdMap[workLog.ProjectId] = project.Id;
            }

            var status = statuses.FirstOrDefault(s => s.Name == workLog.ProcessStatusName);
            if (status != null)
            {
                mappings.ProcessStatusIdMap[workLog.ProcessStatusId] = status.Id;
            }

            foreach (var dept in workLog.Departments)
            {
                var department = departments.FirstOrDefault(d => d.Name == dept.Name);
                if (department != null)
                {
                    mappings.DepartmentIdMap[dept.OriginalId] = department.Id;
                }
            }

            foreach (var workType in workLog.WorkTypes)
            {
                var wt = workTypes.FirstOrDefault(w => w.Name == workType.Name);
                if (wt != null)
                {
                    mappings.WorkTypeIdMap[workType.OriginalId] = wt.Id;
                }
            }
        }

        foreach (var todo in todos.Where(t => t.CategoryId.HasValue && t.CategoryName != null))
        {
            var category = categories.FirstOrDefault(c => c.Name == todo.CategoryName);
            if (category != null)
            {
                mappings.TodoCategoryIdMap[todo.CategoryId!.Value] = category.Id;
            }
        }

        return mappings;
    }
}
