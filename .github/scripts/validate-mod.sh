#!/usr/bin/env bash
# validate-mod.sh — Validate that a mod directory meets project standards.
#
# Called from callable-build-mod.yml after checkout.
# Usage: bash .github/scripts/validate-mod.sh <mod-name>
#
# Checks:
#   Required files  — mod.csproj, README.md, Changelog.txt, LICENSE,
#                     About/About.xml, About/Manifest.xml, About/ModIcon.png,
#                     About/preview.png (case-insensitive)
#   Consistency     — .editorconfig matches root, .gitignore matches template
#
# Exit codes: 0 = all checks passed, 1 = one or more checks failed.

set -uo pipefail

MOD="${1:?Usage: validate-mod.sh <mod-name>}"
MOD_DIR="mods/$MOD"

GREEN='\033[0;32m'
RED='\033[0;31m'
BOLD='\033[1m'
NC='\033[0m'

FAILURES=0

pass() { echo -e "  ${GREEN}✔${NC}  $1"; }
fail() { echo -e "  ${RED}✘${NC}  $1"; FAILURES=$((FAILURES + 1)); }

# Check a file exists at the given path relative to MOD_DIR.
require_file() {
  local rel="$1"
  local label="${2:-$rel}"
  if [ -f "$MOD_DIR/$rel" ]; then
    pass "$label"
  else
    fail "$label  →  missing: $MOD_DIR/$rel"
  fi
}

# Check a file exists case-insensitively inside a subdirectory of MOD_DIR.
# Useful for assets that may be Preview.png or preview.png depending on the mod.
require_file_icase() {
  local subdir="$1"
  local filename="$2"
  local label="${3:-$subdir/$filename}"
  if find "$MOD_DIR/$subdir" -maxdepth 1 -iname "$filename" -print -quit 2>/dev/null | grep -q .; then
    pass "$label"
  else
    fail "$label  →  not found in $MOD_DIR/$subdir/ (case-insensitive search)"
  fi
}

# Check a file exists AND matches a reference file exactly.
# Prints a unified diff on mismatch to make the problem obvious in CI logs.
require_matches() {
  local rel="$1"
  local reference="$2"
  local label="${3:-$rel matches $reference}"
  local path="$MOD_DIR/$rel"

  if [ ! -f "$path" ]; then
    fail "$label  →  $path not found"
    return
  fi

  if diff -q "$path" "$reference" > /dev/null 2>&1; then
    pass "$label"
  else
    fail "$label  →  differs from $reference"
    echo ""
    # Show the diff with reference on the left so it reads as "what to change"
    diff --unified=3 "$reference" "$path" | head -40 || true
    echo ""
  fi
}

# ─────────────────────────────────────────────────────────────────────────────

echo ""
echo -e "${BOLD}Validating: $MOD${NC}"

if [ ! -d "$MOD_DIR" ]; then
  echo -e "${RED}  ✘  Mod directory not found: $MOD_DIR${NC}"
  exit 1
fi

echo ""
echo "── Required files ──────────────────────────────────────────────────────────"
require_file      "mod.csproj"
require_file      "README.md"
require_file      "Changelog.txt"
require_file      "LICENSE"
require_file      "About/About.xml"
require_file      "About/Manifest.xml"
require_file      "About/ModIcon.png"
require_file_icase "About" "preview.png" "About/preview.png"

echo ""
echo "── Consistency checks ──────────────────────────────────────────────────────"
require_matches ".editorconfig" ".editorconfig"                    ".editorconfig matches root"
require_matches ".gitignore"    ".github/templates/mod.gitignore"  ".gitignore matches template"

echo ""
if [ "$FAILURES" -eq 0 ]; then
  echo -e "${GREEN}All checks passed.${NC}"
  exit 0
else
  echo -e "${RED}$FAILURES check(s) failed.${NC}"
  exit 1
fi
