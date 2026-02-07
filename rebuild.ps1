# 清除所有建構輸出
Remove-Item -Recurse -Force src\WorkLog.Api\bin,src\WorkLog.Api\obj -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force src\WorkLog.Infrastructure\bin,src\WorkLog.Infrastructure\obj -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force src\WorkLog.Shared\bin,src\WorkLog.Shared\obj -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force src\WorkLog.Domain\bin,src\WorkLog.Domain\obj -ErrorAction SilentlyContinue

Write-Host "清除完成，開始重新建構..."

# 依序建構專案
dotnet build src\WorkLog.Domain\WorkLog.Domain.csproj
dotnet build src\WorkLog.Shared\WorkLog.Shared.csproj
dotnet build src\WorkLog.Infrastructure\WorkLog.Infrastructure.csproj
dotnet build src\WorkLog.Api\WorkLog.Api.csproj

Write-Host "建構完成！"
