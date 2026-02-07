using Microsoft.EntityFrameworkCore;
using WorkLog.Domain.Entities;
using WorkLog.Domain.Enums;
using WorkLog.Domain.Interfaces;
using WorkLog.Infrastructure.Data;

namespace WorkLog.Infrastructure.Repositories;

/// <summary>
/// 待辦事項 Repository 實作
/// </summary>
public class TodoRepository : ITodoRepository
{
    private readonly AppDbContext _context;

    public TodoRepository(AppDbContext context)
    {
        _context = context;
    }

    #region 主要待辦事項操作

    public async Task<TodoItem?> GetByIdAsync(int id, int userId)
    {
        return await _context.TodoItems
            .Include(t => t.Category)
            .Include(t => t.SubTasks.OrderBy(s => s.SortOrder))
            .Include(t => t.Attachments)
            .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
                .ThenInclude(c => c.User)
            .Where(t => t.Id == id && t.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<PagedResult<TodoItem>> GetAllAsync(TodoQueryParameters parameters)
    {
        var query = _context.TodoItems
            .Include(t => t.Category)
            .Include(t => t.SubTasks)
            .Include(t => t.Attachments)
            .Include(t => t.Comments)
            .Where(t => t.UserId == parameters.UserId)
            .AsQueryable();

        // 篩選條件
        if (parameters.Status.HasValue)
            query = query.Where(t => t.Status == parameters.Status.Value);

        if (parameters.Priority.HasValue)
            query = query.Where(t => t.Priority == parameters.Priority.Value);

        if (parameters.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == parameters.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim();
            query = query.Where(t =>
                t.Title.Contains(keyword) ||
                (t.Description != null && t.Description.Contains(keyword)));
        }

        if (parameters.DueDateFrom.HasValue)
            query = query.Where(t => t.DueDate >= parameters.DueDateFrom.Value);

        if (parameters.DueDateTo.HasValue)
            query = query.Where(t => t.DueDate <= parameters.DueDateTo.Value);

        var totalCount = await query.CountAsync();

        // 排序
        query = parameters.SortBy.ToLower() switch
        {
            "priority" => parameters.IsDescending
                ? query.OrderByDescending(t => t.Priority)
                : query.OrderBy(t => t.Priority),
            "createdat" => parameters.IsDescending
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderBy(t => t.CreatedAt),
            "title" => parameters.IsDescending
                ? query.OrderByDescending(t => t.Title)
                : query.OrderBy(t => t.Title),
            _ => parameters.IsDescending // 預設按截止日期排序
                ? query.OrderByDescending(t => t.DueDate)
                : query.OrderBy(t => t.DueDate)
        };

        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<TodoItem>
        {
            Items = items,
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<TodoDashboardSummary> GetDashboardSummaryAsync(int userId)
    {
        var today = DateTime.UtcNow.Date;
        var weekEnd = today.AddDays(7);

        var allTodos = await _context.TodoItems
            .Include(t => t.Category)
            .Include(t => t.SubTasks)
            .Where(t => t.UserId == userId)
            .AsNoTracking()
            .ToListAsync();

        var pendingCount = allTodos.Count(t => t.Status == TodoStatus.Pending);
        var inProgressCount = allTodos.Count(t => t.Status == TodoStatus.InProgress);
        var completedCount = allTodos.Count(t => t.Status == TodoStatus.Completed);

        var dueTodayItems = allTodos
            .Where(t => t.DueDate.HasValue &&
                       t.DueDate.Value.Date == today &&
                       t.Status != TodoStatus.Completed)
            .OrderBy(t => t.Priority)
            .Take(5)
            .ToList();

        var overdueItems = allTodos
            .Where(t => t.DueDate.HasValue &&
                       t.DueDate.Value.Date < today &&
                       t.Status != TodoStatus.Completed)
            .OrderBy(t => t.DueDate)
            .Take(5)
            .ToList();

        var dueTodayCount = allTodos.Count(t => t.DueDate.HasValue &&
                                                t.DueDate.Value.Date == today &&
                                                t.Status != TodoStatus.Completed);

        var dueThisWeekCount = allTodos.Count(t => t.DueDate.HasValue &&
                                                   t.DueDate.Value.Date >= today &&
                                                   t.DueDate.Value.Date <= weekEnd &&
                                                   t.Status != TodoStatus.Completed);

        var overdueCount = allTodos.Count(t => t.DueDate.HasValue &&
                                              t.DueDate.Value.Date < today &&
                                              t.Status != TodoStatus.Completed);

        return new TodoDashboardSummary
        {
            PendingCount = pendingCount,
            InProgressCount = inProgressCount,
            CompletedCount = completedCount,
            DueTodayCount = dueTodayCount,
            DueThisWeekCount = dueThisWeekCount,
            OverdueCount = overdueCount,
            DueTodayItems = dueTodayItems,
            OverdueItems = overdueItems
        };
    }

    public async Task<TodoItem> CreateAsync(TodoItem todoItem)
    {
        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync();
        return todoItem;
    }

    public async Task UpdateAsync(TodoItem todoItem)
    {
        todoItem.UpdatedAt = DateTime.UtcNow;

        if (todoItem.Status == TodoStatus.Completed && !todoItem.CompletedAt.HasValue)
        {
            todoItem.CompletedAt = DateTime.UtcNow;
        }
        else if (todoItem.Status != TodoStatus.Completed && todoItem.CompletedAt.HasValue)
        {
            todoItem.CompletedAt = null;
        }

        _context.TodoItems.Update(todoItem);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var todoItem = await _context.TodoItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (todoItem != null)
        {
            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region 子任務操作

    public async Task<TodoSubTask> AddSubTaskAsync(TodoSubTask subTask)
    {
        _context.TodoSubTasks.Add(subTask);
        await _context.SaveChangesAsync();
        return subTask;
    }

    public async Task UpdateSubTaskAsync(TodoSubTask subTask)
    {
        _context.TodoSubTasks.Update(subTask);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSubTaskAsync(int id, int userId)
    {
        var subTask = await _context.TodoSubTasks
            .Include(s => s.TodoItem)
            .FirstOrDefaultAsync(s => s.Id == id && s.TodoItem.UserId == userId);

        if (subTask != null)
        {
            _context.TodoSubTasks.Remove(subTask);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region 附件操作

    public async Task<TodoAttachment> AddAttachmentAsync(TodoAttachment attachment)
    {
        _context.TodoAttachments.Add(attachment);
        await _context.SaveChangesAsync();
        return attachment;
    }

    public async Task<TodoAttachment?> GetAttachmentAsync(int id, int userId)
    {
        return await _context.TodoAttachments
            .Include(a => a.TodoItem)
            .FirstOrDefaultAsync(a => a.Id == id && a.TodoItem.UserId == userId);
    }

    public async Task DeleteAttachmentAsync(int id, int userId)
    {
        var attachment = await _context.TodoAttachments
            .Include(a => a.TodoItem)
            .FirstOrDefaultAsync(a => a.Id == id && a.TodoItem.UserId == userId);

        if (attachment != null)
        {
            _context.TodoAttachments.Remove(attachment);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region 評論操作

    public async Task<TodoComment> AddCommentAsync(TodoComment comment)
    {
        _context.TodoComments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task UpdateCommentAsync(TodoComment comment)
    {
        comment.UpdatedAt = DateTime.UtcNow;
        _context.TodoComments.Update(comment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCommentAsync(int id, int userId)
    {
        var comment = await _context.TodoComments
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (comment != null)
        {
            _context.TodoComments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }

    #endregion
}
