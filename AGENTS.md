# AGENTS.md — Architecture and Operating Model

This file is for AI agents working inside this repository. It describes the structure,
conventions, and constraints you must understand before making changes.

---

## What this repository is

A monorepo that centralises build infrastructure, CI/CD, and AI-assisted developer workflows
for multiple RimWorld mods maintained by JalapenoLabs. Each mod lives in its own public GitHub
repo and is included here as a git submodule under `mods/`. The root provides the shared
tooling that individual mod repos do not need to carry themselves.

---

## Repository structure

```
rimworld-mods/
├── mods/<mod-name>/           # git submodule for each mod
├── RimworldForCICD/           # git submodule — managed DLLs for building
├── binaries/
│   └── 0Harmony.dll           # committed third-party library
├── .claude/skills/            # Claude Code slash commands
├── .github/workflows/         # GitHub Actions
├── legacy/                    # archived scripts (do not rely on)
├── magefile.go                # root build tasks
└── go.mod                     # Go module (github.com/JalapenoLabs/rimworld-mods)
```

---

## Submodule model

Every mod is a standalone public git repository added as a submodule. The submodule pointer
in the monorepo records a specific commit — updates require a commit to the monorepo after
pushing to the mod repo.

**Current submodules** (see `.gitmodules`):
- `mods/electricity-meter` → `github.com:JalapenoLabs/rimworld-electricity-meter`
- `mods/fishing-is-fun` → `github.com:JalapenoLabs/rimworld-fishing-is-fun`
- `mods/project-zero-dawn` → `github.com:JalapenoLabs/rimworld-project-zero-dawn`
- `RimworldForCICD` → `github.com:JalapenoLabs/RimworldForCICD`

All submodule URLs use SSH (`git@github.com:`). The CI secret `SSH_PRIVATE_KEY` is required
for checkout.

### RimworldForCICD

This submodule contains the managed `.dll` files that RimWorld ships with — the assemblies
that mod C# code compiles against (`Assembly-CSharp.dll`, `UnityEngine.dll`, etc.). They are
not committed to this repo directly; they are synced to the `RimworldForCICD` repo by a
Docker daemon that runs on the developer's local machine and monitors the RimWorld install.

All `mod.csproj` files reference these DLLs at `../../RimworldForCICD/Managed/*.dll` — that
path resolves to the repo root's `RimworldForCICD/` from `mods/<mod>/`.

---

## Mod layout

Each mod submodule follows this structure:

```
mods/<mod-name>/
├── About/
│   ├── About.xml              # packageId, dependencies, supportedVersions
│   └── Preview.png
├── 1.6/                       # versioned folder — all runtime content lives here
│   ├── Assemblies/            # compiled DLL output destination
│   ├── Source/                # C# source files
│   ├── Defs/                  # XML game definitions
│   └── Languages/             # generated translations (DefInjected + Keyed)
├── Textures/                  # sprites and UI assets (outside versioned folder)
├── mod.csproj                 # .NET project file at mod root
└── README.md
```

The version folder (`1.6/`) is the RimWorld version target. Future versions (`1.7/` etc.)
would add parallel folders.

---

## Build system

### Tool chain

