using System.ComponentModel.DataAnnotations;

namespace WorkLog.Shared.DTOs.WorkLogs;

/// <summary>
/// 建立/編輯 工作紀錄請求
/// </summary>
public class WorkLogRequest
{
    [Required(ErrorMessage = "標題為必填")]
    [StringLength(200, ErrorMessage = "標題最多 200 字元")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "專案名稱為必填")]
    public int ProjectId { get; set; }

    [Required(ErrorMessage = "內文為必填")]
    [StringLength(5000, ErrorMessage = "內文最多 5000 字元")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "紀錄日期為必填")]
    public DateOnly RecordDate { get; set; }

    [Required(ErrorMessage = "業管單位為必填")]
    [MinLength(1, ErrorMessage = "請至少選擇一個業管單位")]
    public List<int> DepartmentIds { get; set; } = [];

    [Required(ErrorMessage = "工作類型為必填")]
    [MinLength(1, ErrorMessage = "請至少選擇一個工作類型")]
    public List<int> WorkTypeIds { get; set; } = [];

    [Required(ErrorMessage = "工時為必填")]
    [Range(0.1, 24, ErrorMessage = "工時需介於 0.1 至 24 小時")]
    public decimal WorkHours { get; set; }

    [Required(ErrorMessage = "處理狀態為必填")]
    public int ProcessStatusId { get; set; }
}

/// <summary>
/// 工作紀錄回應
/// </summary>
public class WorkLogResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateOnly RecordDate { get; set; }
    public decimal WorkHours { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public LookupItemDto Project { get; set; } = null!;
    public LookupItemDto ProcessStatus { get; set; } = null!;
    public List<LookupItemDto> Departments { get; set; } = [];
    public List<LookupItemDto> WorkTypes { get; set; } = [];
}

/// <summary>
/// 工作紀錄查詢參數
/// </summary>
public class WorkLogQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? ProjectId { get; set; }
    public int? ProcessStatusId { get; set; }
    public string? Keyword { get; set; }
}

/// <summary>
/// 分頁回應
/// </summary>
public class PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
