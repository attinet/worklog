using Microsoft.EntityFrameworkCore;
using WorkLog.Domain.Entities;
using WorkLog.Domain.Interfaces;
using WorkLog.Infrastructure.Data;

namespace WorkLog.Infrastructure.Repositories;

/// <summary>
/// 工作紀錄 Repository 實作
/// </summary>
public class WorkLogRepository : IWorkLogRepository
{
    private readonly AppDbContext _context;

    public WorkLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WorkLogEntry?> GetByIdAsync(int id)
        => await _context.WorkLogEntries.FindAsync(id);

    public async Task<WorkLogEntry?> GetByIdWithDetailsAsync(int id)
        => await _context.WorkLogEntries
            .Include(w => w.Project)
            .Include(w => w.ProcessStatus)
            .Include(w => w.WorkLogDepartments).ThenInclude(wd => wd.Department)
            .Include(w => w.WorkLogWorkTypes).ThenInclude(wt => wt.WorkType)
            .FirstOrDefaultAsync(w => w.Id == id);

    public async Task<PagedResult<WorkLogEntry>> GetPagedAsync(WorkLogQueryParameters parameters)
    {
        var query = _context.WorkLogEntries
            .Include(w => w.Project)
            .Include(w => w.ProcessStatus)
            .Include(w => w.WorkLogDepartments).ThenInclude(wd => wd.Department)
            .Include(w => w.WorkLogWorkTypes).ThenInclude(wt => wt.WorkType)
            .Where(w => w.UserId == parameters.UserId)
            .AsQueryable();

        // 篩選條件
        if (parameters.StartDate.HasValue)
            query = query.Where(w => w.RecordDate >= parameters.StartDate.Value);

        if (parameters.EndDate.HasValue)
            query = query.Where(w => w.RecordDate <= parameters.EndDate.Value);

        if (parameters.ProjectId.HasValue)
            query = query.Where(w => w.ProjectId == parameters.ProjectId.Value);

        if (parameters.ProcessStatusId.HasValue)
            query = query.Where(w => w.ProcessStatusId == parameters.ProcessStatusId.Value);

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim();
            query = query.Where(w =>
                w.Title.Contains(keyword) ||
                w.Content.Contains(keyword));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(w => w.RecordDate)
            .ThenByDescending(w => w.CreatedAt)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<WorkLogEntry>
        {
            Items = items,
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<WorkLogEntry> CreateAsync(WorkLogEntry entry)
    {
        _context.WorkLogEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task UpdateAsync(WorkLogEntry entry)
    {
        entry.UpdatedAt = DateTime.UtcNow;

        // 清除舊的多對多關聯
        var existingDepartments = await _context.WorkLogDepartments
            .Where(wd => wd.WorkLogEntryId == entry.Id).ToListAsync();
        _context.WorkLogDepartments.RemoveRange(existingDepartments);

        var existingWorkTypes = await _context.WorkLogWorkTypes
            .Where(wt => wt.WorkLogEntryId == entry.Id).ToListAsync();
        _context.WorkLogWorkTypes.RemoveRange(existingWorkTypes);

        _context.WorkLogEntries.Update(entry);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entry = await _context.WorkLogEntries.FindAsync(id);
        if (entry != null)
        {
            _context.WorkLogEntries.Remove(entry);
            await _context.SaveChangesAsync();
        }
    }
}