- **Language:** C# targeting `net480` (Unity/.NET Framework 4.8 — what RimWorld uses)
- **Build runner:** [Mage](https://magefile.org/) via `magefile.go` at the repo root
- **Compiler:** `dotnet build` (invoked by Mage)

### Mage targets

| Target | Command | What it does |
|--------|---------|--------------|
| `Build` | `mage build <mod>` | Cleans then compiles a single mod |
| `BuildAll` | `mage buildAll` | Builds every mod with a `mod.csproj` under `mods/` |
| `Clean` | `mage clean <mod>` | Removes build-output files from `mods/<mod>/1.6/Assemblies/` |
| `CleanAll` | `mage cleanAll` | Cleans all mods |

Mage discovers buildable mods by scanning for `mods/*/mod.csproj`. A submodule without
`mod.csproj` at its root will not be picked up.

### mod.csproj requirements

`mod.csproj` must live at the **mod root** (not in a subdirectory). Every csproj must include:

```xml
<EnableDefaultCompileItems>false</EnableDefaultCompileItems>
```

This is mandatory. Without it the .NET SDK auto-discovers `.cs` files and produces
`NETSDK1022: Duplicate Compile items` errors when explicit `<Compile Include>` items are
also present.

Key paths inside a `mod.csproj`:
- `<OutputPath>1.6/Assemblies</OutputPath>` — relative to the mod root
- `<Compile Include="1.6/Source/*" />` — relative to the mod root
- `<Reference Include="../../RimworldForCICD/Managed/*.dll">` — two levels up from `mods/<mod>/`

### The `0`-prefix convention

Files in `Assemblies/` whose names start with `0` (e.g. `0Harmony.dll`) are **third-party
bundled libraries**, not build outputs. Mage's `Clean` target skips them explicitly. Never
delete or overwrite these files during a clean.

If a mod needs Harmony, it either references `1.6/Assemblies/0Harmony.dll` (committed to the
mod's own repo) or `../../binaries/0Harmony.dll` (the shared copy at the monorepo root).

---

## GitHub Actions workflows

### `callable-build-mod.yml`
Reusable (`workflow_call`) workflow. Accepts `mod` and `version` inputs. Runs:
1. Checkout with submodules
2. Setup .NET 9 and Go
3. `mage build <mod>` via `magefile/mage-action@v4`
4. Install `xmllint` and validate all `.xml` files under `<version>/` and `About/`
5. Print a coloured pass/fail summary; exit non-zero if any step failed

All steps use `continue-on-error: true` with `if:` dependency conditions so every check
always runs and is reported.

### `main.yml`
Triggers on push to `main`, pull requests, and `workflow_dispatch`. Calls
`callable-build-mod.yml` in a matrix — one entry per mod with its target version. Adding
a new mod requires adding a `{ mod: <name>, version: '1.6' }` entry to the `include:` list.

### `release.yml`
`workflow_dispatch` only. Inputs: `mod`, `version`. Two jobs:
1. **build** — calls `callable-build-mod.yml`; must pass before packaging proceeds
2. **package** — assembles a `release/` folder with only distributable files (`About/`,
   version folder, `Textures/`, `README.md`), zips it, uploads as a workflow artifact

Only files explicitly copied in the "Assemble release folder" step end up in the zip.

### `test-mod.yml`
`workflow_dispatch` only. Input: `mod`. Runs on a **self-hosted Windows 11 runner** with
RimWorld installed. Steps:
1. Checkout with submodules
2. Validate mod directory exists
3. Write `.env` from job-level env vars (`RIMWORLD_DIR`, `RIMWORLD_LOCAL_LOW`)
4. Install Claude Code CLI
5. Run `/test-mod <mod>` non-interactively via `claude -p --dangerously-skip-permissions`

The runner must be running as an interactive desktop session (not a Windows service) so
RimWorld can open a window.

---

## Local environment

The `.env` file at the repo root (gitignored) configures local developer tooling:

```dotenv
RIMWORLD_DIR=E:\SteamLibrary\steamapps\common\RimWorld
RIMWORLD_LOCAL_LOW=C:\Users\<name>\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios
```

Derived paths (used by skills and CI):
- `RIMWORLD_EXE` → `$RIMWORLD_DIR\RimWorldWin64.exe`
- `RIMWORLD_MODS_DIR` → `$RIMWORLD_DIR\Mods`
- `RIMWORLD_MODS_CONFIG` → `$RIMWORLD_LOCAL_LOW\Config\ModsConfig.xml`
- `RIMWORLD_LOG_FILE` → `$RIMWORLD_LOCAL_LOW\Player.log`

`.env.example` is the committed template. Do not commit `.env`.

---

## Claude Code skills

Skills live in `.claude/skills/` and are invoked as slash commands inside Claude Code.
All skills use `context: fork` — they run as isolated subagents and do not affect the
main conversation context.

### `/test-mod <mod-name>`

**Purpose:** Verify a mod loads cleanly in a live RimWorld instance.

**Steps (in order):**
1. Read `.env` and derive all paths
2. Validate the mod directory exists
3. `mage build <mod>` — fresh build
4. Read `About/About.xml` to extract `packageId` and `modDependencies`
5. Deploy mod files to `RIMWORLD_MODS_DIR/<mod>` (via `scripts/deploy.ps1`)
6. Backup `ModsConfig.xml`
7. Write a minimal `ModsConfig.xml` containing only the base game, mod dependencies, and
   the mod itself (in that order)
8. Delete `Player.log`, launch RimWorld (`scripts/launch.ps1`), capture PID
9. Poll `Player.log` for five Unity GC markers indicating load completion (`scripts/poll-log.ps1`)
10. Kill RimWorld
11. Extract and display the log window between `UnloadTime:` and `Memory Statistics:`
12. Restore `ModsConfig.xml` and remove deployed mod — **always**, even on failure

Helper scripts in `.claude/skills/test-mod/scripts/` are PowerShell (`.ps1`) invoked via
`powershell -ExecutionPolicy Bypass -File`. The skill targets Windows 11.

### `/translate-mod <mod-name>`

**Purpose:** Generate or update RimWorld localization files for all 29 supported languages.

**How it works:**
1. Discover source files: all `*.xml` under `Defs/` directories + `Languages/English/Keyed/`
   (excluding `About.xml` and all other `Languages/` content)
2. For each source file × each language:
   - Determine the output path (`Languages/<Language>/DefInjected/...`)
   - Check whether the output file already exists
   - **Existing and complete** → skip entirely (preserves hand-edited translations)
   - **Existing with missing keys** → translate only the absent keys, insert before
     `</LanguageData>`, leave all existing content untouched
   - **Does not exist** → translate all keys, write new file
3. Report: files written, files updated, files skipped

The 29 language folder names are exact RimWorld identifiers (e.g. `ChineseSimplified`,
`PortugueseBrazilian`, `SpanishLatin`). Do not alter these.

---

## Key conventions and constraints

- **Submodule SSH URLs** — all submodule entries in `.gitmodules` use `git@github.com:` not
  `https://`. The CI `SSH_PRIVATE_KEY` secret is required for recursive checkout.
- **Submodule pointer commits** — after pushing changes to a mod repo, you must also commit
  the updated submodule pointer in the monorepo.
- **mod.csproj at mod root** — never place it in a subdirectory. Mage and the CI both expect
  it at `mods/<mod>/mod.csproj`.
- **`EnableDefaultCompileItems=false`** — required in every `mod.csproj`. Forgetting this
  causes `NETSDK1022` build failures.
- **RimworldForCICD path** — from `mods/<mod>/`, the managed DLLs are at
  `../../RimworldForCICD/Managed/`. One `..` only reaches `mods/`.
- **`0`-prefix files are not build outputs** — never delete files starting with `0` during
  clean operations.
- **Release folder content** — only `About/`, the version folder, `Textures/`, and
  `README.md` are distributed. Never include source code, `.csproj`, `.github/`, or CI
  configs in a release.
- **`dev.log`** — local test output written by `/test-mod`. Gitignored. Never commit it.
- **`legacy/`** — archived, do not depend on or modify these files.

---

## Adding a new mod

1. Add the submodule:
   ```shell
   git submodule add git@github.com:JalapenoLabs/rimworld-<mod>.git mods/<mod>
   ```
2. Ensure the mod has `mod.csproj` at its root with `EnableDefaultCompileItems=false` and
   correct paths for `OutputPath`, `Compile Include`, and `Reference` to
   `../../RimworldForCICD/Managed/`.
3. Add `{ mod: <name>, version: '1.6' }` to the `include:` matrix in
   `.github/workflows/main.yml`.
4. Commit `.gitmodules` and the new submodule pointer, push.
