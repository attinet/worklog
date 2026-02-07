using Microsoft.EntityFrameworkCore;
using WorkLog.Domain.Interfaces;
using WorkLog.Infrastructure.Data;

namespace WorkLog.Infrastructure.Repositories;

/// <summary>
/// 通用查詢表 Repository 實作
/// </summary>
public class LookupRepository<T> : ILookupRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public LookupRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public async Task<IReadOnlyList<T>> GetAllAsync()
        => await _dbSet.AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<T>> GetActiveAsync()
    {
        // 使用 dynamic 來處理 IsActive 屬性
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public async Task<T> CreateAsync(T entity)
    {
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
