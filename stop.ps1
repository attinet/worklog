#!/usr/bin/env pwsh
# WorkLog 系統停止腳本

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "工作紀錄系統 - 停止腳本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "正在停止所有服務..." -ForegroundColor Yellow
Write-Host ""

# 方法 1: 根據 Port 停止
$ports = @(5001, 5003)
foreach ($port in $ports) {
    try {
        $connections = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
        if ($connections) {
            foreach ($conn in $connections) {
                $pid = $conn.OwningProcess
                $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
                if ($process) {
                    Write-Host "[停止] Port $port - PID: $pid ($($process.ProcessName))" -ForegroundColor Yellow
                    Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
                    Start-Sleep -Milliseconds 500
                    Write-Host "[✓] Port $port 已釋放" -ForegroundColor Green
                }
            }
        }
    } catch {
        # 忽略錯誤
    }
}

# 方法 2: 停止 worklog 路徑下的 dotnet 程序
Write-Host ""
Write-Host "清理 worklog 相關的 dotnet 程序..." -ForegroundColor Yellow
$worklogProcesses = Get-Process -Name dotnet -ErrorAction SilentlyContinue | 
    Where-Object { $_.Path -like '*worklog*' -or $_.CommandLine -like '*WorkLog*' }

if ($worklogProcesses) {
    foreach ($proc in $worklogProcesses) {
        Write-Host "[停止] PID: $($proc.Id) - $($proc.ProcessName)" -ForegroundColor Yellow
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Seconds 1
    Write-Host "[✓] 所有 dotnet 程序已清理" -ForegroundColor Green
} else {
    Write-Host "[✓] 沒有發現執行中的 dotnet 程序" -ForegroundColor Green
}

# 方法 3: 清理可能殘留的 apphost 檔案鎖定
Write-Host ""
Write-Host "檢查檔案鎖定..." -ForegroundColor Yellow
$apiExe = "D:\開發中\worklog\src\WorkLog.Api\bin\Debug\net10.0\WorkLog.Api.exe"
$webExe = "D:\開發中\worklog\src\WorkLog.Web\bin\Debug\net10.0\WorkLog.Web.exe"

foreach ($exePath in @($apiExe, $webExe)) {
    if (Test-Path $exePath) {
        try {
            $processes = Get-Process | Where-Object { $_.Path -eq $exePath }
            if ($processes) {
                foreach ($proc in $processes) {
                    Write-Host "[停止] 鎖定的執行檔: $($proc.ProcessName) (PID: $($proc.Id))" -ForegroundColor Yellow
                    Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
                }
            }
        } catch {
            # 忽略錯誤
        }
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "清理完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Start-Sleep -Seconds 2
