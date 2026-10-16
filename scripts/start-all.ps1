# Starts backend in separate windows, then frontend in this terminal.
$ErrorActionPreference = "Stop"
& (Join-Path $PSScriptRoot "start-backend.ps1")
Start-Sleep -Seconds 8
& (Join-Path $PSScriptRoot "start-frontend.ps1")
