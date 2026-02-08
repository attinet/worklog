# 清理並重建 WorkLog.Web 專案
# 解決 Blazor WebAssembly integrity 檢查失敗問題

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "開始清理建構產物..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 清理 WorkLog.Web
Write-Host "[1/4] 清理 WorkLog.Web..." -ForegroundColor Yellow
if (Test-Path "src\WorkLog.Web\bin") {
    Remove-Item -Recurse -Force "src\WorkLog.Web\bin"
    Write-Host "  ✓ 已刪除 bin 資料夾" -ForegroundColor Green
}
if (Test-Path "src\WorkLog.Web\obj") {
    Remove-Item -Recurse -Force "src\WorkLog.Web\obj"
    Write-Host "  ✓ 已刪除 obj 資料夾" -ForegroundColor Green
}

# 清理 WorkLog.Shared
Write-Host "[2/4] 清理 WorkLog.Shared..." -ForegroundColor Yellow
if (Test-Path "src\WorkLog.Shared\bin") {
    Remove-Item -Recurse -Force "src\WorkLog.Shared\bin"
    Write-Host "  ✓ 已刪除 bin 資料夾" -ForegroundColor Green
}
if (Test-Path "src\WorkLog.Shared\obj") {
    Remove-Item -Recurse -Force "src\WorkLog.Shared\obj"
    Write-Host "  ✓ 已刪除 obj 資料夾" -ForegroundColor Green
}

# 清理 WorkLog.Domain
Write-Host "[3/4] 清理 WorkLog.Domain..." -ForegroundColor Yellow
if (Test-Path "src\WorkLog.Domain\bin") {
    Remove-Item -Recurse -Force "src\WorkLog.Domain\bin"
    Write-Host "  ✓ 已刪除 bin 資料夾" -ForegroundColor Green
}
if (Test-Path "src\WorkLog.Domain\obj") {
    Remove-Item -Recurse -Force "src\WorkLog.Domain\obj"
    Write-Host "  ✓ 已刪除 obj 資料夾" -ForegroundColor Green
}

Write-Host ""
Write-Host "[4/4] 重新建構 WorkLog.Web..." -ForegroundColor Yellow
dotnet build "src\WorkLog.Web\WorkLog.Web.csproj" --configuration Debug --no-incremental

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "完成！請重新啟動應用程式" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "提示：" -ForegroundColor Yellow
Write-Host "1. 使用 start.bat 啟動應用程式" -ForegroundColor White
Write-Host "2. 如果瀏覽器仍有問題，請按 Ctrl+Shift+Delete 清除瀏覽器快取" -ForegroundColor White
Write-Host "3. 或使用無痕模式開啟瀏覽器 (Ctrl+Shift+N)" -ForegroundColor White
Write-Host "4. 在瀏覽器中按 Ctrl+Shift+R 進行硬重新整理" -ForegroundColor White
