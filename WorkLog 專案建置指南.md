

## 📋 WorkLog 專案建置指南

### 前置需求

1. **.NET SDK 8.0 或更高版本**
   ```powershell
   dotnet --version
   ```

2. **SQL Server LocalDB**（Windows 自帶）
   ```powershell
   sqllocaldb info MSSQLLocalDB
   ```

3. **Entity Framework Core Tools**
   ```powershell
   dotnet tool install --global dotnet-ef
   ```

---

### 方法一：使用提供的腳本（推薦）

#### 🔨 重新建構專案
```powershell
.\rebuild.ps1
```
此腳本會：
- 清除所有 `bin` 和 `obj` 資料夾
- 依序建構 Domain → Shared → Infrastructure → Api

#### 🚀 啟動服務
```powershell
.\start.ps1
```
此腳本會：
- 停止舊的服務程序
- 在新視窗啟動 API 服務（Port 5001）
- 在新視窗啟動 Web 服務（Port 5003）
- 自動開啟瀏覽器

#### ⏹️ 停止服務
```powershell
.\stop.ps1
```

#### ⏹️ 快速啟動方式

```dos
start.bat
```

```


---

### 方法二：手動建置

#### 1️⃣ 還原相依套件
```powershell
cd d:\開發中\worklog
dotnet restore WorkLog.slnx
```

#### 2️⃣ 建構整個解決方案
```powershell
dotnet build WorkLog.slnx
```

或依序建構各專案：
```powershell
dotnet build src\WorkLog.Domain\WorkLog.Domain.csproj
dotnet build src\WorkLog.Shared\WorkLog.Shared.csproj
dotnet build src\WorkLog.Infrastructure\WorkLog.Infrastructure.csproj
dotnet build src\WorkLog.Api\WorkLog.Api.csproj
dotnet build src\WorkLog.Web\WorkLog.Web.csproj
```

#### 3️⃣ 執行測試（選擇性）
```powershell
dotnet test
```

#### 4️⃣ 啟動後端 API
```powershell
cd src\WorkLog.Api
dotnet run
```
API 將在 `https://localhost:5001` 啟動，Swagger 文件位於：`https://localhost:5001/swagger`

#### 5️⃣ 啟動前端 Web（另開視窗）
```powershell
cd src\WorkLog.Web
dotnet run
```
Web 將在 `https://localhost:5002` 或 `http://localhost:5003` 啟動

- 或是執行start.bat會同時啟動前後端
---

### 資料庫設定

#### 初始化資料庫
資料庫會在首次執行時自動建立和遷移（Program.cs 中有 `db.Database.Migrate()`）

#### 手動建立 Migration
```powershell
dotnet ef migrations add InitialCreate `
  -p "d:\開發中\worklog\src\WorkLog.Infrastructure\WorkLog.Infrastructure.csproj" `
  -s "d:\開發中\worklog\src\WorkLog.Api\WorkLog.Api.csproj" `
  -o Data/Migrations
```

#### 重置資料庫（清空重建）
使用提供的 VS Code Task：`Reset Database`

或執行：
```powershell
sqllocaldb stop MSSQLLocalDB
sqllocaldb delete MSSQLLocalDB  
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

---

### 使用 VS Code Tasks

在 VS Code 中按 `Ctrl+Shift+P`，選擇 "Tasks: Run Task"，可使用：

- **Build Full Solution** - 建構完整解決方案
- **Start API** - 啟動 API 服務（背景）
- **Start Frontend** - 啟動前端服務（背景）
- **Reset Database** - 重置資料庫
- **Recreate Migration** - 重新建立 Migration

---

### 預設帳號

- **帳號**: `admin`
- **密碼**: 預設 `Admin@123`或請查看資料庫初始化資料或使用 `/api/auth/generate-hash` 端點產生

---

### 常見問題

**Q: 建構失敗，提示相依套件錯誤？**
```powershell
dotnet clean
dotnet restore
dotnet build
```

**Q: 資料庫連線失敗？**
確認 LocalDB 正在執行：
```powershell
sqllocaldb start MSSQLLocalDB
```

**Q: Port 被佔用？**
修改 launchSettings.json 和 launchSettings.json 中的 Port 設定。

---

### 快速開始（一鍵啟動）

```powershell
# 建構 + 啟動
.\rebuild.ps1
.\start.ps1
```

瀏覽器會自動開啟到 `http://localhost:5003`，享受使用！🎉