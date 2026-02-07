using System.Security.Claims;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkLog.Domain.Entities;
using WorkLog.Domain.Interfaces;
using WorkLog.Shared.DTOs;
using WorkLog.Shared.DTOs.WorkLogs;

namespace WorkLog.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WorkLogsController : ControllerBase
{
    private readonly IWorkLogRepository _workLogRepository;
    private readonly ILookupRepository<Project> _projectRepository;
    private readonly ILookupRepository<Department> _departmentRepository;
    private readonly ILookupRepository<WorkType> _workTypeRepository;
    private readonly ILookupRepository<ProcessStatus> _statusRepository;

    public WorkLogsController(
        IWorkLogRepository workLogRepository,
        ILookupRepository<Project> projectRepository,
        ILookupRepository<Department> departmentRepository,
        ILookupRepository<WorkType> workTypeRepository,
        ILookupRepository<ProcessStatus> statusRepository)
    {
        _workLogRepository = workLogRepository;
        _projectRepository = projectRepository;
        _departmentRepository = departmentRepository;
        _workTypeRepository = workTypeRepository;
        _statusRepository = statusRepository;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>
    /// 查詢工作紀錄（含分頁與篩選）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<WorkLogResponse>>> GetAll(
        [FromQuery] WorkLogQueryDto query)
    {
        var parameters = new WorkLogQueryParameters
        {
            Page = query.Page,
            PageSize = query.PageSize,
            StartDate = query.StartDate,
            EndDate = query.EndDate,
            ProjectId = query.ProjectId,
            ProcessStatusId = query.ProcessStatusId,
            Keyword = query.Keyword,
            UserId = GetUserId()
        };

        var result = await _workLogRepository.GetPagedAsync(parameters);

        return Ok(new PagedResponse<WorkLogResponse>
        {
            Items = result.Items.Select(MapToResponse).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        });
    }

    /// <summary>
    /// 取得單筆工作紀錄
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkLogResponse>> GetById(int id)
    {
        var entry = await _workLogRepository.GetByIdWithDetailsAsync(id);
        if (entry == null) return NotFound();
        if (entry.UserId != GetUserId())
            return Forbid();

        return Ok(MapToResponse(entry));
    }

