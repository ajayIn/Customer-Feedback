# Starts Vite dev server (proxies /api to gateway on :5000).
# Requires frontend/.env.development with VITE_USE_MSW=false

$ErrorActionPreference = "Stop"
$repo = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location (Join-Path $repo "frontend")

if (-not (Test-Path "node_modules")) {
    npm.cmd install
}

Write-Host "UI: http://localhost:5173 (API via proxy -> http://localhost:5000)"
npm.cmd run dev
