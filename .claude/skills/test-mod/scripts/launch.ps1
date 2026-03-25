# launch.ps1 — Start RimWorld as a detached process and print its PID to stdout.
#
# Usage:
#   launch.ps1 -ExePath <path-to-exe>
#
# Prints the PID on a single line so the caller can capture it:
#   $pid = powershell ... launch.ps1 -ExePath ...

param(
    [Parameter(Mandatory)] [string] $ExePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $ExePath)) {
    Write-Error "RimWorld executable not found: $ExePath"
    exit 1
}

$proc = Start-Process -FilePath $ExePath -PassThru
Write-Host $proc.Id
