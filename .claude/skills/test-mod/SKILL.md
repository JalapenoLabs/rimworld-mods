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

The mod name must match a subdirectory under `mods/`.

---

## 0 — Read configuration

Read `.env` from the repo root. If `.env` does not exist, stop and tell the user to copy
`.env.example` to `.env` and fill in the values.

Extract these two keys:

| Key | Description |
|-----|-------------|
| `RIMWORLD_DIR` | Base path to the RimWorld install directory |
| `RIMWORLD_LOG_FILE` | Full path to `Player.log` in AppData |

Derive the rest:
- `RIMWORLD_EXE_PATH` = `$RIMWORLD_DIR/RimWorldWin64.exe`
- `RIMWORLD_MODS_DIR` = `$RIMWORLD_DIR/Mods`
- `RIMWORLD_MODS_CONFIG` = `$RIMWORLD_DIR/ModsConfig.xml`

If any key is missing or a derived path does not exist on disk, stop with a descriptive error.

On Windows (Git Bash), convert backslash paths to forward slashes for shell use.

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
- The `<packageId>` at the top level (the mod's own package ID)
- All `<packageId>` values nested inside `<modDependencies>` (direct deps; skip commented-out blocks)

These will be used to construct a minimal ModsConfig.xml.

---

## 4 — Deploy mod to RimWorld

Copy the built mod into RimWorld's Mods directory. Use the mod directory name as the folder name.

```bash
MOD_NAME="$ARGUMENTS"
TARGET="$RIMWORLD_MODS_DIR/$MOD_NAME"

rm -rf "$TARGET"
mkdir -p "$TARGET"

# Always copy About/
cp -r "mods/$MOD_NAME/About" "$TARGET/"

# Copy versioned folders (e.g. 1.6/, 1.7/) — find all top-level dirs matching N.N
for dir in mods/$MOD_NAME/[0-9]*.[0-9]*/; do
  [ -d "$dir" ] && cp -r "$dir" "$TARGET/"
done

# Optional assets
[ -d "mods/$MOD_NAME/Textures" ] && cp -r "mods/$MOD_NAME/Textures" "$TARGET/"
```

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
2. One `<li>` for each dependency package ID extracted in Step 3, in order
3. The mod's own package ID — always last

Leave the `<version>` field as is, whatever it was when you found it.

Example for `fishing-is-fun` (which depends on `Ludeon.RimWorld.Odyssey`):
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

Delete the existing log so we get a clean capture:
```bash
rm -f "$RIMWORLD_LOG_FILE"
```

Launch RimWorld as a detached background process and capture its PID:
```bash
"$RIMWORLD_EXE_PATH" &
RIMWORLD_PID=$!
echo "RimWorld started (PID $RIMWORLD_PID)"
```

---

## 8 — Poll for startup completion

Poll `$RIMWORLD_LOG_FILE` every 2s, up to **120 seconds**.

RimWorld has fully loaded all mods when all five of these strings appear anywhere in the log:
- `Total:`
- `FindLiveObjects:`
- `CreateObjectMapping:`
- `MarkObjects:`
- `DeleteObjects:`

These are emitted by Unity's garbage collector after the full mod load sequence completes.

```bash
DEADLINE=$((SECONDS + 120))
READY=0
while [ $SECONDS -lt $DEADLINE ]; do
  if [ -f "$RIMWORLD_LOG_FILE" ]; then
    content=$(cat "$RIMWORLD_LOG_FILE")
    if echo "$content" | grep -q "Total:"             &&
       echo "$content" | grep -q "FindLiveObjects:"   &&
       echo "$content" | grep -q "CreateObjectMapping:" &&
       echo "$content" | grep -q "MarkObjects:"       &&
       echo "$content" | grep -q "DeleteObjects:"; then
      READY=1
      break
    fi
  fi
  sleep 2
done
```

If `READY` is still 0 after the loop, proceed to Step 9 but report a timeout error before Step 10.

---

## 9 — Shut down RimWorld

On Windows, use `taskkill` for reliable termination:
```bash
taskkill //F //PID $RIMWORLD_PID 2>/dev/null || kill $RIMWORLD_PID 2>/dev/null || true
```

Wait briefly for the process to exit:
```bash
sleep 1
```

---

## 10 — Display the log

Read `$RIMWORLD_LOG_FILE`. Extract the section between `UnloadTime:` and `Memory Statistics:` —
this window contains mod initialization output and any errors.

Filter out pure noise lines (skip silently):
- Lines starting with `Unloading` that also contain `unused`

Format XML error lines for readability: lines starting with `XML error` should have the XML
fragment pretty-printed with indentation.

If no errors or warnings are found, say so clearly.

Also write an ANSI-stripped copy to `dev.log` at the repo root for user review.

---

## 11 — Restore and clean up

Always run this step, even if an earlier step failed.

```bash
# Restore the original ModsConfig
mv "$RIMWORLD_MODS_CONFIG.bak" "$RIMWORLD_MODS_CONFIG"

# Remove the deployed mod
rm -rf "$RIMWORLD_MODS_DIR/$ARGUMENTS"
```

Report what was restored so the user knows state is clean.
