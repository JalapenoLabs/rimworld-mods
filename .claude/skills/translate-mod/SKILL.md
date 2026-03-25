---
description: Translate all Defs XML files in a mod to every RimWorld-supported language, writing output to the mod's Languages/ directory.
allowed-tools: Glob, Read, Write, Bash
context: fork
---

# /translate-mod

Translate all user-facing strings in a mod's Defs XML files into all 29 languages
supported by RimWorld. Output files are written into the mod's `Languages/` directory
following RimWorld's DefInjected localization structure.

**Usage:** `/translate-mod <mod-name>`
**Example:** `/translate-mod fishing-is-fun`

The mod name must match a subdirectory under `mods/`.

---

## Supported languages

These are the exact folder names RimWorld expects. Use them verbatim in output paths:

```
Catalan  ChineseSimplified  ChineseTraditional  Czech      Danish
Dutch    English            Estonian            Finnish    French
German   Greek              Hungarian           Italian    Japanese
Korean   Norwegian          Polish              Portuguese PortugueseBrazilian
Romanian Russian            Slovak              Spanish    SpanishLatin
Swedish  Turkish            Ukrainian           Vietnamese
```

---

## Step 1 — Discover source files

From `mods/$ARGUMENTS/`, find all files matching these criteria:

**Include:**
- All `*.xml` files under any `Defs/` directory (at any depth)
- All `*.xml` files under `Languages/English/Keyed/` (at any depth)

**Exclude:**
- `About.xml` (anywhere)
- Any other file under a `Languages/` directory (don't re-translate existing translations)

If no source files are found, stop and report it clearly.

---

## Step 2 — Translate each file

For each source file discovered:

1. Read the file contents.
2. Produce a translation for every supported language (all 29).
   - **Exception:** For files from `Languages/English/Keyed/`, skip `English` — there is no point translating English into English.
3. Write each translated file to disk (Step 3).

Process one source file at a time. For each file, produce all language outputs before moving to the next.

### What to translate

Translate only the user-facing text fields — things a player reads in-game:
`<label>`, `<description>`, `<jobString>`, `<reportString>`, `<verb>`, `<gerund>`,
`<deathMessage>`, `<letterLabel>`, `<letterText>`, and similar human-readable fields.

Do **not** translate:
- `<defName>` or any structural/identifier tags
- XML attributes
- Numeric values, boolean values, or enum values

Do **not** reformat the XML or strip comments — translate strings only.

### Output format

RimWorld's DefInjected format uses `<LanguageData>` with dotted paths to reference the
original def. The `defName` becomes the root prefix:

```xml
<?xml version="1.0" encoding="utf-8"?>
<LanguageData>

  <DefName.fieldName>Translated text here</DefName.fieldName>
  <DefName.stages.0.label>Translated stage label</DefName.stages.0.label>

</LanguageData>
```

For Keyed files, the output format mirrors the input key structure but with translated values:
```xml
<?xml version="1.0" encoding="utf-8"?>
<LanguageData>

  <KeyName>Translated text here</KeyName>

</LanguageData>
```

### Output path rules

The output path is derived from the input path by inserting the language into the hierarchy.
All paths are relative to `mods/$ARGUMENTS/`.

**Defs files:**
```
In:  1.6/Defs/ThoughtDef/PleasantFishingTrip.xml
Out: 1.6/Languages/French/DefInjected/ThoughtDef/PleasantFishingTrip.xml

In:  Defs/ResearchDef/ResearchProjectDef.xml
Out: Languages/French/DefInjected/ResearchDef/ResearchProjectDef.xml

In:  1.5/Defs/BuildingDef/SkyScrapers/SkyRise.xml
Out: 1.5/Languages/French/DefInjected/BuildingDef/SkyScrapers/SkyRise.xml

In:  1.6/Defs/ElectricityMeter.xml
Out: 1.6/Languages/French/DefInjected/ElectricityMeter.xml
```

Pattern: `[<Version>/]Languages/<Language>/DefInjected/[<SubPath>/]<Filename>.xml`
- Preserve the version prefix (`1.6/`, `1.5/`, etc.) if present
- Mirror the subdirectory structure that was under `Defs/`

**Keyed files:**
```
In:  1.6/Languages/English/Keyed/Strings.xml
Out: 1.6/Languages/French/Keyed/Strings.xml
```

Pattern: Replace `English/Keyed/` with `<Language>/Keyed/`

### Example translation

Input (`1.6/Defs/ThoughtDef/PleasantFishingTrip.xml`):
```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <ThoughtDef>
    <defName>PleasantFishingTrip</defName>
    <durationDays>0.25</durationDays>
    <stackLimit>1</stackLimit>
    <label>pleasant fishing trip</label>
    <stages>
      <li>
        <label>Went fishing</label>
        <description>It was nice to enjoy some peace while fishing for a bit.</description>
        <baseMoodEffect>3</baseMoodEffect>
      </li>
    </stages>
  </ThoughtDef>
</Defs>
```

Catalan output (`1.6/Languages/Catalan/DefInjected/ThoughtDef/PleasantFishingTrip.xml`):
```xml
<?xml version="1.0" encoding="utf-8"?>
<LanguageData>

  <PleasantFishingTrip.label>Agradable jornada de pesca</PleasantFishingTrip.label>
  <PleasantFishingTrip.stages.0.label>Va anar a pescar</PleasantFishingTrip.stages.0.label>
  <PleasantFishingTrip.stages.0.description>Va ser agradable gaudir d'una mica de pau mentre pescava durant una estona.</PleasantFishingTrip.stages.0.description>

</LanguageData>
```

Reference: https://rimworldwiki.com/wiki/Modding_Tutorials/Localization

---

## Step 3 — Write output files

For each translated file:
- Full output path = `mods/$ARGUMENTS/{output path from Step 2}`
- Create intermediate directories as needed before writing
- Write UTF-8, ensure the file ends with a newline

Do not overwrite a file with empty or clearly malformed content — skip and report it instead.

---

## Step 4 — Report

When all files are processed, print a summary:
- Total source files found
- Total output files written (source files × languages)
- Any skipped or failed files with a reason
