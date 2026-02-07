using WorkLog.Domain.Enums;

namespace WorkLog.Shared.DTOs.Export;

/// <summary>
/// 待辦事項匯出物件
/// </summary>
public class TodoExportDto
{
    public int OriginalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public TodoStatus Status { get; set; }
    public TodoPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // 分類參照 (nullable)
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    // 子項目
    public List<TodoSubTaskExportDto> SubTasks { get; set; } = new();
    public List<TodoCommentExportDto> Comments { get; set; } = new();
    public List<TodoAttachmentExportDto> Attachments { get; set; } = new();
}

/// <summary>
/// 待辦子任務匯出物件
/// </summary>
public class TodoSubTaskExportDto
{
    public int OriginalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 待辦評論匯出物件
/// </summary>
public class TodoCommentExportDto
{
    public int OriginalId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Username { get; set; } = string.Empty; // 評論者使用者名稱
}

/// <summary>
/// 待辦附件匯出物件 (中繼資料)
/// </summary>
public class TodoAttachmentExportDto
{
    public int OriginalId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }

    /// <summary>附件在 ZIP 中的相對路徑 (如果包含附件)</summary>
    public string? FilePath { get; set; }
}
