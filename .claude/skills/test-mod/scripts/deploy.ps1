# deploy.ps1 — Copy a built mod into RimWorld's Mods directory.
#
# Usage:
#   deploy.ps1 -ModDir <relative-mod-path> -TargetDir <absolute-target-path>
#
# Example:
#   deploy.ps1 -ModDir mods/fishing-is-fun -TargetDir "E:\RimWorld\Mods\fishing-is-fun"

param(
    [Parameter(Mandatory)] [string] $ModDir,
    [Parameter(Mandatory)] [string] $TargetDir
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Wipe and recreate so no stale files carry over between runs.
if (Test-Path $TargetDir) {
    Remove-Item $TargetDir -Recurse -Force
}
New-Item -ItemType Directory -Path $TargetDir | Out-Null

# About/ — always required.
Copy-Item "$ModDir\About" "$TargetDir\About" -Recurse
Write-Host "  Copied About/"

# Versioned folders (e.g. 1.6/, 1.7/) — any top-level directory whose name is N.N[.N].
Get-ChildItem $ModDir -Directory |
    Where-Object { $_.Name -match '^\d+\.\d+' } |
    ForEach-Object {
        Copy-Item $_.FullName "$TargetDir\$($_.Name)" -Recurse
        Write-Host "  Copied $($_.Name)/"
    }

# Textures/ — optional.
if (Test-Path "$ModDir\Textures") {
    Copy-Item "$ModDir\Textures" "$TargetDir\Textures" -Recurse
    Write-Host "  Copied Textures/"
}

Write-Host "Deploy complete -> $TargetDir"
