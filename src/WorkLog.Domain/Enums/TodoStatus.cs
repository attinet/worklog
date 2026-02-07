namespace WorkLog.Domain.Enums;

/// <summary>
/// 待辦事項的狀態
/// </summary>
public enum TodoStatus
{
    /// <summary>
    /// 待處理
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 進行中
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 2
}
