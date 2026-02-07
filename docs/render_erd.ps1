# 產生 ERD 圖檔（需安裝 Node.js 與 @mermaid-js/mermaid-cli）
# Windows PowerShell 範例：
# npx @mermaid-js/mermaid-cli -i docs/erd.mmd -o docs/erd.svg
# npx @mermaid-js/mermaid-cli -i docs/erd.mmd -o docs/erd.png

param(
    [string]$Input = "docs/erd.mmd",
    [string]$Output = "docs/erd.svg"
)

Write-Host "產生 ERD 圖檔： $Input -> $Output"

# 使用 npx（會自動安裝執行）
$npx = "npx @mermaid-js/mermaid-cli -i $Input -o $Output"
Write-Host $npx
iex $npx
