using WorkLog.Domain.Entities;

namespace WorkLog.Domain.Interfaces;

/// <summary>
/// 待辦事項分類 Repository 介面
/// </summary>
public interface ITodoCategoryRepository : ILookupRepository<TodoCategory>
{
    // 繼承 ILookupRepository 的所有方法
    // 可根據需要擴充特定方法
}
