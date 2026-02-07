namespace WorkLog.Shared.DTOs;

/// <summary>
/// 統一 API 錯誤回應格式
/// </summary>
public class ApiErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; set; }
}

/// <summary>
/// 統一 API 成功回應格式
/// </summary>
public class ApiResponse<T>
{
    public T Data { get; set; } = default!;
    public string? Message { get; set; }
}
