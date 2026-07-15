<#
.SYNOPSIS
  Merge useful npm scripts into an Angular project's package.json and optionally install dev packages.

.DESCRIPTION
  - Backs up the existing package.json to package.json.bak.<timestamp>
  - Adds/merges the following scripts (won't overwrite existing scripts unless -Force is used):
      "cypress:open" -> "cypress open"
      "cypress:run"  -> "cypress run"
      "test:unit"    -> "ng test --watch=false"
      "test:e2e"     -> "ng e2e"
  - Optionally runs `npm install --save-dev cypress` when -InstallCypress is specified.

.PARAMETER PackageJsonPath
  Path to package.json. Default is the client project package.json in this repo.

.PARAMETER Force
  Overwrite existing script entries when present.

.PARAMETER InstallCypress
  Runs `npm install --save-dev cypress` in the package.json directory after updating scripts.

.EXAMPLE
  .\add_npm_scripts.ps1 -InstallCypress

•	Open PowerShell as needed and execute:
  •	cd C:\Users\avery\Source\Repos\Angular_Products_App\Angular_Products_App.client\scripts
  •	.\add_npm_scripts.ps1            # merge scripts without overwriting
  •	.\add_npm_scripts.ps1 -Force    # force-overwrite existing entries
  •	.\add_npm_scripts.ps1 -InstallCypress  # also install Cypress dev package
#> 

param(
    [string]$PackageJsonPath = "$PSScriptRoot\..\package.json",
    [switch]$Force,
    [switch]$InstallCypress
)

$ErrorActionPreference = 'Stop'

# Resolve and validate
$pkgPath = Resolve-Path -Path $PackageJsonPath -ErrorAction SilentlyContinue
if (-not $pkgPath) {
    Write-Error "package.json not found at path: $PackageJsonPath"
    exit 1
}
$pkgPath = $pkgPath.Path

# Backup
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$backup = "$pkgPath.bak.$timestamp"
Copy-Item -Path $pkgPath -Destination $backup -Force
Write-Host "Backed up package.json to $backup"

# Read and parse
$jsonText = Get-Content -Path $pkgPath -Raw -ErrorAction Stop
try {
    $pkg = $jsonText | ConvertFrom-Json -ErrorAction Stop
} catch {
    Write-Error "Failed to parse package.json: $_"
    exit 1
}

if (-not $pkg.PSObject.Properties.Name -contains 'scripts' -or $null -eq $pkg.scripts) {
    $pkg | Add-Member -MemberType NoteProperty -Name scripts -Value (@{}) -Force
}

# Scripts to add
$newScripts = @{
    'cypress:open' = 'cypress open'
    'cypress:run'  = 'cypress run'
    'test:unit'    = 'ng test --watch=false'
    'test:e2e'     = 'ng e2e'
}

foreach ($k in $newScripts.Keys) {
    if ($pkg.scripts.PSObject.Properties.Name -contains $k) {
        if ($Force) {
            $pkg.scripts.$k = $newScripts[$k]
            Write-Host "Overwrote script '$k' with '$($newScripts[$k])'"
        } else {
            Write-Host "Script '$k' already exists. Use -Force to overwrite."
        }
    } else {
        $pkg.scripts | Add-Member -NotePropertyName $k -NotePropertyValue $newScripts[$k]
        Write-Host "Added script '$k' -> '$($newScripts[$k])'"
    }
}

# Write back with reasonable formatting
# ConvertTo-Json sometimes orders properties; depth 10 preserves nested objects
$pkg | ConvertTo-Json -Depth 10 | Out-File -FilePath $pkgPath -Encoding utf8

Write-Host "Updated package.json at $pkgPath"

# Optionally install Cypress
if ($InstallCypress) {
    $pkgDir = Split-Path -Path $pkgPath -Parent
    Write-Host "Running 'npm install --save-dev cypress' in $pkgDir ..."
    Push-Location $pkgDir
    try {
        & npm install --save-dev cypress
        Write-Host "npm install finished."
    } catch {
        Write-Warning "npm install failed: $_"
    } finally {
        Pop-Location
    }
}

Write-Host "Done."
