using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkLog.Domain.Entities;
using WorkLog.Domain.Interfaces;
using WorkLog.Shared.DTOs;

namespace WorkLog.Api.Controllers;

/// <summary>
/// 查詢表 API（取得下拉選單資料）
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LookupController : ControllerBase
{
    private readonly ILookupRepository<Project> _projectRepo;
    private readonly ILookupRepository<Department> _departmentRepo;
    private readonly ILookupRepository<WorkType> _workTypeRepo;
    private readonly ILookupRepository<ProcessStatus> _statusRepo;

    public LookupController(
        ILookupRepository<Project> projectRepo,
        ILookupRepository<Department> departmentRepo,
        ILookupRepository<WorkType> workTypeRepo,
        ILookupRepository<ProcessStatus> statusRepo)
    {
        _projectRepo = projectRepo;
        _departmentRepo = departmentRepo;
        _workTypeRepo = workTypeRepo;
        _statusRepo = statusRepo;
    }

    [HttpGet("projects")]
    public async Task<ActionResult<List<LookupItemDto>>> GetProjects()
    {
        var items = await _projectRepo.GetAllAsync();
        return Ok(items.Where(p => p.IsActive).OrderBy(p => p.SortOrder)
            .Select(p => new LookupItemDto { Id = p.Id, Name = p.Name, IsActive = p.IsActive, SortOrder = p.SortOrder }));
    }

    [HttpGet("departments")]
    public async Task<ActionResult<List<LookupItemDto>>> GetDepartments()
    {
        var items = await _departmentRepo.GetAllAsync();
        return Ok(items.Where(d => d.IsActive).OrderBy(d => d.SortOrder)
            .Select(d => new LookupItemDto { Id = d.Id, Name = d.Name, IsActive = d.IsActive, SortOrder = d.SortOrder }));
    }

    [HttpGet("work-types")]
    public async Task<ActionResult<List<LookupItemDto>>> GetWorkTypes()
    {
        var items = await _workTypeRepo.GetAllAsync();
        return Ok(items.Where(w => w.IsActive).OrderBy(w => w.SortOrder)
            .Select(w => new LookupItemDto { Id = w.Id, Name = w.Name, IsActive = w.IsActive, SortOrder = w.SortOrder }));
    }

    [HttpGet("statuses")]
    public async Task<ActionResult<List<LookupItemDto>>> GetStatuses()
    {
        var items = await _statusRepo.GetAllAsync();
        return Ok(items.Where(s => s.IsActive).OrderBy(s => s.SortOrder)
            .Select(s => new LookupItemDto { Id = s.Id, Name = s.Name, IsActive = s.IsActive, SortOrder = s.SortOrder }));
    }
}
