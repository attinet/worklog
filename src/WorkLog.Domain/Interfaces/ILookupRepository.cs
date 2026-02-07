namespace WorkLog.Domain.Interfaces;

/// <summary>
/// 通用查詢表項目 Repository 介面
/// 適用於 Project、Department、WorkType、ProcessStatus 的 CRUD 操作
/// </summary>
public interface ILookupRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> GetActiveAsync();
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
