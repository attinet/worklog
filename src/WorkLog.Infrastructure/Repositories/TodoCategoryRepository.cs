using WorkLog.Domain.Entities;
using WorkLog.Domain.Interfaces;
using WorkLog.Infrastructure.Data;

namespace WorkLog.Infrastructure.Repositories;

/// <summary>
/// 待辦事項分類 Repository 實作
/// </summary>
public class TodoCategoryRepository : LookupRepository<TodoCategory>, ITodoCategoryRepository
{
    public TodoCategoryRepository(AppDbContext context) : base(context)
    {
    }
}
