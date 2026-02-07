using System.ComponentModel.DataAnnotations;

namespace WorkLog.Shared.DTOs;

/// <summary>
/// 查詢表項目（共用於專案、業管單位、工作類型、處理狀態）
/// </summary>
public class LookupItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

/// <summary>
/// 建立/編輯 查詢表項目請求
/// </summary>
public class LookupItemRequest
{
    [Required(ErrorMessage = "名稱為必填")]
    [StringLength(100, ErrorMessage = "名稱最多 100 字元")]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
