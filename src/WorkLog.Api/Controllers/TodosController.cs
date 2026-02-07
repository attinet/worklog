using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkLog.Domain.Entities;
using WorkLog.Domain.Enums;
using WorkLog.Domain.Interfaces;
using WorkLog.Shared.DTOs;
using WorkLog.Shared.DTOs.Todos;
using WorkLog.Shared.DTOs.WorkLogs;

namespace WorkLog.Api.Controllers;

/// <summary>
/// 待辦事項 API
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly ITodoRepository _todoRepository;
    private readonly ITodoCategoryRepository _categoryRepository;

    public TodosController(ITodoRepository todoRepository, ITodoCategoryRepository categoryRepository)
    {
        _todoRepository = todoRepository;
        _categoryRepository = categoryRepository;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    #region 主要待辦事項操作

    /// <summary>
    /// 查詢待辦事項（含分頁與篩選）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<TodoItemListResponse>>> GetAll([FromQuery] TodoQueryDto query)
    {
        var parameters = new TodoQueryParameters
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Status = !string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<TodoStatus>(query.Status, true, out var status) ? status : null,
            Priority = !string.IsNullOrWhiteSpace(query.Priority) && Enum.TryParse<TodoPriority>(query.Priority, true, out var priority) ? priority : null,
            CategoryId = query.CategoryId,
            Keyword = query.Keyword,
            DueDateFrom = query.DueDateFrom,
            DueDateTo = query.DueDateTo,
            SortBy = query.SortBy,
            IsDescending = query.IsDescending,
            UserId = GetUserId()
        };

        var result = await _todoRepository.GetAllAsync(parameters);

        return Ok(new PagedResponse<TodoItemListResponse>
        {
            Items = result.Items.Select(MapToListResponse).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        });
    }

    /// <summary>
    /// 取得單筆待辦事項（含完整明細）
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItemResponse>> GetById(int id)
    {
        var todo = await _todoRepository.GetByIdAsync(id, GetUserId());
        if (todo == null) return NotFound();

        return Ok(MapToResponse(todo));
    }

    /// <summary>
    /// 取得儀表板摘要
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<TodoDashboardResponse>> GetDashboard()
    {
        var summary = await _todoRepository.GetDashboardSummaryAsync(GetUserId());

        return Ok(new TodoDashboardResponse
        {
            PendingCount = summary.PendingCount,
            InProgressCount = summary.InProgressCount,
            CompletedCount = summary.CompletedCount,
            DueTodayCount = summary.DueTodayCount,
            DueThisWeekCount = summary.DueThisWeekCount,
            OverdueCount = summary.OverdueCount,
            DueTodayItems = summary.DueTodayItems.Select(MapToListResponse).ToList(),
            OverdueItems = summary.OverdueItems.Select(MapToListResponse).ToList()
        });
    }

    /// <summary>
    /// 新增待辦事項
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TodoItemResponse>> Create([FromBody] CreateTodoItemRequest request)
    {
        if (!Enum.TryParse<TodoPriority>(request.Priority, true, out var priority))
            return BadRequest(new ApiErrorResponse { Message = "無效的優先順序" });

        var todo = new TodoItem
        {
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            Priority = priority,
            Status = TodoStatus.Pending,
            CategoryId = request.CategoryId,
            UserId = GetUserId(),
            CreatedAt = DateTime.UtcNow,
            SubTasks = request.SubTasks.Select((st, index) => new TodoSubTask
            {
                Title = st.Title,
                SortOrder = st.SortOrder > 0 ? st.SortOrder : index + 1,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            }).ToList()
        };

        await _todoRepository.CreateAsync(todo);

        var created = await _todoRepository.GetByIdAsync(todo.Id, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, MapToResponse(created!));
    }

    /// <summary>
    /// 更新待辦事項
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateTodoItemRequest request)
    {
        var todo = await _todoRepository.GetByIdAsync(id, GetUserId());
        if (todo == null) return NotFound();

        if (!Enum.TryParse<TodoPriority>(request.Priority, true, out var priority))
            return BadRequest(new ApiErrorResponse { Message = "無效的優先順序" });

        if (!Enum.TryParse<TodoStatus>(request.Status, true, out var status))
            return BadRequest(new ApiErrorResponse { Message = "無效的狀態" });

        todo.Title = request.Title;
        todo.Description = request.Description;
        todo.DueDate = request.DueDate;
        todo.Priority = priority;
        todo.Status = status;
        todo.CategoryId = request.CategoryId;

        await _todoRepository.UpdateAsync(todo);

        return NoContent();
    }

    /// <summary>
    /// 刪除待辦事項
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var todo = await _todoRepository.GetByIdAsync(id, GetUserId());
        if (todo == null) return NotFound();

        await _todoRepository.DeleteAsync(id, GetUserId());

        return NoContent();
    }

    #endregion

    #region 子任務操作

    /// <summary>
    /// 新增子任務
    /// </summary>
    [HttpPost("{todoId}/subtasks")]
    public async Task<ActionResult<TodoSubTaskResponse>> AddSubTask(int todoId, [FromBody] CreateTodoSubTaskRequest request)
    {
        var todo = await _todoRepository.GetByIdAsync(todoId, GetUserId());
        if (todo == null) return NotFound();

        var subTask = new TodoSubTask
        {
            TodoItemId = todoId,
            Title = request.Title,
            SortOrder = request.SortOrder,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _todoRepository.AddSubTaskAsync(subTask);

        return Ok(new TodoSubTaskResponse
        {
            Id = subTask.Id,
            Title = subTask.Title,
            IsCompleted = subTask.IsCompleted,
            SortOrder = subTask.SortOrder,
            CreatedAt = subTask.CreatedAt
        });
    }

    /// <summary>
    /// 更新子任務
    /// </summary>
    [HttpPut("{todoId}/subtasks/{id}")]
    public async Task<ActionResult> UpdateSubTask(int todoId, int id, [FromBody] UpdateTodoSubTaskRequest request)
    {
        var todo = await _todoRepository.GetByIdAsync(todoId, GetUserId());
        if (todo == null) return NotFound();

        var subTask = todo.SubTasks.FirstOrDefault(st => st.Id == id);
        if (subTask == null) return NotFound();

        subTask.Title = request.Title;
        subTask.IsCompleted = request.IsCompleted;
        subTask.SortOrder = request.SortOrder;

        await _todoRepository.UpdateSubTaskAsync(subTask);

        return NoContent();
    }

    /// <summary>
    /// 刪除子任務
    /// </summary>
    [HttpDelete("{todoId}/subtasks/{id}")]
    public async Task<ActionResult> DeleteSubTask(int todoId, int id)
    {
        var todo = await _todoRepository.GetByIdAsync(todoId, GetUserId());
        if (todo == null) return NotFound();

        await _todoRepository.DeleteSubTaskAsync(id, GetUserId());

        return NoContent();
    }

    #endregion

    #region 附件操作

    /// <summary>
    /// 上傳附件
    /// </summary>
    [HttpPost("{todoId}/attachments")]
    public async Task<ActionResult<TodoAttachmentResponse>> UploadAttachment(int todoId, [FromBody] UploadTodoAttachmentRequest request)
    {
        var todo = await _todoRepository.GetByIdAsync(todoId, GetUserId());
        if (todo == null) return NotFound();

        var attachment = new TodoAttachment
        {
            TodoItemId = todoId,
            FileName = request.FileName,
            FileSize = request.FileSize,
            ContentType = request.ContentType,
            FileData = request.FileData,
            UploadedAt = DateTime.UtcNow
        };

        await _todoRepository.AddAttachmentAsync(attachment);

        return Ok(new TodoAttachmentResponse
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            FileSize = attachment.FileSize,
            ContentType = attachment.ContentType,
            UploadedAt = attachment.UploadedAt
        });
    }

    /// <summary>
    /// 下載附件
    /// </summary>
    [HttpGet("{todoId}/attachments/{id}/download")]
    public async Task<ActionResult> DownloadAttachment(int todoId, int id)
    {
        var attachment = await _todoRepository.GetAttachmentAsync(id, GetUserId());
        if (attachment == null || attachment.TodoItemId != todoId) return NotFound();

        return File(attachment.FileData, attachment.ContentType, attachment.FileName);
    }

    /// <summary>
    /// 刪除附件
    /// </summary>
    [HttpDelete("{todoId}/attachments/{id}")]
    public async Task<ActionResult> DeleteAttachment(int todoId, int id)
    {
        var todo = await _todoRepository.GetByIdAsync(todoId, GetUserId());
        if (todo == null) return NotFound();

        await _todoRepository.DeleteAttachmentAsync(id, GetUserId());

        return NoContent();
    }

    #endregion

    #region 評論操作

    /// <summary>
    /// 新增評論
    /// </summary>
    [HttpPost("{todoId}/comments")]
    public async Task<ActionResult<TodoCommentResponse>> AddComment(int todoId, [FromBody] CreateTodoCommentRequest request)
    {
        var todo = await _todoRepository.GetByIdAsync(todoId, GetUserId());
        if (todo == null) return NotFound();

        var comment = new TodoComment
        {
            TodoItemId = todoId,
            UserId = GetUserId(),
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _todoRepository.AddCommentAsync(comment);

        return Ok(new TodoCommentResponse
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            UserId = comment.UserId,
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? ""
        });
    }

    /// <summary>
    /// 更新評論
    /// </summary>
    [HttpPut("{todoId}/comments/{id}")]
    public async Task<ActionResult> UpdateComment(int todoId, int id, [FromBody] UpdateTodoCommentRequest request)
    {
        var todo = await _todoRepository.GetByIdAsync(todoId, GetUserId());
        if (todo == null) return NotFound();

        var comment = todo.Comments.FirstOrDefault(c => c.Id == id);
        if (comment == null) return NotFound();

        if (comment.UserId != GetUserId())
            return Forbid();

        comment.Content = request.Content;

        await _todoRepository.UpdateCommentAsync(comment);

        return NoContent();
    }

    /// <summary>
    /// 刪除評論
    /// </summary>
    [HttpDelete("{todoId}/comments/{id}")]
    public async Task<ActionResult> DeleteComment(int todoId, int id)
    {
        var todo = await _todoRepository.GetByIdAsync(todoId, GetUserId());
        if (todo == null) return NotFound();

        var comment = todo.Comments.FirstOrDefault(c => c.Id == id);
        if (comment == null) return NotFound();

        if (comment.UserId != GetUserId())
            return Forbid();

        await _todoRepository.DeleteCommentAsync(id, GetUserId());

        return NoContent();
    }

    #endregion

    #region Helper Methods

    private TodoItemResponse MapToResponse(TodoItem todo)
    {
        return new TodoItemResponse
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            DueDate = todo.DueDate,
            Status = todo.Status.ToString(),
            Priority = todo.Priority.ToString(),
            CreatedAt = todo.CreatedAt,
            UpdatedAt = todo.UpdatedAt,
            CompletedAt = todo.CompletedAt,
            Category = todo.Category != null ? new TodoCategoryResponse
            {
                Id = todo.Category.Id,
                Name = todo.Category.Name,
                ColorCode = todo.Category.ColorCode,
                Icon = todo.Category.Icon
            } : null,
            SubTasks = todo.SubTasks.Select(st => new TodoSubTaskResponse
            {
                Id = st.Id,
                Title = st.Title,
                IsCompleted = st.IsCompleted,
                SortOrder = st.SortOrder,
                CreatedAt = st.CreatedAt
            }).ToList(),
            Attachments = todo.Attachments.Select(a => new TodoAttachmentResponse
            {
                Id = a.Id,
                FileName = a.FileName,
                FileSize = a.FileSize,
                ContentType = a.ContentType,
                UploadedAt = a.UploadedAt
            }).ToList(),
            Comments = todo.Comments.Select(c => new TodoCommentResponse
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                UserId = c.UserId,
                UserName = c.User.Username
            }).ToList()
        };
    }

    private TodoItemListResponse MapToListResponse(TodoItem todo)
    {
        return new TodoItemListResponse
        {
            Id = todo.Id,
            Title = todo.Title,
            DueDate = todo.DueDate,
            Status = todo.Status.ToString(),
            Priority = todo.Priority.ToString(),
            CreatedAt = todo.CreatedAt,
            Category = todo.Category != null ? new TodoCategoryResponse
            {
                Id = todo.Category.Id,
                Name = todo.Category.Name,
                ColorCode = todo.Category.ColorCode,
                Icon = todo.Category.Icon
            } : null,
            SubTaskCount = todo.SubTasks.Count,
            CompletedSubTaskCount = todo.SubTasks.Count(st => st.IsCompleted),
            AttachmentCount = todo.Attachments.Count,
            CommentCount = todo.Comments.Count
        };
    }

    #endregion
}
