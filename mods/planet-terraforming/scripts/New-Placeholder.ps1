<#
.SYNOPSIS
    Generates a solid-color placeholder PNG with a border and centered label.

.DESCRIPTION
    Uses System.Drawing to render a placeholder image: a solid fill, a 2px
    darker border, and a centered label whose font size is auto-shrunk to fit
    within the image bounds. Parent directories are created as needed.

    This is the standard way to produce placeholder art for the Terraformation
    mod. There is no ImageMagick available on this machine; a Python + miniconda
    fallback exists, but prefer this PowerShell helper for consistency.

.PARAMETER Path
    Output PNG path. Parent directories are created if missing.

.PARAMETER Width
    Image width in pixels.

.PARAMETER Height
    Image height in pixels.

.PARAMETER Label
    Text drawn centered on the image. Pass an empty string for no label.

.PARAMETER Color
    Optional fill color. Accepts any name understood by [System.Drawing.Color]
    (e.g. "Firebrick", "SteelBlue") or a hex string like "#8B3A2F".
    Defaults to a Mars-like rust red.

.EXAMPLE
    .\New-Placeholder.ps1 -Path ..\About\Preview.png -Width 640 -Height 360 -Label "Terraformation"

.EXAMPLE
    .\New-Placeholder.ps1 -Path ..\About\ModIcon.png -Width 64 -Height 64 -Label "T" -Color "#8B3A2F"
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $Path,

    [Parameter(Mandatory = $true)]
    [int] $Width,

    [Parameter(Mandatory = $true)]
    [int] $Height,

    [Parameter(Mandatory = $true)]
    [AllowEmptyString()]
    [string] $Label,

    [Parameter(Mandatory = $false)]
    [string] $Color = "#8B3A2F"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

# Resolve the fill color from either a hex string (#RRGGBB) or a named color.
function Resolve-FillColor([string] $value) {
    if ($value -match '^#?[0-9A-Fa-f]{6}$') {
        $hex = $value.TrimStart('#')
        $r = [Convert]::ToInt32($hex.Substring(0, 2), 16)
        $g = [Convert]::ToInt32($hex.Substring(2, 2), 16)
        $b = [Convert]::ToInt32($hex.Substring(4, 2), 16)
        return [System.Drawing.Color]::FromArgb(255, $r, $g, $b)
    }
    return [System.Drawing.Color]::FromName($value)
}

# Darken a color by the given factor (0..1) for the border.
function Get-DarkerColor([System.Drawing.Color] $base, [double] $factor) {
    $r = [int]([Math]::Floor($base.R * $factor))
    $g = [int]([Math]::Floor($base.G * $factor))
    $b = [int]([Math]::Floor($base.B * $factor))
    return [System.Drawing.Color]::FromArgb(255, $r, $g, $b)
}

# Ensure the output directory exists.
$parent = Split-Path -Path $Path -Parent
if ($parent -and -not (Test-Path -LiteralPath $parent)) {
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
}

$fill = Resolve-FillColor $Color
$borderColor = Get-DarkerColor $fill 0.6

$bitmap = $null
$graphics = $null
try {
    $bitmap = New-Object System.Drawing.Bitmap($Width, $Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias

    # Solid fill.
    $fillBrush = New-Object System.Drawing.SolidBrush($fill)
    $graphics.FillRectangle($fillBrush, 0, 0, $Width, $Height)
    $fillBrush.Dispose()

    # 2px darker border drawn inside the bounds.
    $pen = New-Object System.Drawing.Pen($borderColor, 2)
    $graphics.DrawRectangle($pen, 1, 1, $Width - 2, $Height - 2)
    $pen.Dispose()

    # Centered label, auto-shrunk to fit within 90% of each dimension.
    if (-not [string]::IsNullOrEmpty($Label)) {
        $maxW = $Width * 0.9
        $maxH = $Height * 0.9
        $fontSize = [Math]::Max(6.0, [double]$Height * 0.5)
        $font = $null
        while ($fontSize -ge 6.0) {
            $candidate = New-Object System.Drawing.Font("Arial", [single]$fontSize, [System.Drawing.FontStyle]::Bold)
            $size = $graphics.MeasureString($Label, $candidate)
            if ($size.Width -le $maxW -and $size.Height -le $maxH) {
                $font = $candidate
                break
            }
            $candidate.Dispose()
            $fontSize -= 1.0
        }
        if ($null -eq $font) {
            $font = New-Object System.Drawing.Font("Arial", 6, [System.Drawing.FontStyle]::Bold)
        }

        $format = New-Object System.Drawing.StringFormat
        $format.Alignment = [System.Drawing.StringAlignment]::Center
        $format.LineAlignment = [System.Drawing.StringAlignment]::Center

        $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
        $rect = New-Object System.Drawing.RectangleF(0, 0, [single]$Width, [single]$Height)
        $graphics.DrawString($Label, $font, $textBrush, $rect, $format)

        $textBrush.Dispose()
        $format.Dispose()
        $font.Dispose()
    }

    $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Host "Wrote placeholder: $Path ($Width x $Height)"
}
finally {
    if ($null -ne $graphics) { $graphics.Dispose() }
    if ($null -ne $bitmap) { $bitmap.Dispose() }
}
