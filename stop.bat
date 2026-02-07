@echo off
chcp 65001 >nul
echo ========================================
echo 工作紀錄系統 - 停止腳本
echo ========================================
echo.

echo 正在停止所有服務...

REM 方法 1: 根據視窗標題停止
taskkill /FI "WindowTitle eq WorkLog API*" /F >nul 2>&1
taskkill /FI "WindowTitle eq WorkLog Web*" /F >nul 2>&1

REM 方法 2: 根據 Port 停止程序
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":5001.*LISTENING"') do (
    echo 正在停止 Port 5001 的程序 (PID: %%a^)...
    taskkill /F /PID %%a >nul 2>&1
)

for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":5003.*LISTENING"') do (
    echo 正在停止 Port 5003 的程序 (PID: %%a^)...
    taskkill /F /PID %%a >nul 2>&1
)

REM 方法 3: 停止工作區內的 dotnet 程序
powershell -Command "Get-Process -Name dotnet* -ErrorAction SilentlyContinue | Where-Object {$_.Path -like '*worklog*'} | Stop-Process -Force"

echo [✓] 所有服務已停止
echo.
echo ========================================
echo 清理完成！
echo ========================================
echo.
timeout /t 2 /nobreak >nul
