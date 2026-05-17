#Requires -Version 7.0
<#
.SYNOPSIS
  Generates the Kiota API client from the running backend OpenAPI document.

.DESCRIPTION
  1. Start the backend: dotnet run --project src/backend
  2. Run this script from the repository root: ./scripts/generate-api.ps1

  Output: src/frontend/Generated/Api (gitignored)
  Rebuild the frontend to enable KIOTA_GENERATED and the Kiota health probe.
#>
param(
    [string]$OpenApiUrl = "https://localhost:7120/swagger/v1/swagger.json",
    [string]$OutputPath = "src/frontend/Generated/Api",
    [string]$ClientClassName = "TrackrApiClient",
    [string]$Namespace = "Trackr.Api"
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

Write-Host "Restoring dotnet tools..."
dotnet tool restore | Out-Null

if (Test-Path $OutputPath) {
    Remove-Item -Recurse -Force $OutputPath
}

Write-Host "Generating Kiota client from $OpenApiUrl ..."
dotnet kiota generate `
    --language CSharp `
    --openapi $OpenApiUrl `
    --output $OutputPath `
    --class-name $ClientClassName `
    --namespace-name $Namespace `
    --clean-output

if ($LASTEXITCODE -ne 0) {
    throw "Kiota generation failed with exit code $LASTEXITCODE"
}

Write-Host "Done. Rebuild the frontend: dotnet build src/frontend"
