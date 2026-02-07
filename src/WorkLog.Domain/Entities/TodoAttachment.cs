namespace WorkLog.Domain.Entities;

/// <summary>
/// 待辦事項附件實體
/// </summary>
public class TodoAttachment
{
    public int Id { get; set; }

    /// <summary>檔案名稱</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>檔案大小（位元組）</summary>
    public long FileSize { get; set; }

    /// <summary>檔案類型（MIME Type）</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>檔案資料</summary>
    public byte[] FileData { get; set; } = Array.Empty<byte>();

    /// <summary>上傳時間</summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public int TodoItemId { get; set; }

    // Navigation properties
    public TodoItem TodoItem { get; set; } = null!;
}
