# poll-log.ps1 — Wait for RimWorld's mod load sequence to complete.
#
# Polls Player.log every 2 seconds until all five Unity GC markers appear,
# indicating that the full mod load cycle has finished. Exits 0 on success,
# 1 on timeout.
#
# Usage:
#   poll-log.ps1 -LogFile <path-to-Player.log> [-TimeoutSecs <n>]

param(
    [Parameter(Mandatory)] [string] $LogFile,
    [int] $TimeoutSecs = 120
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# These five markers are emitted by Unity's garbage collector at the end of
# the asset load sequence, which only completes after all mods have loaded.
$markers = @(
    'Total:',
    'FindLiveObjects:',
    'CreateObjectMapping:',
    'MarkObjects:',
    'DeleteObjects:'
)

$deadline = (Get-Date).AddSeconds($TimeoutSecs)
$elapsed  = 0

Write-Host "Waiting for RimWorld to finish loading (timeout: ${TimeoutSecs}s)..."

while ((Get-Date) -lt $deadline) {
    if (Test-Path $LogFile) {
        $content = Get-Content $LogFile -Raw -ErrorAction SilentlyContinue
        if ($content) {
            $allFound = $true
            foreach ($marker in $markers) {
                if ($content -notlike "*$marker*") {
                    $allFound = $false
                    break
                }
            }
            if ($allFound) {
                Write-Host "Load complete (${elapsed}s elapsed)."
                exit 0
            }
        }
    }

    Start-Sleep -Seconds 2
    $elapsed += 2
}

Write-Error "Timed out after ${TimeoutSecs}s - RimWorld did not finish loading."
exit 1
