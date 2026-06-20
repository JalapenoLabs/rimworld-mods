# Terraformation - Design Plan

> A RimWorld 1.6 + Odyssey mod that adapts **The Planet Crafter** into RimWorld: land on a dead,
> airless, lethally cold red planet and terraform it back to a living world over a multi-year
> colony, raising Oxygen, Heat, Pressure, and Biomass until the sky turns blue, water returns,
> forests grow, wildlife and people repopulate, and the world becomes fully habitable.
>
> **Companion doc:** [`PLANET-CRAFTERS.md`](./PLANET-CRAFTERS.md) is the faithful reference for
> the source game. This file is the RimWorld adaptation design and the authority for what we build.
>
> **Status:** Complete design spec. Build sequencing intentionally left flexible (see §16).
> Original requirements brief preserved verbatim in Appendix A.

---

## 1. Vision

You are dropped onto a barren, Mars-like planet with no breathable air, deadly cold, dust storms,
and thin-atmosphere radiation. Survival depends on vacsuits and pressurized oxygenated rooms. The
core loop, faithful to Planet Crafter, is a positive-feedback megaproject: gather resources, build
and power terraforming machines, watch a global **Terraformation Index (Ti)** climb, and trigger a
fixed sequence of **15 phases** that visibly bring the planet to life. As the world heals, the
survival pressures lift, soil and water and forests appear, animals return, and other factions
repopulate the planet until it reaches **Complete Terraformation**.

