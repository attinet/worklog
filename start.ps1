#!/usr/bin/env pwsh
# WorkLog 系統啟動腳本

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "工作紀錄系統 - 啟動腳本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 先停止舊的服務
Write-Host "[0/3] 清理舊的服務程序..." -ForegroundColor Yellow
& "$PSScriptRoot\stop.ps1"
Start-Sleep -Seconds 2

Write-Host "[1/2] 正在啟動後端 API 服務..." -ForegroundColor Yellow
$apiPath = Join-Path $PSScriptRoot "src\WorkLog.Api"
Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$apiPath'; dotnet run" -WindowStyle Normal
Start-Sleep -Seconds 3

Write-Host "[2/2] 正在啟動前端 Web 服務..." -ForegroundColor Yellow
$webPath = Join-Path $PSScriptRoot "src\WorkLog.Web"
Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$webPath'; dotnet run" -WindowStyle Normal

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "服務啟動中，請稍候..." -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "後端 API: http://localhost:5001" -ForegroundColor Green
Write-Host "前端 Web: http://localhost:5003" -ForegroundColor Green
Write-Host ""
Write-Host "提示：" -ForegroundColor Cyan
Write-Host "- 等待約 10-15 秒讓服務完全啟動"
Write-Host "- 兩個 PowerShell 視窗會自動開啟"
Write-Host "- 關閉視窗或按 Ctrl+C 即可停止服務"
Write-Host ""
Write-Host "按任意鍵開啟瀏覽器..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

Start-Process "http://localhost:5003"

Write-Host ""
Write-Host "瀏覽器已開啟！" -ForegroundColor Green
Write-Host "按任意鍵關閉此視窗..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