    /// <summary>
    /// 新增工作紀錄
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WorkLogResponse>> Create([FromBody] WorkLogRequest request)
    {
        var entry = new WorkLogEntry
        {
            Title = request.Title,
            Content = request.Content,
            RecordDate = request.RecordDate,
            WorkHours = request.WorkHours,
            ProjectId = request.ProjectId,
            ProcessStatusId = request.ProcessStatusId,
            UserId = GetUserId(),
            CreatedAt = DateTime.UtcNow,
            WorkLogDepartments = request.DepartmentIds
                .Select(dId => new WorkLogDepartment { DepartmentId = dId })
                .ToList(),
            WorkLogWorkTypes = request.WorkTypeIds
                .Select(wtId => new WorkLogWorkType { WorkTypeId = wtId })
                .ToList()
        };

        await _workLogRepository.CreateAsync(entry);

        var created = await _workLogRepository.GetByIdWithDetailsAsync(entry.Id);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, MapToResponse(created!));
    }

    /// <summary>
    /// 編輯工作紀錄
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<WorkLogResponse>> Update(int id, [FromBody] WorkLogRequest request)
    {
        var entry = await _workLogRepository.GetByIdAsync(id);
        if (entry == null) return NotFound();
        if (entry.UserId != GetUserId())
            return Forbid();

        entry.Title = request.Title;
        entry.Content = request.Content;
        entry.RecordDate = request.RecordDate;
        entry.WorkHours = request.WorkHours;
        entry.ProjectId = request.ProjectId;
        entry.ProcessStatusId = request.ProcessStatusId;
        entry.WorkLogDepartments = request.DepartmentIds
            .Select(dId => new WorkLogDepartment { WorkLogEntryId = id, DepartmentId = dId })
            .ToList();
        entry.WorkLogWorkTypes = request.WorkTypeIds
            .Select(wtId => new WorkLogWorkType { WorkLogEntryId = id, WorkTypeId = wtId })
            .ToList();

        await _workLogRepository.UpdateAsync(entry);

        var updated = await _workLogRepository.GetByIdWithDetailsAsync(id);
        return Ok(MapToResponse(updated!));
    }

    /// <summary>
    /// 刪除工作紀錄
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await _workLogRepository.GetByIdAsync(id);
        if (entry == null) return NotFound();
        if (entry.UserId != GetUserId())
            return Forbid();

        await _workLogRepository.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// 匯出工作紀錄為Excel檔案（依據月份）
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportToExcel([FromQuery] int year, [FromQuery] int month)
    {
        var userId = GetUserId();
        var startDate = DateOnly.FromDateTime(new DateTime(year, month, 1));
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var parameters = new WorkLogQueryParameters
        {
            Page = 1,
            PageSize = 10000, // 取得所有資料
            StartDate = startDate,
            EndDate = endDate,
            UserId = userId
        };

        var result = await _workLogRepository.GetPagedAsync(parameters);
        var workLogs = result.Items.OrderBy(w => w.RecordDate).ThenBy(w => w.CreatedAt).ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add($"{year}年{month}月工作紀錄");

        // 設定標題列
        worksheet.Cell(1, 1).Value = "日期";
        worksheet.Cell(1, 2).Value = "工作標題";
        worksheet.Cell(1, 3).Value = "工作內容";
        worksheet.Cell(1, 4).Value = "工時";
        worksheet.Cell(1, 5).Value = "專案名稱";
        worksheet.Cell(1, 6).Value = "業管單位";
        worksheet.Cell(1, 7).Value = "工作類型";
        worksheet.Cell(1, 8).Value = "處理狀態";

        // 標題列樣式
        var headerRange = worksheet.Range(1, 1, 1, 8);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // 填入資料
        int row = 2;
        foreach (var log in workLogs)
        {
            worksheet.Cell(row, 1).Value = log.RecordDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 2).Value = log.Title;
            worksheet.Cell(row, 3).Value = log.Content;
            worksheet.Cell(row, 4).Value = log.WorkHours;
            worksheet.Cell(row, 5).Value = log.Project.Name;
            worksheet.Cell(row, 6).Value = string.Join(", ", log.WorkLogDepartments.Select(d => d.Department.Name));
            worksheet.Cell(row, 7).Value = string.Join(", ", log.WorkLogWorkTypes.Select(w => w.WorkType.Name));
            worksheet.Cell(row, 8).Value = log.ProcessStatus.Name;
            row++;
        }

        // 自動調整欄寬
        worksheet.Columns().AdjustToContents();

        // 設定欄寬上限
        foreach (var column in worksheet.Columns())
        {
            if (column.Width > 50)
                column.Width = 50;
        }

        // 產生檔案
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"工作紀錄_{year}年{month:D2}月.xlsx";
        return File(stream.ToArray(), 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            fileName);
    }

    private static WorkLogResponse MapToResponse(WorkLogEntry entry) => new()
    {
        Id = entry.Id,
        Title = entry.Title,
        Content = entry.Content,
        RecordDate = entry.RecordDate,
        WorkHours = entry.WorkHours,
        CreatedAt = entry.CreatedAt,
        UpdatedAt = entry.UpdatedAt,
        Project = new LookupItemDto
        {
            Id = entry.Project.Id,
            Name = entry.Project.Name,
            IsActive = entry.Project.IsActive,
            SortOrder = entry.Project.SortOrder
        },
        ProcessStatus = new LookupItemDto
        {
            Id = entry.ProcessStatus.Id,
            Name = entry.ProcessStatus.Name,
            IsActive = entry.ProcessStatus.IsActive,
            SortOrder = entry.ProcessStatus.SortOrder
        },
        Departments = entry.WorkLogDepartments.Select(wd => new LookupItemDto
        {
            Id = wd.Department.Id,
            Name = wd.Department.Name,
            IsActive = wd.Department.IsActive,
            SortOrder = wd.Department.SortOrder
        }).ToList(),
        WorkTypes = entry.WorkLogWorkTypes.Select(wt => new LookupItemDto
        {
            Id = wt.WorkType.Id,
            Name = wt.WorkType.Name,
            IsActive = wt.WorkType.IsActive,
            SortOrder = wt.WorkType.SortOrder
        }).ToList()
    };
}