Design philosophy (the user's directive): **lean on vanilla RimWorld and Odyssey systems wherever
possible, and sprinkle in a tiny taste of Planet Crafter** (four new ores, the terraforming
machines, the Ti HUD, the phase transformation). Not a 1:1 item-for-item port; a RimWorld game
that *feels* like Planet Crafter.

---

## 2. Hard constraints and dependencies

- **Requires:** RimWorld Core + **Odyssey DLC** (hard dependency). We reuse Odyssey's vacsuit
  apparel and the `VacuumResistance` stat.
- **Target version:** 1.6 (folder `1.6/`). Targets `net480`, builds via the repo's Mage toolchain.
- **Identity:** display name **"Terraformation"**, `packageId` **`jalapenolabs.rimworld.terraformation`**
  (lowercase, matching the repo convention like `jalapenolabs.rimworld.lavapowerpumps`),
  shipped as a git submodule like the repo's other mods.
- **Engine reality checks** (from implementation research, all confirmed against decompiled source):
  - Planet-surface maps are **always force-pressurized**; Odyssey vacuum exists only on space-layer
    maps. Therefore our "no oxygen" is a **custom system**, not Odyssey vacuum (§7).
  - Scenarios **cannot** force a biome/landing tile, nor force pawns to spawn *wearing* apparel.
    Both need C# (§13, §15).
  - Water has **no fluid simulation**; "rain/lakes" are weather forcing + cosmetic terrain swaps (§5, §13).
  - Most of the mod needs **no Harmony**; Harmony is reserved for a short, enumerated list (§17).

---

## 3. Decisions log (quick reference)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Atmosphere model | Custom Mars biome + custom O2 system; suffocation keyed off Odyssey `VacuumResistance` |
| 2 | Breathing | Suffocate unless in vacsuit OR enclosed powered-Oxygenator room; lifts at Oxygen-lane threshold |
| 3 | Metric lanes | Full 4 lanes, each a buildable machine tree: Oxygen, Heat, Pressure, Biomass |
| 4 | Units | Faithful Ti total (kTi/MTi/GTi/TTi); lanes ppq/pK/nPa/g, roll up x1000 |
| 5 | Pacing | ~3-5 in-game years to full terraform, default ~4; mod-settings speed multiplier |
| 6 | Phase count | Faithful 15 stages, 1:1 with Planet Crafter |
| 7 | Phase gating | 15 phases gate on total Ti; independent per-lane milestone unlocks run in parallel (incl. helmet-off) |
| 8 | Unlock model | Pure RimWorld research tree (spend points); Ti/lane thresholds drive world transformation + helmet-off only |
| 9 | Scenarios | Ship all 3: drop-pod trio, lone convict, crashed gravship crew (all vacsuit-clad) |
| 10 | World scope | Whole planet barren; global save-wide score; world-wide consequences |
| 11 | Resources | Lean on vanilla+Odyssey; 4 new ores (iridium, zeolite, Super Alloy, ice); crystal deposits yield vanilla mats, wealth-neutral until mined |
| 12 | Deep drill | Powered ~325W, autonomous, randomized daily output dropped on floor; vanilla ores + 4 new; per-drill target gizmo; research tiers |
| 13 | Machine tiers | 4 tiers per lane (16 core machines), exponential scaling |
| 14 | Biomass machines | Power-driven; T3-T4 also consume a simple on-site bio-input |
| 15 | Factions | Few small vacsuit factions at start; tribals gated; settlements ramp with phases; raiders suited until breathable |
| 16 | Win condition | Victory celebration + keep playing; optional voluntary ending hook |
| 17 | HUD | Top-right Ti/phase readout + hover tooltip + bottom-bar dashboard + phase letters |
| 18 | Research tree | Dedicated tab; per-lane + survival/drilling/crafting branches; vanilla bench for basics, custom Analyzer (+facility) for advanced |
| 19 | Food | Cheaper custom grower line T1+T2 (low power, slower; T1 in starting kit); vanilla hydroponics = T3 |
| 20 | Climate | Lethal cold (warms with Heat, capped temperate) + dust storms + thin-atmo radiation; all ease with phases/lanes |
| 21 | Oxygen economy | Ice = oxygen feedstock; Oxygenator runs on power + ice/water; personal gear from ice; transitions to water, then unneeded after Oxygen threshold |
| 22 | Build sequence | Full design spec now; milestones decided at implementation time |
| 23 | Art direction | Vanilla-consistent + Odyssey industrial/spacer; Mars red/rust; sky red->blue; placeholder-first |
| 24 | Name | "Terraformation" |
| 25 | Satellites | Lane-multiplier satellites (stacking, soft-capped) + per-ore attractor satellites; launched from a pad; visible in orbit via Odyssey's orbital view |
| 26 | Meteor showers | Custom incident scattering ores; rare baseline + satellite-driven/biased + milestone-triggered; mild impact hazard |
| 27 | Storyteller | Custom storyteller **Gaia** - the awakening world; a phase-reactive, three-act engine (resist -> unstable life -> green rush) that also rewards progress |

---

## 4. The terraforming score model

### Four lanes, one index
Total score is the **Terraformation Index (Ti)**, the sum of four lanes (faithful to Planet Crafter):

```
Ti = Oxygen + Heat + Pressure + Biomass    (each converted to the common Ti unit)
```

Each lane has its own physical unit and accumulates a **per-second rate** from every powered,
running machine assigned to it. Lanes and base units:

| Lane | Base unit | Display roll-up (x1000) | Raised by |
|------|-----------|--------------------------|-----------|
| Oxygen | ppq | ppq -> ppt -> ppb -> ppm | Oxygen machine tree |
| Heat | pK | pK -> nK -> uK -> mK -> K | Heat machine tree |
| Pressure | nPa | nPa -> uPa -> mPa -> Pa -> kPa | Pressure machine tree |
| Biomass | g | g -> kg -> t | Biomass machine tree (+ later, living things on the map) |

Total Ti rolls up **Ti -> kTi -> MTi -> GTi -> TTi**. The HUD shows the rolled-up total; the
tooltip shows each lane's value and `+/sec` rate (§9 formatting).

### Accumulation and pacing
- A central **GameComponent** owns the four lane accumulators and ticks once per in-game second,
  summing the `CurrentRate` of every registered, powered, running contributor comp and adding a
  one-second slice to each lane.
- Machine output rates are calibrated so an **actively-built colony reaches ~5 TTi (full
  terraform) in ~4 in-game years** (range 3-5). A **mod-settings speed multiplier** scales all
  rates for faster/slower runs.
- Calibration is a balancing task done against the faithful Planet Crafter thresholds in §5; the
  thresholds stay authentic and **machine rates are the tuning knob**.

---

## 5. The 15-phase progression (gated by total Ti)

The 15 named stages are **faithful 1:1 to Planet Crafter** and gate purely on **total Ti**
(decision #7). Thresholds below are the authentic Planet Crafter values; machine rates (§4) are
tuned so the arc spans the pacing target. Each phase fires a celebratory **letter** and applies
concrete RimWorld changes. Effects are cumulative.

| # | Phase | Ti threshold | RimWorld effect |
|---|-------|-------------|-----------------|
| 1 | Barren | 0 (start) | Red barren terrain, zero plants/animals/water, lethal cold, dust storms, radiation, no air |
| 2 | Blue Sky | 175 kTi | Custom GameCondition begins shifting sky tint from hostile red toward neutral |
| 3 | Clouds | 350 kTi | Cloud overlay/sky lightens further |
| 4 | Rain | 875 kTi | Rain/storm WeatherDefs unlocked and allowed; dust storms begin easing |
| 5 | Liquid Water | 3 MTi | Wet-ground terrain swaps begin; surface ice melts (also gated by Heat floor, §6) |
| 6 | Lakes | 50 MTi | Water bodies form via budgeted terrain swaps in low-lying/low-traffic cells; fishing viable |
| 7 | Moss | 200 MTi | Barren terrain converts toward soil; first ground cover; plant density lifts above 0 |
| 8 | Flora | 700 MTi | Grass and small wild plants spawn; zeolite surfaces become minable |
| 9 | Trees | 2 GTi | Trees grow; forests; wood economy returns |
| 10 | Insects | 8 GTi | First small fauna/insects spawn (staged species, §13/§17) |
| 11 | Breathable Atmosphere | 32 GTi | Oxygen-lane threshold (calibrated to align here) lifts suffocation/helmet requirement planet-wide; radiation ends |
| 12 | Fish | 120 GTi | Aquatic life populates lakes; fishing fruitful |
| 13 | Amphibians | 425 GTi | More fauna; pond/wetland life |
| 14 | Mammals | 1.25 TTi | Large animals spawn approaching vanilla density; tribal factions begin settling |
| 15 | Complete Terraformation | 5 TTi | Vanilla-density life, temperate climate, factions at normal amounts; victory celebration (§14) |

Thresholds, phase names, and per-phase effect lists are data-driven (a `TerraformPhaseDef` list)
so they can be tuned without code changes.

---

## 6. Per-lane milestone effects (parallel to phases)

Independent of the total-Ti phases, **each lane has its own milestone effects** (decision #7, the
dual system Planet Crafter uses). With the pure-research unlock model (#8), lanes do **not** unlock
tech; they drive felt world/survival effects:

- **Oxygen lane** -> at a calibrated threshold (aligned with Phase 11 Breathable), the custom
  asphyxiation system shuts off planet-wide: pawns no longer suffocate, helmets/vacsuits become
  optional, the Oxygenator loop becomes unnecessary.
- **Heat lane** -> drives a **continuous ambient temperature offset** (custom GameCondition), warming
  the planet from lethal cold toward a comfortable range, **capped temperate** (never runaway hot).
- **Pressure lane** -> **continuously reduces the frequency** of dust storms and thin-atmosphere
  radiation events; radiation is fully gone by the Breathable phase.
- **Biomass lane** -> primarily a score contributor; reinforces the life phases. Living plants and
  animals on the map also feed Biomass once they exist (the "machines drive the metric, but life
  contributes too" reconciliation).

---

## 7. Custom atmosphere / oxygen survival system

The signature early-game system (decisions #1, #2, #21).

- **No native vacuum on surface maps**, so we ship our own: a **MapComponent** + a custom
  **asphyxiation Hediff** applied each tick to pawns who are (a) not adequately protected and
  (b) not in a safe oxygenated space.
- **Protection is keyed off Odyssey's `VacuumResistance` stat**: a pawn with sufficient
  `VacuumResistance` (vacsuit + helmet ~101%) is safe outdoors. This means Odyssey vacsuits,
  EVA-capable armor, and any modded vacuum gear all protect, for free.
- **Safe oxygenated rooms:** an enclosed room (our own room-seal detection via RimWorld's
  `Room`/`RoomGroup`) served by a powered **Oxygenator** building is breathable without a suit.
- **Oxygen economy (decision #21):**
  - **Ice is the oxygen feedstock.** A **basic oxygen craft station** converts ice into oxygen
    (canisters) and personal oxygen gear.
  - The **Oxygenator** consumes **power + ice/water** slowly to keep its sealed room breathable.
  - **Personal oxygen gear** (mask/tank crafted from ice) is a cheaper alternative to a full vacsuit
    for extra pawns.
  - **After the heat/water phases**, melted water replaces ice as feedstock.
  - **After the Oxygen-lane threshold** (Phase ~11), the entire loop is unnecessary.
- **Vacsuits:** reuse Odyssey's apparel defs (confirm exact defNames in-game: likely
  `Apparel_Vacsuit`, `Apparel_VacsuitHelmet`). Starting scenarios spawn pawns **already wearing**
  them (needs C#, §15) and grant the Odyssey research that makes them craftable.

---

## 8. Climate and hazards (decision #20)

The barren start is the most immersive option: multiple pressures that ease as terraforming
progresses, each tied to a lane/phase.

- **Lethal cold** -> custom GameCondition temperature offset driven by the **Heat lane** (§6);
  unprotected pawns risk hypothermia on top of suffocation. Warms to temperate and caps.
- **Dust storms** -> custom Mars-like weather/incident reducing visibility and solar output;
  frequency scales down with the **Pressure lane**, ending around the Rain/atmosphere phases.
- **Thin-atmosphere radiation** -> custom periodic event applying a radiation Hediff to exposed
  (outdoor, unprotected) pawns; vanilla has no radiation, so this is bespoke. Frequency scales down
  with **Pressure/Oxygen**, fully gone by the Breathable phase.

All three are data-driven and scale continuously; they are the felt reason the Heat and Pressure
lanes matter before their abstract score does.

---

## 9. HUD and UI (decision #17)

The full package, all achievable without Harmony (one optional cosmetic Harmony anchor):

- **Always-on top-right readout** (drawn in `GameComponentOnGUI`, anchored near the date/speed
  cluster at `UI.screenWidth - 200f`): shows **current phase + total Ti** (rolled-up prefix).
- **Hover tooltip** (`TooltipHandler.TipRegion`): per-lane values and `+/sec` rates, e.g.
  `Oxygen: 4.2 ppt (+0.30 ppq/s)`, plus progress to next phase.
- **Bottom-bar dashboard** (`MainButtonDef` + `MainTabWindow`): per-lane fillable-bar gauges,
  next-threshold progress, current phase + the 15-phase timeline, and a list of contributing
  machines per lane (counts and total rate).
- **Phase letters** (`Find.LetterStack.ReceiveLetter`, PositiveEvent): a celebratory letter on each
  of the 15 transitions; a bigger one at Complete Terraformation.
- **Formatting:** an SI-prefix formatter (value + rolled-up unit) shared by readout, tooltip,
  dashboard. Guard against re-firing letters by persisting the last-reached phase index.

---

## 10. Machines (decisions #3, #13, #14, #19, #21)

### Four lanes x four tiers (16 core terraforming machines)
Each lane has T1-T4 buildings, each a separate `ThingDef` gated by a research project (#8), with
**roughly exponential** output and power draw across tiers (the endgame is a few top-tier machines
on a heavy power grid). Each carries a custom `CompProperties_TerraformingContributor { lane,
ratePerSecond }`; the contributor registers with the score GameComponent and contributes
`ratePerSecond` to its lane only while powered and switched on.

- **Oxygen line** (O2 ppq/s): e.g. oxygenator/algae stack -> vegetube-likes -> tree-spreader-likes
  (outdoor placement for higher tiers, RimWorld-flavored).
- **Heat line** (Heat pK/s): heaters of rising output; top tiers also nudge Pressure (faithful).
- **Pressure line** (Pressure nPa/s): atmosphere drills/gas releasers.
- **Biomass line** (Biomass g/s): T1-T2 = power-only (algae tanks, greenhouses), **T3-T4 also
  consume a simple on-site bio-input** (e.g. a "biosample" crafted from local plants/meat, or
  vanilla raw food) to keep running (decision #14). Higher tiers emit a little Oxygen too.

### Support buildings
- **Oxygenator** (survival, §7): powered, consumes ice/water, makes an enclosed room breathable.
- **Basic oxygen craft station** (§7): ice -> oxygen + personal oxygen gear.
- **Custom research bench(es)** (§12): Terraforming Analyzer + a linked higher-tier facility.
- **Food growers** (decision #19): a **cheap, low-power, slower T1 and T2 grower line**; vanilla
  hydroponics is effectively "T3." **A few T1 growers are included in every starting scenario.**
- **Power:** use vanilla/Odyssey generators; no custom power line unless balancing demands it.

---

## 11. Deep-drill auto-miner (decision #12)

A powered, **autonomous** miner (no pawn/operator), distinct from vanilla's pawn-driven deep drill:

- **Power:** ~325W (positive `basePowerConsumption`); power is the on/off gate.
- **Output:** at **randomized intervals spread across the in-game day**, produces a small ore stack
  and drops it on the floor near the building (`GenPlace.TryPlaceThing(..., ThingPlaceMode.Near)`,
  which auto-merges onto existing piles to limit clutter). Backpressure: pause production if the
  output cell is backed up.
- **Ore pool:** vanilla mineable resources (steel, plasteel, gold, silver, uranium, jade, etc.) +
  the 4 new ores (iridium, zeolite, Super Alloy; ice early).
- **Targeting:** a **per-drill gizmo** selects a "target" ore that gets a **heavily weighted (not
  guaranteed)** chance; the rest comes out randomly.
- **Tiers:** research-gated tiers improve yield rate and how strongly targeting biases the roll.
- On a mountain-free Mars map this is the **backbone supply line**.

Implemented as a fresh `ThingComp` (`CompAutoMiner`) owning its own randomized timer + target field;
**no Harmony**.

---

## 11B. Orbital satellites and meteor showers (decisions #25, #26)

### Satellites (#25)
- Built and launched from a **Satellite Launch Pad** (craft a satellite, a pawn launches it). Each
  launched satellite becomes a **real orbital world-object, visible in orbit via Odyssey's built-in
  orbital view** (leverage the Odyssey orbital-object system; exact API to confirm at implementation).
- **Two families:**
  - **Lane-multiplier satellites** (Oxygen / Heat / Pressure / Biomass): each launched satellite adds
    a permanent, **stacking, soft-capped global multiplier** to all of that lane's machine output.
    The late-game scaling path - "a few top-tier machines + orbital multipliers."
  - **Attractor satellites** (attract-uranium, attract-Super Alloy, attract-iridium, attract-zeolite,
    etc.): each raises meteor-shower **frequency** and **biases** drops toward its ore (faithful to
    Planet Crafter's per-ore "colored" meteors). Includes a **Magnetic Field** satellite that boosts
    the Pressure lane and accelerates radiation-easing.
- Costs scale with the strength of the effect; **soft cap / diminishing returns** prevent runaway.
  Satellite counts and multipliers persist in the score GameComponent (orbital objects via a
  WorldComponent).

### Meteor showers (#26)
A custom **Meteor Shower incident** scatters multiple minable ore chunks/deposits across the map.
Three trigger paths:
1. **Rare baseline** storyteller showers - an early-game windfall before satellites exist.
2. **Satellite-driven & biased** - each attractor satellite in orbit raises frequency and skews the
   ore mix toward its target.
3. **Milestone showers** (faithful to Planet Crafter) - one-time special showers fired by the score
   GameComponent at specific Ti/phase milestones, dropping rare ore caches (e.g. a first Super Alloy
   or iridium cache at a set phase).

- **Mild hazard:** impacts can strike pawns/things left in the open (like vanilla meteorites), so
  players shelter during a storm; it is primarily a resource windfall, not a raid.
- **Drops:** vanilla ore chunks + the 4 new ores; the biased mix depends on active attractors and the
  milestone table. This is the **primary resource lifeline** on a mountain-free Mars map, complementing
  deep drills (§11) and scattered crystals (§12).

---

## 12. Resources, ores, and research (decisions #11, #18)

### Resources / ores (#11)
- **Four new ores only:** **iridium, zeolite, Super Alloy, ice** - used to craft some terraforming
  machines, Odyssey-flavored items, and decorative collectibles. **Ice is phase-1-only** (melts
  away after the heat/water phases); it is the oxygen feedstock (§7).
- **Scattered crystal deposits** generate across the map and **yield vanilla materials** (steel,
  components, plasteel, etc.) when mined - an early-game boost before committing to full mining.
  As mineable things they are naturally **wealth-neutral until mined** (no raid-point inflation).
- Everything else leans on **vanilla + Odyssey recipes and materials**; terraforming recipes use
  vanilla mats wherever possible, with the four ores as the "tiny taste of Planet Crafter."
- New ores are `ThingDef`s under `ResourceBase`; tuned `stackLimit`/`Mass`/`MarketValue`. Mineable
  forms are `Mineable`-style rocks with `mineableThing`. Scatter via a custom/high-density
  `GenStep_ScatterLumpsMineable` in the Mars `MapGeneratorDef`.

### Research tree (#18)
- **One dedicated "Terraforming" research tab** (`ResearchTabDef`).
- **Trunk:** life-support/oxygen survival + basic crafting (early, vanilla bench).
- **Four parallel lane ladders** (Oxygen, Heat, Pressure, Biomass), each T1->T4.
- **Drilling/Resources branch** (deep-drill tiers, ore processing) and an **advanced Crafting branch.**
- **Benches:** basics at the vanilla research bench; **advanced projects require a custom
  "Terraforming Analyzer"** building, and the **top tiers additionally require a linked
  higher-tier facility**.
- **Pure research-points unlocks** (#8): no Ti-gating on buildability. Layout via distinct
  `researchViewX/Y` per project (engine errors on duplicate coordinates in a tab).

---

## 13. World, biome, and map generation (decision #10)

- **Whole planet barren:** a custom **barren Mars `BiomeDef`** (+ `BiomeWorker`) that dominates
  world generation so every tile is dead red rock - wherever the player lands, it's Mars. (May ship
  a custom world preset/`MapGeneratorDef` to guarantee this.)
- **Global score:** the terraforming GameComponent is **save-wide**, independent of any one map.
- **World-wide consequences:**
  - Sky tint + the helmet-off milestone apply everywhere.
  - **Faction/settlement ramp** across the planet (§14).
  - **World-map tiles shift biome** at major phases (barren -> greening -> verdant), so the planet
    map visibly heals. (Heaviest piece; see risks §17.)
- **Active colony maps** get the full transformation: budgeted `TerrainGrid.SetTerrain` swaps
  (red -> wet -> soil -> rich soil -> water), plant-density ramp + staged species, animal spawning.
- **Mars look:** custom red/rust `TerrainDef`s (fertility 0 at start) and a custom GameCondition for
  the sky; terrain fertility rises through phases to gate which plants/trees become eligible.

---

## 14. Factions and endgame (decisions #15, #16)

### Faction ramp (#15)
- **Start:** a couple of low-density industrial/spacer-type factions whose pawns **wear vacsuits**;
  tribal factions exist in the world but **settle nothing** initially.
- **Ramp:** total settlement count scales up phase by phase toward vanilla density; **tribals begin
  settling around Phase 14 (Mammals)**; full vanilla density by Phase 15.
- **Suited raiders:** raiders/visitors wear vacsuits while the atmosphere is unbreathable, dropping
  them once breathable (apparel forcing via PawnKind patches and/or a generation patch, §17).
- Controlled via `FactionDef` fields (`requiredCountAtGameStart`, `maxCountAtGameStart`,
  `settlementGenerationWeight`, `hidden`) + a **WorldComponent/IncidentWorker** that injects
  settlements as phases advance.

### Endgame (#16)
- **Complete Terraformation** fires a big congratulatory event and marks the save as "terraformed"
  (persistent recognition); the colony **keeps playing** on the lush world.
- **Optional voluntary ending hook** (e.g. an Odyssey gravship departure or a monument) for players
  who want closure - never forced.

---

## 14B. Custom storyteller - Gaia (decision #27)

A bespoke storyteller named **Gaia**: the planet itself as a living force, the antagonist that
turns into an ally. Its defining, no-vanilla-equivalent feature is that **pacing, threats, and
gifts react to the terraforming GameComponent** (current Ti and phase), via custom
`StorytellerComp`s layered on a `StorytellerDef` with its own difficulty/population curves. The
colony's adversary is the dead world waking up.

### Three-act, phase-reactive arc
- **Act I - the world resists (Barren -> Rain).** Threats are environmental, not social: deep
  freezes, global dust storms, thin-atmosphere solar flares that EMP power, micrometeor pinging,
  pressure-leak / oxygen-system failures, and brief "the planet resists" debuffs that slow Ti gain.
  Few or no raids - nobody else is out here yet.
- **Act II - life is unstable (Water -> Insects).** Events turn ecological and strange: the First
  Rain (mood lift, perhaps acid-tinged at first), uncontrolled blooms (sudden plant/insect
  explosions, alien-fauna infestations), atmospheric instability that fluctuates a lane, toxic
  outgassing as terrain transforms, strange new lifeforms as set-pieces, and the first faction
  scouts noticing the greening world.
- **Act III - the green rush (Breathable -> Complete).** The world is now a prize: refugee/settler
  waves seeking the habitable haven, pilgrimage joiners, land-grab raids over valuable terraformed
  ground, territorial returning megafauna, and trade booms as caravans flock to the oasis. Ties into
  the faction ramp (§14).

### Partner, not just tormentor
- **Gift/reward cycle tied to progress:** milestone resource caches, orbital supply drops, and the
  satellite/meteor showers (§11B) fire as celebrations of hitting Ti targets - Gaia funds the
  megaproject as you advance.
- **Terraforming-aware difficulty:** threat scaling considers **Ti**, not just colony wealth, so the
  world "notices" you more the more you reshape it.
- **Signature setback events** unique to this mod: machine cascade failures, a "false spring"
  (premature thaw then hard refreeze), a "bloom" that is half windfall / half infestation, and an
  atmospheric reversal that temporarily knocks a lane back - real drama around your numbers.

### Implementation
- `StorytellerDef` + custom `StorytellerComp`(s) that query the score GameComponent for phase/Ti and
  weight/gate incident categories per act; standard difficulty and population-intent curves.
- New `IncidentDef`s + workers for the act-specific events above (many are custom; reuse vanilla
  incident workers where one fits). Event letters written in Gaia's "voice."
- Gaia is selectable like any storyteller; the mod's scenarios can default to her but not force her.

---

## 15. Starting scenarios (decision #9)

Ship **three selectable scenarios** (`ScenarioDef`s), all sharing: barren Mars start, mandatory
vacsuits worn at spawn, scattered minable crystals + ore chunks, ice present, a few T1 food growers,
the Odyssey research needed to craft vacsuits, minimal supplies.

1. **Drop-pod trio** - 3 colonists (configurable) in vacsuits, drop-pod landing, sparse supplies.
2. **Lone convict** - single vacsuit-clad pawn, maximally faithful, brutal.
3. **Crashed gravship crew** - 3-5 survivors in vacsuits with a wrecked ship as starting structure
   and a bit more salvage (Odyssey-native framing).

**Forcing worn vacsuits at spawn needs C#** (custom `ScenPart` or a Harmony postfix on starting-pawn
generation that wears freshly-made vacsuits) - XML `StartingThing_Defined` only drops loose items.

---

## 16. Build sequencing (decision #22)

This document is the **complete design spec**; milestones are decided at implementation time. A
likely-sensible (non-binding) order, for reference only: barren biome + one scenario + survival
(suffocation/oxygen/climate) + score GameComponent + HUD readout + T1 of each lane + first phases
(blue sky, rain) as a playable vertical slice; then full tiers, biomass inputs, deep drills, all 15
phases, faction ramp, world-map biome shift, dashboard, all 3 scenarios, art, and balancing.

---

## 17. Technical architecture and risks

### Core constructs (mostly Harmony-free)
- **Score + phases:** a `GameComponent` (save-wide) holds the four lane accumulators, total Ti,
  current phase, last-fired-phase index; ticks once/in-game-second; persists via `Scribe_Values`.
- **Per-map transformation:** a `MapComponent` per map reads the score and applies **budgeted**
  `SetTerrain` swaps and staged plant/animal seeding (cap cells/tick to avoid stutter).
- **Sky / temperature / plant+animal density:** custom permanent `GameCondition`(s) registered by
  the GameComponent, overriding `SkyTarget`/`SkyTargetLerpFactor`, `TemperatureOffset`,
  `PlantDensityFactor`, `AnimalDensityFactor` - the engine aggregates these for free.
- **Suffocation/radiation:** custom Hediffs applied from the MapComponent, gated on
  `VacuumResistance` + room-seal/Oxygenator checks.
- **Machines/drills/benches/recipes/resources:** XML `ThingDef`/`RecipeDef`/`ResearchProjectDef`/
  `ResearchTabDef` + custom `ThingComp`s (`CompTerraformingContributor`, `CompAutoMiner`,
  `CompOxygenator`). Model on the repo's `lava-power-pumps` (powered comp + `MapComponent` +
  settings + DefOf) and `electricity-meter` (`ITab`/`Listing_Standard` UI).
- **HUD:** `GameComponentOnGUI` + `TooltipHandler` + `MainButtonDef`/`MainTabWindow` + letters.
- **Weather control:** suppress/allow rainy `WeatherDef`s by phase (throttled `TransitionTo` from
  the MapComponent, or a `WeatherDecider` patch).

### Where Harmony IS needed (enumerated)
- Forcing starting/faction pawns to **spawn already wearing** vacsuits.
- **Staged flora/fauna species** per phase (patch `BiomeDef.AllWildPlants/AllWildAnimals`) and/or
  **world-tile biome swaps** as the planet heals.
- Optional cosmetic: pixel-perfect anchoring of the HUD readout beneath the vanilla date/speed
  cluster (postfix on `GlobalControls`). Not required.

### Persistence
`Scribe_Values` for lane accumulators/phase index; `Scribe_Collections` for the set of
already-converted cells (so terrain isn't re-swapped on load); contributor comps re-register on
spawn (don't scribe the list). Re-register conditions in `FinalizeInit`/`LoadedGame` if the mod is
added mid-save.

### Risks / to-calibrate
- **World-map biome repaint** is the heaviest, highest-risk feature; if it proves too costly it can
  degrade gracefully to "transformation mainly on active maps + sky/faction changes world-wide."
- **Mass `SetTerrain`** is the only real per-frame performance hazard - must be budgeted.
- **Machine-rate calibration** against the faithful Ti thresholds to hit the ~4-year pacing.
- **Exact Odyssey defNames** (vacsuit apparel, any vacuum hediff/stat casing) must be verified
  in-game / from the Odyssey XML before coding.
- **Odyssey orbital world-object/satellite-view API** must be verified before building visible
  orbital satellites (§11B); fallback is an abstract orbital WorldObject we render ourselves.
- **Satellites/meteors** add: a `WorldComponent` for orbital objects + multipliers, a custom Meteor
  Shower `IncidentDef`/worker, and milestone-shower triggers from the score GameComponent. The
  GameComponent applies satellite multipliers to lane rates before accumulating.
- **Water placement** must avoid trapping pawns or destroying buildings (low-traffic cells only).
- Mod settings to expose: terraform **speed multiplier**, hazard intensity, starting-grower count,
  optional toggles for the heavier world-map effects.

---

## 18. References
- [`PLANET-CRAFTERS.md`](./PLANET-CRAFTERS.md) - source-game reference.
- RimWorld manuals: `~/.claude/manuals/RimWorld/*.md` (read in full before writing any mod code).
- Decompiled engine source: `RimworldForCICD/DecompiledSource/` (ground truth for class/method names).
- Repo patterns: `mods/lava-power-pumps/` (powered comp + MapComponent + settings + research),
  `mods/electricity-meter/` (ITab/Listing UI), `mods/tiered-gravship-shields/` (PatchOperation usage).

---

*Per the repo's source-of-truth policy: when any design decision here changes, update this file in
the same change.*

---

## Appendix A - Original requirements brief (verbatim)

What I want:
- Rimworld Odyssey DLC is required, so we can leverage vacsuits, no-oxygen environments, etc.
- In Rimworld, new starting scenario for a mars-like planet (there could be more in the future too).
- Your starting pawns MUST start with vacsuits (part of the starting scenario)
- When the game starts, there is no oxygen! The planet must look dramatically different. Super red, mars-like, no trees, no wild life, no rivers, no water.
- There could be other starting factions too, but significantly smaller spawn rate. They all have vacsuits too. Possibly no tribal starting factions (they can be enabled but not spawn yet).
- When you start, resources are scattered everywhere on the map as minable chunks. We could probably also provide our own versions of steel/components/plasteel, etc as easy minable crystals but passable and lighter.
- Some new ores will be present on the map: Super Alloy, iridium, zeolite and ice. Ice will only be present in the first phase.
- We provide a basic craft station where ice can be used to craft oxygen equipment or entry level room oxygen stuff
- There's food growing machines that would substitute crops for food
- There's x3 large trees of machines to build: Oxygen machines, pressure machines, and heat machines. Just like planet crafters, you must build each and grow the metrics. There's a "total" terraforming score that grows over time too. A number in the status right side like an alert (we can model how the game shows in-game time and in-game speed and such). Hovering over it shows live oxygen/pressure/heat stats.
- As these change, just like planet crafters the planet will begin to change. And in phases... First, the sky will begin to change. Then eventually water and rain, then eventually the terrain begins to change, then there's grass, then there's trees, then there's wildlife that starts to spawn, then tribes begin to spawn around the planet until they hit the normal spawn amount.
- I also want deep drill miners that can try to mine specific resources. They will require a lot of power (like 325w) and will slowly turn out a new tiny stack of ores slowly over time throughout the in-game day (random calculated times between chunks) in-game time and drop them on the floor. They can't mine specific resources but we can increase the chances of getting the exact ore that they want from the deep drill miner.
- A full dedicated research tree just for this "Terraforming"
