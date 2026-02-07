using System.ComponentModel.DataAnnotations;

namespace WorkLog.Shared.DTOs.Todos;

#region 請求 DTOs

/// <summary>
/// 建立待辦事項請求
/// </summary>
public class CreateTodoItemRequest
{
    [Required(ErrorMessage = "標題為必填")]
    [StringLength(200, ErrorMessage = "標題最多 200 字元")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "描述最多 2000 字元")]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    [Required(ErrorMessage = "優先順序為必填")]
    public string Priority { get; set; } = "Medium";

    public int? CategoryId { get; set; }

    public List<CreateTodoSubTaskRequest> SubTasks { get; set; } = [];
}

/// <summary>
/// 更新待辦事項請求
/// </summary>
public class UpdateTodoItemRequest
{
    [Required(ErrorMessage = "標題為必填")]
    [StringLength(200, ErrorMessage = "標題最多 200 字元")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "描述最多 2000 字元")]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    [Required(ErrorMessage = "優先順序為必填")]
    public string Priority { get; set; } = "Medium";

    [Required(ErrorMessage = "狀態為必填")]
    public string Status { get; set; } = "Pending";

    public int? CategoryId { get; set; }
}

/// <summary>
/// 建立子任務請求
/// </summary>
public class CreateTodoSubTaskRequest
{
    [Required(ErrorMessage = "子任務標題為必填")]
    [StringLength(200, ErrorMessage = "子任務標題最多 200 字元")]
    public string Title { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}

/// <summary>
/// 更新子任務請求
/// </summary>
public class UpdateTodoSubTaskRequest
{
    [Required(ErrorMessage = "子任務標題為必填")]
    [StringLength(200, ErrorMessage = "子任務標題最多 200 字元")]
    public string Title { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public int SortOrder { get; set; }
}

/// <summary>
/// 上傳附件請求
/// </summary>
public class UploadTodoAttachmentRequest
{
    [Required(ErrorMessage = "檔案名稱為必填")]
    [StringLength(255, ErrorMessage = "檔案名稱最多 255 字元")]
    public string FileName { get; set; } = string.Empty;

    [Required(ErrorMessage = "檔案大小為必填")]
    [Range(1, 5242880, ErrorMessage = "檔案大小需介於 1 位元組至 5MB")]
    public long FileSize { get; set; }

    [Required(ErrorMessage = "檔案類型為必填")]
    [StringLength(100, ErrorMessage = "檔案類型最多 100 字元")]
    public string ContentType { get; set; } = string.Empty;

    [Required(ErrorMessage = "檔案資料為必填")]
    public byte[] FileData { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// 建立評論請求
/// </summary>
public class CreateTodoCommentRequest
{
    [Required(ErrorMessage = "評論內容為必填")]
    [StringLength(1000, ErrorMessage = "評論內容最多 1000 字元")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 更新評論請求
/// </summary>
public class UpdateTodoCommentRequest
{
    [Required(ErrorMessage = "評論內容為必填")]
    [StringLength(1000, ErrorMessage = "評論內容最多 1000 字元")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 待辦事項查詢參數
/// </summary>
public class TodoQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public int? CategoryId { get; set; }
    public string? Keyword { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public string SortBy { get; set; } = "DueDate";
    public bool IsDescending { get; set; } = false;
}

#endregion

#region 回應 DTOs

/// <summary>
/// 待辦事項回應（完整）
/// </summary>
public class TodoItemResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public TodoCategoryResponse? Category { get; set; }
    public List<TodoSubTaskResponse> SubTasks { get; set; } = [];
    public List<TodoAttachmentResponse> Attachments { get; set; } = [];
    public List<TodoCommentResponse> Comments { get; set; } = [];

    // 計算屬性
    public int TotalSubTasks => SubTasks.Count;
    public int CompletedSubTasks => SubTasks.Count(s => s.IsCompleted);
    public bool IsOverdue => DueDate.HasValue && DueDate.Value.Date < DateTime.UtcNow.Date && Status != "Completed";
}

/// <summary>
/// 待辦事項回應（列表用）
/// </summary>
public class TodoItemListResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public TodoCategoryResponse? Category { get; set; }

    public int SubTaskCount { get; set; }
    public int CompletedSubTaskCount { get; set; }
    public int AttachmentCount { get; set; }
    public int CommentCount { get; set; }

    public bool IsOverdue => DueDate.HasValue && DueDate.Value.Date < DateTime.UtcNow.Date && Status != "Completed";
}

/// <summary>
/// 待辦事項分類回應
/// </summary>
public class TodoCategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ColorCode { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// 子任務回應
/// </summary>
public class TodoSubTaskResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 附件回應
/// </summary>
public class TodoAttachmentResponse
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }

    // 顯示用的檔案大小文字
    public string FileSizeText =>
        FileSize < 1024 ? $"{FileSize} B" :
        FileSize < 1048576 ? $"{FileSize / 1024.0:F2} KB" :
        $"{FileSize / 1048576.0:F2} MB";
}

/// <summary>
/// 附件回應（含檔案資料）
/// </summary>
public class TodoAttachmentWithDataResponse : TodoAttachmentResponse
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// 評論回應
/// </summary>
public class TodoCommentResponse
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}

/// <summary>
/// 儀表板摘要回應
/// </summary>
public class TodoDashboardResponse
{
    public int PendingCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int DueTodayCount { get; set; }
    public int DueThisWeekCount { get; set; }
    public int OverdueCount { get; set; }
    public List<TodoItemListResponse> DueTodayItems { get; set; } = [];
    public List<TodoItemListResponse> OverdueItems { get; set; } = [];
}

#endregion
