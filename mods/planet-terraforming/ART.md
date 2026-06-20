# Terraformation - Art Asset List

> Complete list of art needed for the **Terraformation** mod, for handing to an artist. Grouped by
> type, with the exact rotation variations each asset needs. Design direction (PLAN.md S23):
> vanilla-consistent top-down RimWorld style with an Odyssey spacer/industrial flavor; terraforming
> machines have glowing cores that intensify by tier; Mars = red/rust terrain; the sky shifts
> red -> hazy -> blue across phases.

---

## Conventions (read first)

### Rotation rules
RimWorld buildings use one of two graphic classes:

- **`Graphic_Single`** - one file, no direction suffix. Used for non-rotatable buildings and all
  items/icons/terrain.
- **`Graphic_Multi`** - up to four files with suffixes **`_north`, `_east`, `_south`, `_west`**.
  - `_south` is the default "facing the camera" view.
  - **The engine auto-mirrors West from East**, so for a left/right-**symmetric** building you only
    need to draw **3 unique** views (`_north`, `_east`, `_south`) and skip `_west`.
  - Draw all **4** only when the building is left/right **asymmetric**.

In the tables below, the **Variations** column states exactly what is needed. "NESW (3 unique)"
means draw north/east/south and let west mirror east; "NESW (4)" means draw all four.

### Canvas / format
- PNG with transparency, top-down orientation.
- Baseline resolution **128 px per tile-cell**; e.g. a 1x1 building ~128x128, 2x2 ~256x256, 3x3
  ~384x384. Hero/large machines may go 256 px/cell for crispness.
- Items/icons ~128x128 on a roughly 64 px content area (RimWorld item convention).
- Keep a consistent light source (top-left) to match vanilla.

### What we do NOT need art for
- **Vacsuits / EVA gear** - reused from Odyssey (no new apparel art).
- **Sky colors** - configured as color values (`SkyColorSet`), not textures. The red->blue shift is
  data, not art.
- **Soil / rich soil / water terrain** - reuse vanilla terrain textures (only the Mars-specific and
  transitional terrains below are custom).
- **Meteor impacts / most effects** - reuse vanilla motes/effecters.

---

## 1. Terraforming machines (4 lanes x 4 tiers = 16)

Large emitter devices. Recommended **non-rotatable** (`Graphic_Single`, one file each); they read as
fixed installations and need no interaction cell. Tiers should be visually distinct (bigger, more
complex, brighter core as tier rises).

| Asset | Footprint | Rotatable | Variations | Notes |
|-------|-----------|-----------|------------|-------|
| Oxygen machine T1-T4 | 2x2 (T1) -> 3x3 (T4) | No | 1 each (4 total) | Greenhouse/algae -> vegetube -> spreader look; green-cyan glow |
| Heat machine T1-T4 | 2x2 -> 3x3 | No | 1 each (4 total) | Heater/furnace look; orange-red glow |
| Pressure machine T1-T4 | 2x2 -> 3x3 | No | 1 each (4 total) | Atmosphere drill/gas releaser; blue-white vents |
| Biomass machine T1-T4 | 2x2 -> 3x3 | No | 1 each (4 total) | Algae tank -> biodome -> tree spreader; green glow |

**Optional (recommended):** a separate **glow/"powered-on" overlay** per machine (16 extra files)
layered when running, so the cores light up only while powered. Can be a simple additive layer.

**Section total: 16 base files** (+16 optional glow overlays).

---

## 2. Support and production buildings

| Asset | Footprint | Rotatable | Variations | Notes |
|-------|-----------|-----------|------------|-------|
| Oxygenator | 1x1 or 2x2 | No | 1 | Room air unit; subtle vent glow |
| Basic oxygen craft station | 2x1 | Yes | NESW (3 unique) | Has an interaction/work side |
| Terraforming Analyzer (research bench) | 2x1 | Yes | NESW (3 unique) | Work side; screen/terminal |
| Advanced research facility (linked) | 1x1 | No | 1 | Linked-facility prop |
| Food grower T1 | 1x1 | No | 1 | Small planter tray; cheap look |
| Food grower T2 | 1x2 | No | 1 | Bigger tray/rack |
| Deep-drill auto-miner T1 | 1x1 | Yes | NESW (3 unique) | Output/interaction side |
| Deep-drill auto-miner T2 | 1x1 | Yes | NESW (3 unique) | Palette/detail upgrade of T1 |
| Deep-drill auto-miner T3 | 1x1 | Yes | NESW (3 unique) | Palette/detail upgrade of T2 |
| Satellite Launch Pad | 3x3 | No | 1 | Rocket/launch platform; flat pad |

