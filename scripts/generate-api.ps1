#Requires -Version 7.0
<#
.SYNOPSIS
  Generates the Kiota API client from the running backend OpenAPI document.

.DESCRIPTION
  1. Start the backend: dotnet run --project src/backend
  2. Run this script from the repository root: ./scripts/generate-api.ps1

  Output: src/frontend/TrackrApi
  Rebuild the frontend after regenerating the client.
#>
param(
    [string]$OpenApiUrl = "http://localhost:5080/openapi/v1.json",
    [string]$OutputPath = "src/frontend/TrackrApi",
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
