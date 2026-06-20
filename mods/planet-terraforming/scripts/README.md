# Scripts

Helper scripts for the Terraformation mod.

## New-Placeholder.ps1

Generates a placeholder PNG: a solid color fill with a 2px darker border and a
centered, auto-shrinking label. This is the standard tool for producing
placeholder art (previews, mod icon, building/item textures) until final art is
made. There is no ImageMagick on this machine. A Python + miniconda fallback
exists, but prefer this PowerShell helper for consistency.

### Usage

Run with Windows PowerShell:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/New-Placeholder.ps1 `
    -Path About/Preview.png -Width 640 -Height 360 -Label "Terraformation"
```

### Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `-Path`   | yes      | Output PNG path. Parent directories are created automatically. |
| `-Width`  | yes      | Image width in pixels. |
| `-Height` | yes      | Image height in pixels. |
| `-Label`  | yes      | Centered label text. Pass `""` for no label. Font auto-shrinks to fit. |
| `-Color`  | no       | Fill color: a named color (e.g. `Firebrick`) or hex `#RRGGBB`. Defaults to a Mars rust red `#8B3A2F`. |

### Examples

```powershell
# Mod preview image
powershell -ExecutionPolicy Bypass -File scripts/New-Placeholder.ps1 `
    -Path About/Preview.png -Width 640 -Height 360 -Label "Terraformation"

# Mod icon
powershell -ExecutionPolicy Bypass -File scripts/New-Placeholder.ps1 `
    -Path About/ModIcon.png -Width 64 -Height 64 -Label "T"

# A building texture in a custom color
powershell -ExecutionPolicy Bypass -File scripts/New-Placeholder.ps1 `
    -Path Textures/Things/Building/Oxygenator.png -Width 128 -Height 128 -Label "O2" -Color "SteelBlue"
```