**Optional:** a launch-pad "occupied/loaded" overlay (1).

**Section total: ~22 files** (drill tiers can share a base with palette swaps to reduce work).

---

## 3. Mineable deposits (in-map rocks)

Natural rock/crystal clusters the player mines. **No rotation** (`Graphic_Single`). Recommend
**2-3 shape variants each** for a natural scattered look (RimWorld picks among variants randomly).

| Asset | Rotatable | Variations | Notes |
|-------|-----------|------------|-------|
| Iridium ore deposit | No | 1 (rec. 2-3 variants) | Embedded ore in red rock |
| Zeolite ore deposit | No | 1 (rec. 2-3 variants) | Pale/porous mineral |
| Super Alloy ore deposit | No | 1 (rec. 2-3 variants) | Dark metallic vein |
| Ice deposit | No | 1 (rec. 2-3 variants) | Phase-1 only; pale blue |
| Vanilla-material crystals | No | 1 per material (rec. 2-3 variants) | One color per yielded material: steel, plasteel, components, gold/silver, uranium (~5). Crystals do NOT need NESW |

**Section total: ~9 base files** (more if you draw the recommended 2-3 variants each).

---

## 4. Items / resources (icons)

Stack-count item icons (`Graphic_Single` / `Graphic_StackCount`). **No rotation.**

| Asset | Variations | Notes |
|-------|------------|-------|
| Iridium (item) | 1 | Matches its deposit |
| Zeolite (item) | 1 | |
| Super Alloy (item) | 1 | |
| Ice (item) | 1 | |
| Oxygen canister | 1 | Oxygen feedstock/refill item |
| Biosample (bio-input) | 1 | Feeds T3-T4 biomass machines |
| Satellite (pre-launch item) - multiplier | 1 | Crafted, then launched |
| Satellite (pre-launch item) - attractor | 1 | Color/tint per ore acceptable from one base |
| Satellite (pre-launch item) - magnetic field | 1 | |

**Section total: ~9 files** (satellite items can share one base with tints).

---

## 5. Apparel - personal oxygen gear (the one custom wearable)

A cheap mask/oxygen pack crafted from ice (vacsuit alternative). This is the **most involved** art
because worn apparel draws on the pawn. Keep it simple (a mask or small pack).

| Asset | Variations | Notes |
|-------|------------|-------|
| Personal oxygen gear - inventory icon | 1 | Item icon |
| Personal oxygen gear - worn on pawn | N/E/S (3) | Worn overlay; west mirrors east. Drawn over the head (mask) or torso (pack) |

**Section total: ~4 files.** *Alternative to reduce scope: reuse a vanilla gas-mask-style worn
graphic and only draw a new inventory icon.*

---

## 6. UI icons

Flat icons, ~128x128 (or smaller), single file each. **No rotation.**

| Asset | Variations | Notes |
|-------|------------|-------|
| Lane icon - Oxygen | 1 | For HUD tooltip, dashboard gauges |
| Lane icon - Heat | 1 | |
| Lane icon - Pressure | 1 | |
| Lane icon - Biomass | 1 | |
| Total Ti / terraform icon | 1 | Headline readout + main-tab button |
| Main tab button icon | 1 | Bottom-bar button (can reuse Ti icon) |
| Research tab icon | 1 | Terraforming research tab |
| Gizmo - deep-drill target select | 1 | Command button (ore submenu can reuse ore item icons) |
| Gizmo - satellite launch | 1 | Command button |

**Optional:** custom dashboard gauge-bar fill texture (1); custom phase `LetterDef` icon (1).

**Section total: ~9 files.**

---

## 7. World / orbital (satellites in orbit)

Textures shown in Odyssey's orbital view as world-objects. **No rotation** (rendered as a flat
orbital sprite).

