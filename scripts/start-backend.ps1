# Starts Identity, Feedback, Reporting, and Gateway (live API demo).
# Run from repo root:  .\scripts\start-backend.ps1

$ErrorActionPreference = "Stop"
$repo = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repo

$env:ASPNETCORE_ENVIRONMENT = "Development"

Write-Host "Building solution..."
dotnet build backend/CustomerFeedback.slnx -v q
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

function Start-ServiceWindow {
    param(
        [string]$Title,
        [string]$ProjectPath,
        [int]$Port
    )
    $project = Join-Path $repo $ProjectPath
    $args = @(
        "run", "--no-build", "--no-launch-profile",
        "--urls", "http://localhost:$Port",
        "--project", $project
    )
    Start-Process -FilePath "dotnet" -ArgumentList $args -WorkingDirectory $repo -WindowStyle Normal
    Write-Host "Started $Title on http://localhost:$Port"
}

Start-ServiceWindow -Title "Identity"   -ProjectPath "backend\services\identity\Identity.Api\Identity.Api.csproj"   -Port 5101
Start-Sleep -Seconds 2
Start-ServiceWindow -Title "Feedback"   -ProjectPath "backend\services\feedback\Feedback.Api\Feedback.Api.csproj"   -Port 5102
Start-Sleep -Seconds 1
Start-ServiceWindow -Title "Reporting" -ProjectPath "backend\services\reporting\Reporting.Api\Reporting.Api.csproj" -Port 5103
Start-Sleep -Seconds 1
Start-ServiceWindow -Title "Gateway"    -ProjectPath "backend\gateway\Gateway\Gateway.csproj"                        -Port 5000

Write-Host ""
Write-Host "Gateway (API entry): http://localhost:5000/health"
Write-Host "Ensure RabbitMQ is running on localhost:5672 and CF_* databases exist on AJAYPC."
