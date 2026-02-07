@echo off
chcp 65001 >nul
echo ========================================
echo 工作紀錄系統 - 啟動腳本
echo ========================================
echo.

REM 先停止舊的服務
echo [0/3] 清理舊的服務程序...
call "%~dp0stop.bat" >nul
timeout /t 2 /nobreak >nul

echo [1/2] 正在啟動後端 API 服務...
start "WorkLog API" cmd /k "cd /d %~dp0src\WorkLog.Api && dotnet run"
timeout /t 3 /nobreak >nul

echo [2/2] 正在啟動前端 Web 服務...
start "WorkLog Web" cmd /k "cd /d %~dp0src\WorkLog.Web && dotnet run"

echo.
echo ========================================
echo 服務啟動中，請稍候...
echo ========================================
echo.
echo 後端 API: http://localhost:5001
echo 前端 Web: http://localhost:5003
echo.
echo 提示：
echo - 等待約 10-15 秒讓服務完全啟動
echo - 兩個命令視窗會自動開啟
echo - 關閉命令視窗即可停止服務
echo.
echo 按任意鍵開啟瀏覽器...
pause >nul

start http://localhost:5003

echo.
echo 瀏覽器已開啟！
echo 按任意鍵關閉此視窗...
pause >nul
