# 開發環境前後端整合設定
# 用途：在開發模式下也能測試整合部署的效果

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "設定開發環境整合" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 發佈 Blazor WASM 前端
Write-Host "正在發佈 Blazor WASM 前端..." -ForegroundColor Yellow
dotnet publish src\WorkLog.Web -c Debug

if ($LASTEXITCODE -ne 0) {
    Write-Host "前端發佈失敗" -ForegroundColor Red
    exit 1
}

# 建立 API 的 wwwroot 資料夾（如果不存在）
$wwwrootPath = "src\WorkLog.Api\wwwroot"
if (-not (Test-Path $wwwrootPath)) {
    New-Item -ItemType Directory -Path $wwwrootPath | Out-Null
}

# 複製前端發佈檔案到 API 的 wwwroot
Write-Host "複製前端檔案到 API wwwroot..." -ForegroundColor Yellow
$sourcePath = "src\WorkLog.Web\bin\Debug\net10.0\publish\wwwroot\*"
Copy-Item -Path $sourcePath -Destination $wwwrootPath -Recurse -Force

Write-Host ""
Write-Host "✓ 設定完成！" -ForegroundColor Green
Write-Host ""
Write-Host "現在可以執行：" -ForegroundColor Cyan
Write-Host "  dotnet run --project src\WorkLog.Api" -ForegroundColor White
Write-Host ""
Write-Host "然後開啟瀏覽器訪問：http://localhost:5001" -ForegroundColor Cyan
