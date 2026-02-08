# 清除 git 版控中的 bin 和 obj 資料夾
Write-Host "開始清理 bin 和 obj 資料夾..." -ForegroundColor Cyan

# 方法1: 嘗試直接移除特定路徑
Write-Host "`n方法1: 移除特定路徑..." -ForegroundColor Yellow
$paths = @(
    "src/WorkLog.Api/bin",
    "src/WorkLog.Api/obj",
    "src/WorkLog.Domain/bin",
    "src/WorkLog.Domain/obj",
    "src/WorkLog.Infrastructure/bin",
    "src/WorkLog.Infrastructure/obj",
    "src/WorkLog.Shared/bin",
    "src/WorkLog.Shared/obj",
    "src/WorkLog.Web/bin",
    "src/WorkLog.Web/obj",
    "temp/HashTest/bin",
    "temp/HashTest/obj",
    "tests/WorkLog.Api.Tests/bin",
    "tests/WorkLog.Api.Tests/obj",
    "tests/WorkLog.Domain.Tests/bin",
    "tests/WorkLog.Domain.Tests/obj",
    "tests/WorkLog.Infrastructure.Tests/bin",
    "tests/WorkLog.Infrastructure.Tests/obj",
    "tests/WorkLog.Web.Tests/bin",
    "tests/WorkLog.Web.Tests/obj"
)

$removed = 0
foreach ($path in $paths) {
    $result = git rm -r --cached $path 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  已移除: $path" -ForegroundColor Green
        $removed++
    }
}

if ($removed -eq 0) {
    Write-Host "`n方法1 沒有移除任何檔案，嘗試方法2..." -ForegroundColor Yellow
    Write-Host "`n方法2: 重置並重新加入所有檔案..." -ForegroundColor Yellow
    
    # 備份當前暫存區
    git diff --cached > staged_changes.patch 2>$null
    
    # 清除所有已追蹤的檔案
    Write-Host "  清除索引..." -ForegroundColor Cyan
    git rm -r --cached . 2>&1 | Out-Null
    
    # 重新加入所有檔案（會遵守 .gitignore）
    Write-Host "  重新加入檔案..." -ForegroundColor Cyan
    git add .
    
    Write-Host "`n完成重置！" -ForegroundColor Green
} else {
    Write-Host "`n成功移除 $removed 個路徑" -ForegroundColor Green
    # 更新索引
    git add .gitignore
}

# 顯示狀態
Write-Host "`n當前狀態:" -ForegroundColor Cyan
git status -s

Write-Host "`n接下來請執行:" -ForegroundColor Yellow
Write-Host "  git commit -m `"Remove bin and obj folders from version control`"" -ForegroundColor White
