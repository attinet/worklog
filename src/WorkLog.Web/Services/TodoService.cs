using System.Net.Http.Json;
using WorkLog.Shared.DTOs.Todos;
using WorkLog.Shared.DTOs.WorkLogs;

namespace WorkLog.Web.Services;

/// <summary>
/// 待辦事項服務
/// </summary>
public class TodoService
{
    private readonly HttpClient _httpClient;

    public TodoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    #region 主要待辦事項操作

    /// <summary>
    /// 查詢待辦事項列表
    /// </summary>
    public async Task<PagedResponse<TodoItemListResponse>?> GetTodosAsync(TodoQueryDto query)
    {
        var queryString = $"?Page={query.Page}&PageSize={query.PageSize}&SortBy={query.SortBy}&IsDescending={query.IsDescending}";
        
        if (!string.IsNullOrWhiteSpace(query.Status))
            queryString += $"&Status={query.Status}";
        
        if (!string.IsNullOrWhiteSpace(query.Priority))
            queryString += $"&Priority={query.Priority}";
        
        if (query.CategoryId.HasValue)
            queryString += $"&CategoryId={query.CategoryId.Value}";
        
        if (!string.IsNullOrWhiteSpace(query.Keyword))
            queryString += $"&Keyword={Uri.EscapeDataString(query.Keyword)}";
        
        if (query.DueDateFrom.HasValue)
            queryString += $"&DueDateFrom={query.DueDateFrom.Value:yyyy-MM-ddTHH:mm:ss}";
        
        if (query.DueDateTo.HasValue)
            queryString += $"&DueDateTo={query.DueDateTo.Value:yyyy-MM-ddTHH:mm:ss}";

        var response = await _httpClient.GetAsync($"api/todos{queryString}");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<PagedResponse<TodoItemListResponse>>()
            : null;
    }

    /// <summary>
    /// 取得單一待辦事項
    /// </summary>
    public async Task<TodoItemResponse?> GetTodoByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/todos/{id}");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<TodoItemResponse>()
            : null;
    }

    /// <summary>
    /// 取得儀表板摘要
    /// </summary>
    public async Task<TodoDashboardResponse?> GetDashboardAsync()
    {
        var response = await _httpClient.GetAsync("api/todos/dashboard");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<TodoDashboardResponse>()
            : null;
    }

    /// <summary>
    /// 建立待辦事項
    /// </summary>
    public async Task<TodoItemResponse?> CreateTodoAsync(CreateTodoItemRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/todos", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<TodoItemResponse>()
            : null;
    }

    /// <summary>
    /// 更新待辦事項
    /// </summary>
    public async Task<bool> UpdateTodoAsync(int id, UpdateTodoItemRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/todos/{id}", request);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// 刪除待辦事項
    /// </summary>
    public async Task<bool> DeleteTodoAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/todos/{id}");
        return response.IsSuccessStatusCode;
    }

    #endregion

    #region 子任務操作

    /// <summary>
    /// 新增子任務
    /// </summary>
    public async Task<TodoSubTaskResponse?> AddSubTaskAsync(int todoId, CreateTodoSubTaskRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/todos/{todoId}/subtasks", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<TodoSubTaskResponse>()
            : null;
    }

    /// <summary>
    /// 更新子任務
    /// </summary>
    public async Task<bool> UpdateSubTaskAsync(int todoId, int subTaskId, UpdateTodoSubTaskRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/todos/{todoId}/subtasks/{subTaskId}", request);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// 刪除子任務
    /// </summary>
    public async Task<bool> DeleteSubTaskAsync(int todoId, int subTaskId)
    {
        var response = await _httpClient.DeleteAsync($"api/todos/{todoId}/subtasks/{subTaskId}");
        return response.IsSuccessStatusCode;
    }

    #endregion

    #region 附件操作

    /// <summary>
    /// 上傳附件
    /// </summary>
    public async Task<TodoAttachmentResponse?> UploadAttachmentAsync(int todoId, UploadTodoAttachmentRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/todos/{todoId}/attachments", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<TodoAttachmentResponse>()
            : null;
    }

    /// <summary>
    /// 下載附件
    /// </summary>
    public async Task<byte[]?> DownloadAttachmentAsync(int todoId, int attachmentId)
    {
        var response = await _httpClient.GetAsync($"api/todos/{todoId}/attachments/{attachmentId}/download");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadAsByteArrayAsync()
            : null;
    }

    /// <summary>
    /// 刪除附件
    /// </summary>
    public async Task<bool> DeleteAttachmentAsync(int todoId, int attachmentId)
    {
        var response = await _httpClient.DeleteAsync($"api/todos/{todoId}/attachments/{attachmentId}");
        return response.IsSuccessStatusCode;
    }

    #endregion

    #region 評論操作

    /// <summary>
    /// 新增評論
    /// </summary>
    public async Task<TodoCommentResponse?> AddCommentAsync(int todoId, CreateTodoCommentRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/todos/{todoId}/comments", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<TodoCommentResponse>()
            : null;
    }

    /// <summary>
    /// 更新評論
    /// </summary>
    public async Task<bool> UpdateCommentAsync(int todoId, int commentId, UpdateTodoCommentRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/todos/{todoId}/comments/{commentId}", request);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// 刪除評論
    /// </summary>
    public async Task<bool> DeleteCommentAsync(int todoId, int commentId)
    {
        var response = await _httpClient.DeleteAsync($"api/todos/{todoId}/comments/{commentId}");
        return response.IsSuccessStatusCode;
    }

    #endregion

    #region 分類操作

    /// <summary>
    /// 取得所有分類
    /// </summary>
    public async Task<List<TodoCategoryResponse>?> GetCategoriesAsync()
    {
        var response = await _httpClient.GetAsync("api/todocategories");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<TodoCategoryResponse>>()
            : null;
    }

    #endregion
}
