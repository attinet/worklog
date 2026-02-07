using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkLog.Domain.Entities;
using WorkLog.Domain.Enums;
using WorkLog.Domain.Interfaces;
using WorkLog.Shared.DTOs;
using WorkLog.Shared.DTOs.Admin;

namespace WorkLog.Api.Controllers;

/// <summary>
/// 系統管理 API（僅管理人員可存取）
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILookupRepository<Project> _projectRepo;
    private readonly ILookupRepository<Department> _departmentRepo;
    private readonly ILookupRepository<WorkType> _workTypeRepo;
    private readonly ILookupRepository<ProcessStatus> _statusRepo;

    public AdminController(
        IUserRepository userRepository,
        ILookupRepository<Project> projectRepo,
        ILookupRepository<Department> departmentRepo,
        ILookupRepository<WorkType> workTypeRepo,
        ILookupRepository<ProcessStatus> statusRepo)
    {
        _userRepository = userRepository;
        _projectRepo = projectRepo;
        _departmentRepo = departmentRepo;
        _workTypeRepo = workTypeRepo;
        _statusRepo = statusRepo;
    }

    // ===== 使用者管理 =====

    [HttpGet("users")]
    public async Task<ActionResult<List<UserListItemDto>>> GetUsers()
    {
        var users = await _userRepository.GetAllAsync();
        return Ok(users.Select(u => new UserListItemDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role.ToString(),
            CreatedAt = u.CreatedAt
        }));
    }

    [HttpPost("users")]
    public async Task<ActionResult<UserListItemDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        // 檢查使用者名稱是否已存在
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
            return Conflict(new ApiErrorResponse { Message = "使用者名稱已存在" });

        // 檢查電子郵件是否已存在
        var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
            return Conflict(new ApiErrorResponse { Message = "電子郵件已被註冊" });

        // 驗證角色
        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            return BadRequest(new ApiErrorResponse { Message = "無效的角色名稱" });

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        return CreatedAtAction(nameof(GetUsers), new UserListItemDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> ChangeUserRole(int id, [FromBody] ChangeRoleRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound(new ApiErrorResponse { Message = "使用者不存在" });

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            return BadRequest(new ApiErrorResponse { Message = "無效的角色名稱" });

        user.Role = role;
        await _userRepository.UpdateAsync(user);
        return NoContent();
    }

    [HttpPut("users/{id}/password")]
    public async Task<IActionResult> ChangeUserPassword(int id, [FromBody] ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound(new ApiErrorResponse { Message = "使用者不存在" });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user);
        
        return NoContent();
    }

    // ===== 專案名稱維護 =====

    [HttpGet("projects")]
    public async Task<ActionResult<List<LookupItemDto>>> GetProjects()
    {
        var items = await _projectRepo.GetAllAsync();
        return Ok(items.OrderBy(p => p.SortOrder).Select(MapProject));
    }

    [HttpPost("projects")]
    public async Task<ActionResult<LookupItemDto>> CreateProject([FromBody] LookupItemRequest request)
    {
        var project = new Project { Name = request.Name, IsActive = request.IsActive, SortOrder = request.SortOrder };
        await _projectRepo.CreateAsync(project);
        return CreatedAtAction(nameof(GetProjects), MapProject(project));
    }

    [HttpPut("projects/{id}")]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] LookupItemRequest request)
    {
        var project = await _projectRepo.GetByIdAsync(id);
        if (project == null) return NotFound();
        project.Name = request.Name;
        project.IsActive = request.IsActive;
        project.SortOrder = request.SortOrder;
        await _projectRepo.UpdateAsync(project);
        return NoContent();
    }

    [HttpDelete("projects/{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        await _projectRepo.DeleteAsync(id);
        return NoContent();
    }

    // ===== 業管單位維護 =====

    [HttpGet("departments")]
    public async Task<ActionResult<List<LookupItemDto>>> GetDepartments()
    {
        var items = await _departmentRepo.GetAllAsync();
        return Ok(items.OrderBy(d => d.SortOrder).Select(MapDepartment));
    }

    [HttpPost("departments")]
    public async Task<ActionResult<LookupItemDto>> CreateDepartment([FromBody] LookupItemRequest request)
    {
        var dept = new Department { Name = request.Name, IsActive = request.IsActive, SortOrder = request.SortOrder };
        await _departmentRepo.CreateAsync(dept);
        return CreatedAtAction(nameof(GetDepartments), MapDepartment(dept));
    }

    [HttpPut("departments/{id}")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] LookupItemRequest request)
    {
        var dept = await _departmentRepo.GetByIdAsync(id);
        if (dept == null) return NotFound();
        dept.Name = request.Name;
        dept.IsActive = request.IsActive;
        dept.SortOrder = request.SortOrder;
        await _departmentRepo.UpdateAsync(dept);
        return NoContent();
    }

    [HttpDelete("departments/{id}")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        await _departmentRepo.DeleteAsync(id);
        return NoContent();
    }

    // ===== 工作類型維護 =====

    [HttpGet("work-types")]
    public async Task<ActionResult<List<LookupItemDto>>> GetWorkTypes()
    {
        var items = await _workTypeRepo.GetAllAsync();
        return Ok(items.OrderBy(w => w.SortOrder).Select(MapWorkType));
    }

    [HttpPost("work-types")]
    public async Task<ActionResult<LookupItemDto>> CreateWorkType([FromBody] LookupItemRequest request)
    {
        var wt = new WorkType { Name = request.Name, IsActive = request.IsActive, SortOrder = request.SortOrder };
        await _workTypeRepo.CreateAsync(wt);
        return CreatedAtAction(nameof(GetWorkTypes), MapWorkType(wt));
    }

    [HttpPut("work-types/{id}")]
    public async Task<IActionResult> UpdateWorkType(int id, [FromBody] LookupItemRequest request)
    {
        var wt = await _workTypeRepo.GetByIdAsync(id);
        if (wt == null) return NotFound();
        wt.Name = request.Name;
        wt.IsActive = request.IsActive;
        wt.SortOrder = request.SortOrder;
        await _workTypeRepo.UpdateAsync(wt);
        return NoContent();
    }

    [HttpDelete("work-types/{id}")]
    public async Task<IActionResult> DeleteWorkType(int id)
    {
        await _workTypeRepo.DeleteAsync(id);
        return NoContent();
    }

    // ===== 處理狀態維護 =====

    [HttpGet("statuses")]
    public async Task<ActionResult<List<LookupItemDto>>> GetStatuses()
    {
        var items = await _statusRepo.GetAllAsync();
        return Ok(items.OrderBy(s => s.SortOrder).Select(MapStatus));
    }

    [HttpPost("statuses")]
    public async Task<ActionResult<LookupItemDto>> CreateStatus([FromBody] LookupItemRequest request)
    {
        var status = new ProcessStatus { Name = request.Name, IsActive = request.IsActive, SortOrder = request.SortOrder };
        await _statusRepo.CreateAsync(status);
        return CreatedAtAction(nameof(GetStatuses), MapStatus(status));
    }

    [HttpPut("statuses/{id}")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] LookupItemRequest request)
    {
        var status = await _statusRepo.GetByIdAsync(id);
        if (status == null) return NotFound();
        status.Name = request.Name;
        status.IsActive = request.IsActive;
        status.SortOrder = request.SortOrder;
        await _statusRepo.UpdateAsync(status);
        return NoContent();
    }

    [HttpDelete("statuses/{id}")]
    public async Task<IActionResult> DeleteStatus(int id)
    {
        await _statusRepo.DeleteAsync(id);
        return NoContent();
    }

    // ===== Mapping Helpers =====

    private static LookupItemDto MapProject(Project p) => new() { Id = p.Id, Name = p.Name, IsActive = p.IsActive, SortOrder = p.SortOrder };
    private static LookupItemDto MapDepartment(Department d) => new() { Id = d.Id, Name = d.Name, IsActive = d.IsActive, SortOrder = d.SortOrder };
    private static LookupItemDto MapWorkType(WorkType w) => new() { Id = w.Id, Name = w.Name, IsActive = w.IsActive, SortOrder = w.SortOrder };
    private static LookupItemDto MapStatus(ProcessStatus s) => new() { Id = s.Id, Name = s.Name, IsActive = s.IsActive, SortOrder = s.SortOrder };
}