| Asset | Variations | Notes |
|-------|------------|-------|
| Orbital satellite - multiplier | 1 | Lane multiplier satellites (tint per lane from one base) |
| Orbital satellite - attractor | 1 | Per-ore attractor (tint per ore from one base) |
| Orbital satellite - magnetic field | 1 | Distinct silhouette |

**Section total: ~3 files** (could collapse to 1 base + tints).

---

## 8. Terrain

Tileable top-down terrain. **No rotation.** Recommend a few variants per type so large areas do not
visibly tile. Soil/rich-soil/water reuse vanilla (not listed).

| Asset | Variations | Notes |
|-------|------------|-------|
| Mars barren rock | 1 (rec. 2-3 variants) | Default dead surface; dark red |
| Mars red sand | 1 (rec. 2-3 variants) | Dusty patches |
| Mars cracked/rough | 1 (rec. 2 variants) | Visual variety |
| Transitional wet ground / mud | 1 (rec. 2 variants) | Appears at Liquid Water phase before vanilla soil takes over |

**Section total: ~4 base files** (more with variants).

---

## 9. Weather / overlays

| Asset | Variations | Notes |
|-------|------------|-------|
| Dust storm overlay | 1 | Optional full-screen weather overlay (like rain/snow). Can reuse a vanilla overlay tinted red if time-constrained |

Sky tint and the thin-atmosphere radiation event need **no textures** (color config + vanilla event
visuals).

**Section total: 0-1 file.**

---

## 10. Mod meta

| Asset | Size | Notes |
|-------|------|-------|
| Preview.png | 640x360 (Steam) | Store/cover image |
| ModIcon.png | ~64x64 / per Odyssey convention | Mod-list icon |

**Section total: 2 files.**

---

## 11. Optional / later - custom creatures

The "strange new lifeform" set-pieces (PLAN.md S14B Act II) could be custom alien fauna. **Each
custom creature is expensive**: a pawn needs body `_north/_east/_south` (3, west mirrors) per body
size, plus optional dead/dessicated. **Recommendation: reuse vanilla/Odyssey animals and insectoids
first**; only commission custom creatures if a signature alien is wanted.

Per custom creature (if pursued): **~3-4 files** (N/E/S + optional dead).

---

## Summary totals

| Section | Base files | Notes / optional |
|---------|-----------|------------------|
| 1. Terraforming machines (16) | 16 | +16 optional glow overlays |
| 2. Support / production buildings | ~22 | +1 launch overlay; drill tiers can palette-swap |
| 3. Mineable deposits | ~9 | x2-3 if drawing recommended variants |
| 4. Item icons | ~9 | satellite items can share a base |
| 5. Personal oxygen gear (apparel) | ~4 | or reuse vanilla gas-mask worn art |
| 6. UI icons | ~9 | +2 optional |
| 7. Orbital satellites | ~3 | or 1 base + tints |
| 8. Terrain | ~4 | x2-3 with variants |
| 9. Weather overlay | 0-1 | optional |
| 10. Mod meta | 2 | |
| **Base total** | **~78-80 files** | before optional overlays/variants |
| Custom creatures (optional) | +3-4 each | reuse vanilla first |

### Biggest scope levers (decide with the artist)
- **Per-machine glow overlays** (+16): big polish, optional.
- **Deep-drill tiers as palette swaps** vs unique art (saves ~6-8 files).
- **2-3 random variants** on deposits/terrain for natural scatter (multiplies those sections).
- **Personal oxygen gear worn art** - the only real apparel cost; reusing vanilla avoids it.
- **Custom creatures** - avoid early; reuse vanilla fauna.

### Rotation quick-reference
- **Needs NESW (rotatable, has a work/interaction side):** oxygen craft station, Terraforming
  Analyzer, deep-drill tiers. (3 unique each; west mirrors east.)
- **Single texture (non-rotatable):** all 16 terraforming machines, Oxygenator, research facility,
  food growers, satellite launch pad.
- **No rotation at all (items/terrain/deposits/icons/orbital):** every ore, crystal, item icon, UI
  icon, terrain tile, orbital satellite, and the meta images. (Crystals, as you noted, are single.)
