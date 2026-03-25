---
description: Build, deploy, and boot-test a mod against a live RimWorld instance. Captures the startup log and restores all state on exit.
allowed-tools: Bash, Read, Write
context: fork
---

# /test-mod

Boot RimWorld with only the target mod active, capture the startup log, then restore everything
to its original state. Use this to verify a mod loads without errors.

**Usage:** `/test-mod <mod-name>`
**Example:** `/test-mod fishing-is-fun`

**Platform:** Windows 11. All shell commands are run via Git Bash; PowerShell helper scripts
live in `.claude/skills/test-mod/scripts/` and are invoked with `powershell -ExecutionPolicy Bypass -File`.

The mod name must match a subdirectory under `mods/`.

---

## 0 — Read configuration

Read `.env` from the repo root. If `.env` does not exist, stop and tell the user to copy
`.env.example` to `.env` and fill in the values.

Extract these two keys:

| Key | Description |
|-----|-------------|
| `RIMWORLD_DIR` | Base path to the RimWorld install (e.g. `E:\SteamLibrary\steamapps\common\RimWorld`) |
| `RIMWORLD_LOG_FILE` | Full path to `Player.log` in AppData |

Derive the rest — no conversion needed, PowerShell handles Windows paths natively:
- `RIMWORLD_EXE` = `$RIMWORLD_DIR\RimWorldWin64.exe`
- `RIMWORLD_MODS_DIR` = `$RIMWORLD_DIR\Mods`
- `RIMWORLD_MODS_CONFIG` = `$RIMWORLD_DIR\ModsConfig.xml`

If any key is missing or a derived path does not exist, stop with a descriptive error.

---

## 1 — Validate mod

Confirm that the directory `mods/$ARGUMENTS` exists. If not, stop with:
> Error: mod '$ARGUMENTS' not found — expected directory mods/$ARGUMENTS

---

## 2 — Build

Run from the repo root:
```bash
mage build $ARGUMENTS
```

Stop if the build fails.

---

## 3 — Read mod metadata

Read `mods/$ARGUMENTS/About/About.xml`. Extract:
- The top-level `<packageId>` (the mod's own ID)
- All `<packageId>` values inside `<modDependencies>` (skip commented-out blocks)

These will populate the minimal ModsConfig.xml written in Step 6.

---

## 4 — Deploy mod to RimWorld

Call the deploy script:
```bash
powershell -ExecutionPolicy Bypass -File ".claude/skills/test-mod/scripts/deploy.ps1" \
  -ModDir "mods/$ARGUMENTS" \
  -TargetDir "$RIMWORLD_MODS_DIR\\$ARGUMENTS"
```

Stop if the script exits non-zero.

---

## 5 — Back up ModsConfig.xml

```bash
cp "$RIMWORLD_MODS_CONFIG" "$RIMWORLD_MODS_CONFIG.bak"
```

---

## 6 — Write minimal ModsConfig.xml

Using the Write tool, overwrite `$RIMWORLD_MODS_CONFIG` with a minimal config.

The `<activeMods>` list must be:
1. `ludeon.rimworld` — always first
2. One `<li>` per dependency package ID from Step 3, in order
3. The mod's own package ID — always last

Read the existing `<version>` value from the backup and preserve it.

Example for `fishing-is-fun` (depends on `Ludeon.RimWorld.Odyssey`):
```xml
<?xml version="1.0" encoding="utf-8"?>
<ModsConfigData>
  <version>1.6.4566 rev607</version>
  <activeMods>
    <li>ludeon.rimworld</li>
    <li>Ludeon.RimWorld.Odyssey</li>
    <li>jalapenolabs.rimworld.fishingisfun</li>
  </activeMods>
  <knownExpansions />
</ModsConfigData>
```

---

## 7 — Launch RimWorld

Delete any existing log for a clean capture:
```bash
rm -f "$RIMWORLD_LOG_FILE"
```

Start RimWorld and capture its PID via the launch script:
```bash
RIMWORLD_PID=$(powershell -ExecutionPolicy Bypass -File ".claude/skills/test-mod/scripts/launch.ps1" \
  -ExePath "$RIMWORLD_EXE")
echo "RimWorld started (PID $RIMWORLD_PID)"
```

---

## 8 — Poll for startup completion

Call the poll script. It exits 0 when all GC markers are present, 1 on timeout:
```bash
powershell -ExecutionPolicy Bypass -File ".claude/skills/test-mod/scripts/poll-log.ps1" \
  -LogFile "$RIMWORLD_LOG_FILE" \
  -TimeoutSecs 120
POLL_EXIT=$?
```

If `POLL_EXIT` is non-zero, proceed to Step 9 to shut down, then report the timeout before Step 10.

---

## 9 — Shut down RimWorld

```bash
powershell -ExecutionPolicy Bypass -Command "Stop-Process -Id $RIMWORLD_PID -Force -ErrorAction SilentlyContinue"
sleep 1
```

---

## 10 — Display the log

Read `$RIMWORLD_LOG_FILE`. Extract the section between `UnloadTime:` and `Memory Statistics:` —
this is the window that contains mod initialization output and any errors.

Filter out noise (skip silently):
- Lines starting with `Unloading` that also contain `unused`

Format XML errors for readability: lines starting with `XML error` should have their XML
fragment pretty-printed with indentation.

If no errors or warnings appear, say so clearly.

Write an ANSI-stripped copy to `dev.log` at the repo root for later review.

---

## 11 — Restore and clean up

Always run this step, even if an earlier step failed.

```bash
# Restore the original ModsConfig
mv "$RIMWORLD_MODS_CONFIG.bak" "$RIMWORLD_MODS_CONFIG"

# Remove the deployed mod
powershell -ExecutionPolicy Bypass -Command "Remove-Item -Path '$RIMWORLD_MODS_DIR\\$ARGUMENTS' -Recurse -Force -ErrorAction SilentlyContinue"
```

Report what was restored so the user knows state is clean.
