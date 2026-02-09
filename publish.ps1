# WorkLog 整合發佈腳本
# 用途：將前後端打包成單一可部署到 IIS 的資料夾

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = "publish"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "WorkLog 前後端整合發佈" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 清理舊的發佈資料夾
if (Test-Path $OutputPath) {
    Write-Host "清理舊的發佈資料夾..." -ForegroundColor Yellow
    Remove-Item $OutputPath -Recurse -Force
}

# 發佈 API（會自動觸發 Blazor 前端發佈）
Write-Host "開始發佈..." -ForegroundColor Green
Write-Host "組態: $Configuration" -ForegroundColor Gray
Write-Host "輸出: $OutputPath" -ForegroundColor Gray
Write-Host ""

dotnet publish src\WorkLog.Api -c $Configuration -o $OutputPath

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "✓ 發佈成功！" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "發佈位置: $((Get-Item $OutputPath).FullName)" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "IIS 部署步驟：" -ForegroundColor Yellow
    Write-Host "1. 確保伺服器已安裝 .NET Hosting Bundle" -ForegroundColor White
    Write-Host "2. 在 IIS 建立新網站，指向發佈資料夾" -ForegroundColor White
    Write-Host "3. Application Pool 設為 No Managed Code" -ForegroundColor White
    Write-Host "4. 修改 appsettings.json 中的連線字串與設定" -ForegroundColor White
    Write-Host "5. 瀏覽器開啟網站測試" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "✗ 發佈失敗，請檢查錯誤訊息" -ForegroundColor Red
    exit 1
}
