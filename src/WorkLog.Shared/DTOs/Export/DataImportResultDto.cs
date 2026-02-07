namespace WorkLog.Shared.DTOs.Export;

/// <summary>
/// 資料匯入結果
/// </summary>
public class DataImportResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public ImportStatisticsDto Statistics { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// 匯入統計資料
/// </summary>
public class ImportStatisticsDto
{
    // 參照資料統計
    public int ProjectsCreated { get; set; }
    public int ProjectsSkipped { get; set; }
    public int DepartmentsCreated { get; set; }
    public int DepartmentsSkipped { get; set; }
    public int WorkTypesCreated { get; set; }
    public int WorkTypesSkipped { get; set; }
    public int ProcessStatusesCreated { get; set; }
    public int ProcessStatusesSkipped { get; set; }
    public int TodoCategoriesCreated { get; set; }
    public int TodoCategoriesSkipped { get; set; }

    // 工作紀錄統計
    public int WorkLogsImported { get; set; }
    public int WorkLogsFailed { get; set; }

    // 待辦事項統計
    public int TodosImported { get; set; }
    public int TodosFailed { get; set; }
    public int SubTasksImported { get; set; }
    public int CommentsImported { get; set; }
    public int AttachmentsImported { get; set; }
}
