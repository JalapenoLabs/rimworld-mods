# The Planet Crafter - Game Reference

> A complete reference for how **The Planet Crafter** (Miju Games, 1.0 released March 2024)
> works. This document exists to inform the design of the `planet-terraforming` RimWorld mod.
> It describes the source game faithfully; design adaptations live in `PLAN.md`, not here.
>
> Source basis: the official Planet Crafter Fandom wikis (`planet-crafter.fandom.com` and the
> data-heavy `planet-crafter-terraformation.fandom.com`), `planetcrafter.wiki.gg`, the Steam
> store/community guides, and corroborating third-party guides (ScreenRant, GameRant, TheGamer,
> TechRaptor, The Nerd Stash, Casual Game Guides). Where the Fandom wiki blocked direct fetches,
> figures were cross-checked across multiple independent guides. Late-game numbers shifted across
> the Caves & Crater, Space Trading, and Moons updates and are flagged as version-approximate.

---

## 1. Premise and Core Loop

You are a convict dropped onto a hostile, dead planet with one job: make it habitable. At the
start there is **no breathable air, lethal cold, near-zero atmospheric pressure, and a hostile
red sky**. The core loop is a positive-feedback economy:

```
gather resources -> craft & place terraforming machines -> machines passively raise the
planet's environmental stats -> rising stats unlock new tiers of blueprints/research ->
better machines raise stats faster -> the planet visibly transforms, stage by stage ->
repeat until the world is breathable, green, and self-sustaining
```

Terraforming progress is itself the currency that unlocks the technology needed to terraform
faster, while literally reshaping the world (sky, water, plants, animals).

### Win condition

There is no hard "game over" win. Terraforming is **complete at a Terraformation Index of
~5 TTi** (the final "Complete Terraformation" stage; exact value varies 4-7 TTi by planet).
Reaching it (plus late-game narrative steps) lets the player build an **Extraction Platform**
and choose one of three faction endings. Triggering an ending does not close the save; you
respawn on the finished planet and may keep playing. "Completion" is therefore the terraforming
milestone plus a narrative exit, not a wall.

---

## 2. The Terraformation Index (Ti)

The **Terraformation Index (Ti)** is the single global score that drives both stage milestones
and technology unlocks. It is the **sum of the planet's environmental contributions**, each
normalized into a common Ti unit:

> **Ti = Oxygen + Heat + Pressure + Biomass** (each converted to the common unit)

Each lane contributes equally: one normalized unit from any lane raises Ti the same amount.
Early game you can only influence **Oxygen, Heat, and Pressure**; **Biomass** is introduced
mid/late game once the planet supports life, and is split into **Plants, Insects, Animals**.
(Toxic-planet variants add a **Purification (Pu)** contribution.)

### Ti unit scale (SI prefixes, x1,000 each)

| Unit | Value          |
|------|----------------|
| Ti   | base unit      |
| kTi  | 1,000 Ti       |
| MTi  | 10^6 Ti        |
| GTi  | 10^9 Ti        |
| TTi  | 10^12 Ti       |
| PTi  | 10^15 Ti       |

> Watch the trap: lowercase `mTi` = milli (tiny); uppercase `MTi` = mega.

Accumulation is continuous. Every placed machine adds a **per-second rate** to its lane
(Oxygen/s, Heat/s, Pressure/s, Biomass/s). Those rates raise the lane stat, the lane stat is
converted to Ti, and total Ti ticks upward in real time as long as machines run.

### Per-lane units

Each lane is shown in its own physical unit, which itself rolls up x1,000 with SI prefixes.
The base unit folded into the Ti sum is deliberately tiny so early machines produce visible,
satisfying numbers.

| Metric        | Base unit in Ti | Display prefixes (roll up x1,000)        | Raised by                                            |
|---------------|-----------------|------------------------------------------|------------------------------------------------------|
| **Oxygen**    | ppq             | ppq -> ppt -> ppb -> ppm                 | Plant-based machines (Vegetubes, Algae, Tree/Grass/Flower Spreaders) |
| **Heat**      | pK              | pK -> nK -> uK -> mK -> K (absolute)     | Heaters (later fusion/large heat sources)            |
| **Pressure**  | nPa             | nPa -> uPa -> mPa -> Pa -> kPa            | Drills and Gas Extractors                            |
| **Biomass**   | g               | g -> kg -> t                             | Living organisms (Plants + Insects + Animals)        |

**Biomass sub-metrics** (all measured in grams, each adds to the Biomass lane):
- **Plants** - first biomass source: Biodome, Seed/Tree/Grass/Flower Spreaders, Algae.
- **Insects** - larvae incubated into butterflies/bees (Butterfly Dome/Farm, Beehive, Outdoor Farm, Insect Spreader Rocket).
- **Animals** - fish eggs (Aquarium/Fish Farm), frog eggs (Amphibian Farm), mammals (genetic pipeline + Animal Shelter).

### Stages UI

The terraforming/stages panel shows: **current total Ti** (with rolled-up prefix), **per-lane
stats and live per-second rates** (so you can see which lane is weakest), and a **stage gauge**
showing the current named stage, the next stage, and progress toward the next Ti threshold.
Many unlocks key off **individual lane milestones** (a specific Oxygen/Heat/Pressure/Biomass
value), not just total Ti, which is why balanced play matters.

---

## 3. Terraformation Stages (in order)

The planet advances through named stages, each gated at a global Ti threshold. The order is
fixed and reflects realistic dependency: **atmosphere/sky first, then water, then plants, then
animals.** Stages 1-11 are stable across sources; Fish/Amphibians/Mammals thresholds differ by
patch (rebalanced values shown in parentheses).

| #  | Stage                     | Ti threshold        | What happens (visual / mechanical)                                  |
|----|---------------------------|---------------------|---------------------------------------------------------------------|
| 1  | **Barren**                | 0 (start)           | Lifeless red planet, hostile sky, no air/water                      |
| 2  | **Blue Sky**              | 175 kTi             | Sky shifts from hostile red to calm blue                            |
| 3  | **Clouds**                | 350 kTi             | Clouds begin to form                                                |
| 4  | **Rain**                  | 875 kTi             | First weather: rain and storms                                      |
| 5  | **Liquid Water**          | 3 MTi               | Ground looks wet/saturated; water becomes possible; Biomass begins  |
| 6  | **Lakes**                 | 50 MTi              | Water pools collect in basins, grow into lakes; Lake Water Collector |
| 7  | **Moss**                  | 200 MTi             | Ground slowly turns green as moss spreads; first plant biomass      |
| 8  | **Flora**                 | 700 MTi             | Grass and small plants sprout; DNA Manipulator; zeolite exposed     |
| 9  | **Trees**                 | 2 GTi               | Larger vegetation / trees grow; Tree Spreaders                      |
| 10 | **Insects**               | ~8 GTi (Incubator ~5 GTi) | First macro life (butterflies); wild larvae spawn             |
| 11 | **Breathable Atmosphere** | ~32 GTi             | Atmosphere holds enough O2 to breathe without a supply (gauge = inf) |
| 12 | **Fish**                  | 120 GTi (5 TTi reb.) | Aquatic life appears; animal biomass begins                        |
| 13 | **Amphibians (Frogs)**    | 425 GTi (45 TTi reb.) | Frogs emerge                                                       |
| 14 | **Mammals**               | 1.25 TTi (1 PTi reb.) | Large terrestrial mammals via genetic pipeline                     |
| 15 | **Complete Terraformation** | ~5 TTi (4-7 by planet) | Fully habitable, self-sustaining; enables Extraction Platform / endings |

### Visual transformation summary

1. **Atmosphere phase (kTi):** sky red -> blue (175k) -> clouds (350k) -> rain/storms (875k).
   Driven by Heat + Oxygen + Pressure (no biomass yet). The big early payoff.
2. **Hydrosphere phase (MTi):** ground wet (3M) -> lakes grow (50M). Liquid water gates all later life.
3. **Flora phase (hundreds of MTi -> GTi):** moss (200M) -> grass/small plants (700M) -> trees (2G).
4. **Fauna phase (GTi -> TTi):** insects (8G) -> breathable air (32G) -> fish (120G) -> frogs (425G) -> mammals (1.25T).
5. **Completion (5 TTi):** lush, living, breathable world.

Precise order: **oxygen/heat/pressure climb together -> blue sky -> clouds -> rain -> wet ground
-> lakes -> moss -> grass -> trees -> insects -> breathable air -> fish -> frogs -> mammals ->
complete.** Vegetation precedes most fauna; breathable atmosphere lands between insects and fish.

---

## 4. Survival Mechanics

Three depleting gauges (HUD bottom-left); any reaching **zero = death** (black out, respawn at
last pressurized building, drop recoverable inventory; equipped gear stays).

### Oxygen (drains fastest)
- Empties whenever the player is outside a pressurized structure and while underwater (~2 O2/s, community figure).
- Base max **100** with no tank.
- Replenished by: entering pressurized buildings (instant + held), **Oxygen Capsule** (instant full refill; from crates, Craft Station ~2 Cobalt, or Gas Extractor), oxygen machines, and ultimately terraforming the atmosphere. At **Breathable Atmosphere (~32 GTi)** the gauge shows infinity (still drains underwater).
- **Oxygen Tanks** (one equipped) raise the max:

  | Tier | Bonus | Total |
  |------|-------|-------|
  | T1   | +45   | 145   |
  | T2   | +100  | 200   |
  | T3   | +180  | 280   |
  | T4   | +270  | 370   |
  | T5   | +350  | 450   |

### Water / Thirst (moderate drain)
- Restored only by drinking **Water Bottles** (from ice early; from Lake Water Collectors later).

### Health / Food (combined bar, slowest drain)
- Acts as both hunger and HP. Reduced by hunger over time and by damage (falls; meteor-storm hits, which target the player's location and cannot damage buildings, so shelter indoors during storms).
- Healing = eating food (no separate heal item).

### Survival equipment (one of each slot)
- **Backpacks** - 6 tiers, 12 slots up to 45 slots at T6. (T1 = 2 Iron; T4 = 3 Super Alloy + 1 Titanium.)
- **Oxygen Tanks** - see table above.
- **Jetpack** - flight/vertical boost, up to T5.
- **Agility Boots** - movement and jetpack speed.
- **Exoskeleton** - movement/carry, up to T5.

---

## 5. Resources and Ores

### Basic ores (mineable widely; T1 Ore Extractor byproducts)

| Ore           | Where found                                          | Primary uses                                          |
|---------------|------------------------------------------------------|-------------------------------------------------------|
| **Iron**      | Everywhere (surface rocks, caves); biggest extractor share | Backpacks, drones, circuit boards, most early builds |
| **Cobalt**    | Everywhere                                            | Oxygen Capsule, Super Alloy, early machines           |
| **Magnesium** | Everywhere                                            | Super Alloy, spreaders, mid-game builds               |
| **Silicon**   | Everywhere                                            | Circuit boards, Bioplastic Nugget, Super Alloy        |
| **Titanium**  | Everywhere                                            | Super Alloy, Launch Platform, equipment, Ore Extractor T3 |
| **Aluminium** | Concentrated in Aluminum Hills, caves, orange meteors | Super Alloy/rods, circuit boards, mid/late builds     |
| **Iridium**   | Iridium Mine, caves, the Meteor Crater glowing meteor (mine before Lakes floods it), red meteors | Iridium Rod, heaters, Pulsar Quartz, Rocket Engine |

### Rare / advanced ores (require T2+ Ore Extractor)

| Ore                | Where found                                                  | Primary uses                                            |
|--------------------|--------------------------------------------------------------|---------------------------------------------------------|
| **Osmium**         | Osmium caves (often ice-sealed until Heat melts them); blue meteors | Fusion Energy Cell, Osmium Rod (unlocks 39 GTi), Pulsar Quartz |
| **Uranium**        | Crates early; Uranium Caves; green-trail meteors             | Uranium Rod -> fuels Nuclear Generators; Rocket Engine  |
| **Zeolite**        | Pushed to surface by Flora roots (~150-700 MTi); Zeolite Cave; Meteor Field. No meteor event yields it. | Tree Spreader T2/T3, Butterfly farms, DNA Manipulator, Gas Extractor |
| **Sulfur**         | Sulfur Fields, osmium caves; blue meteors                    | Jetpack/BioLab T2, Fertilizer, Explosive Powder, Mutagen |
| **Obsidian**       | Volcanoes biome; small caves                                 | Fusion Energy Cell and advanced recipes                 |
| **Super Alloy (ore)** | Super Alloy Cave; Sand Falls, ledges; purple meteors      | Super Alloy Rod, high-tier builds (distinct from crafted Super Alloy) |
| **Pulsar Quartz**  | Pulsar Quartz Caves, Meteor Field; purple massive-meteor event (unlocks 8 GTi). Also craftable. | Fusion Energy Cell; top sellable item for Terra Tokens |

**Moons-update materials:** **Tungsten** (Toxicity moon -> Tungsten Rod), **Phosphorus**
(Aqualis ocean moon -> rockets/fertilizer), **Cosmic Quartz** (endgame tech).

### Refined "Rods" (intermediate for high-tier machines, Advanced Craft Station)
- **Iridium Rod** = 9 Iridium
- **Osmium Rod** = 9 Osmium (unlocks 39 GTi)
- **Uranium Rod** = 9 Uranium
- **Super Alloy Rod** = 8 Super Alloy + 1 Aluminium (unlocks 750 MTi)
- **Tungsten Rod** = 8 Tungsten + 1 Toxins (Toxicity moon)

### Ice and Water
- **Early game:** Ice chunks scattered on the surface are hand-collected. **Water Bottle** is crafted at a Craft Station from Ice (~2 Ice -> 1 Bottle). Only way to refill thirst early.
- **Mid game:** as terraforming progresses (Rain 875k -> Liquid Water 3M -> Lakes 50M), water appears in the world. **Lake Water Collector** (50 MTi) sits on water, holds up to 8 bottles, produces ~1 bottle / 100-150 s. A water filter tech lets the player drink from any source.
- Ice remains a crafting input after collectors come online.

---

## 6. Machines and Buildings

> Per-second values are base output before seed multipliers, Machine Optimizer chips, and the
> orbital rocket boosters that multiply terraforming machines globally. Numbers vary by patch.

### 6a. Heaters (raise Heat)

| Machine     | Heat output | Pressure side | Power   | Build cost (representative)                  | Unlock                |
|-------------|-------------|---------------|---------|----------------------------------------------|-----------------------|
| Heater T1   | 0.3 pK/s    | -             | 1.00 kW | 1 Iron, 1 Iridium, 1 Silicon                 | early (after Iridium) |
| Heater T2   | 4.5 pK/s    | -             | 3.50 kW | + Titanium, Aluminium                        | Heat progression      |
| Heater T3   | 28.5 pK/s   | 0.60 nPa/s    | 17.50 kW| refined materials                            | Heat progression      |
| Heater T4   | 538 pK/s    | 35.5 nPa/s    | 51.50 kW| Super Alloy class                            | Oxygen ~63 ppb        |

### 6b. Drills & Gas Extractors (raise Pressure)

| Machine        | Pressure    | Heat side  | Power    | Build cost (representative)        | Unlock                  |
|----------------|-------------|------------|----------|------------------------------------|-------------------------|
| Drill T1       | 0.20 nPa/s  | -          | 0.50 kW  | 1 Iron, 1 Titanium                 | Construction Microchip  |
| Drill T2       | 1.50 nPa/s  | 0.10 pK/s  | 5.00 kW  | 1 Iron, 2 Titanium                 | Pressure 1.2 uPa        |
| Drill T3       | 17.00 nPa/s | 0.25 pK/s  | 8.50 kW  | 2 Iron, 2 Titanium, 2 Aluminium    | Heat 21 nK              |
| Drill T4       | 459 nPa/s   | minor      | 45.50 kW | 6 Super Alloy, 3 Osmium            | Heat 41 uK              |
| Gas Extractor  | Pressure + harvests gases | yes | moderate | refined materials             | mid-game Pressure       |

The **Magnetic Field Protection Rocket** multiplies all Drill pressure output globally
(exponentially), making late drills far more potent than base numbers.

### 6c. Oxygen machines (raise Oxygen; also produce Plant biomass)

- **Vegetubes T1/T2/T3** - enclosed planters that accept a flower seed; output scales with the seed's oxygen multiplier. T1 ~0.15 ppq/s, 0.35 kW (1 Iron, 1 Ice, 1 Magnesium). T2 needs Heat ~500 pK; T3 needs Oxygen ~30 ppt (outdoor only).
- **Algae Generators T1/T2** - outdoor only; a big step up from Vegetubes; also grow algae/mushrooms (food + crafting byproduct).
- **Tree Spreaders T1/T2/T3** - dominant late-game Oxygen + Plant-biomass machines. Accept a tree seed (>=125% O2 multiplier). T1 ~920 O2/s (built in water), T2 ~1,950 O2/s, **T3 ~12,500 O2/s + ~680 Biomass/s, 193 kW** (the single strongest Ti-raising machine, ~9,750 Ti/s base). Zero output without both power and a seed.
- **Grass & Flower Spreaders** - generate ground cover in a radius, raising Plant biomass + some oxygen; feed the Moss/Flora stages.

### 6d. Biomass: insect & animal machines (seeded with larvae/eggs)

| Machine                  | Raises            | Notes                            |
|--------------------------|-------------------|----------------------------------|
| Beehive T1/T2            | Insects + Plants  | Honey; T2 also Bee Larva         |
| Butterfly Dome / Farm    | Insects           | Grows butterflies from larvae    |
| Aquarium / Fish Farm     | Animals           | Grows fish (Fish stage)          |
| Amphibian Farm           | Animals           | Grows frogs (Amphibians stage)   |
| Outdoor Farm             | Insects / Plants  | Open-air plot                    |
| Incubator                | production        | Makes larvae, fish/frog eggs     |

### 6e. Crafting / fabrication / genetics stations

| Station                  | What it does                                                            |
|--------------------------|-------------------------------------------------------------------------|
| Craft Station T1/T2      | Basic fabrication: components, machine parts, microchips, water bottles |
| Advanced Craft Station   | Top-tier components (Super Alloy class, rods, advanced chips). Gate at 175 kTi |
| Ore Extractor T1/T2/T3   | Auto-mines ore over time (see below)                                    |
| Biolab                   | Bacteria samples, fertilizer, bioplastic, mutagen, food/chemical recipes |
| DNA Manipulator          | Synthesizes plant/tree seeds (700 MTi)                                   |
| Genetic Extractor / Synthesizer | Extracts/combines genetic traits into Creature DNA for mammals    |
| Food Grower T1/T2        | Grows food crops for survival hunger                                    |
| Cooking Station          | Buffed cooked foods                                                     |
| Recycling Machine        | Breaks items back into resources                                        |
| Launch Platform          | Launches terraforming rockets (global multipliers)                      |

### 6f. Power (global, wireless)

Power is **global and connectionless** - no wires. The game sums all generation and subtracts
all running-machine draw; if draw exceeds supply, machines stop.

| Generator                  | Output     | Notes                                  |
|----------------------------|------------|----------------------------------------|
| Wind Turbine               | 1.20 kW    | Earliest, weak                         |
| Solar Panel T1 / T2        | 6.5 / 19.5 kW | Light-dependent                     |
| Nuclear Reactor T1 / T2    | 86.5 / 331.5 kW | Consumes uranium rods             |
| Nuclear Fusion Generator   | ~1,835 kW  | Uses Fusion Energy Cells; only one buildable indoors |

Power is the recurring bottleneck: every machine tier multiplies both output and draw, forcing
repeated grid upgrades (Wind -> Solar -> Nuclear -> Fusion).

### 6g. The Ore Extractor (deep-mining analogue)

Auto-mines ore over time into its own inventory until full. Placement: only on **sand or dirt**
(not bare rock or buildings). Output depends on **biome** + **tier**:

- **Default output** (no specific deposit): ~33% Iron, ~16.6% each Cobalt/Magnesium/Silicon/Titanium.
- **On a target deposit**: the target dominates (~25%+), basics as remainder.

| Tier | Unlock (Pressure) | Mines                                          | Behavior                                  |
|------|-------------------|------------------------------------------------|-------------------------------------------|
| T1   | ~155 uPa          | Basic ores (+ Aluminium, Sulfur, Iridium)      | Random output, lots of byproduct          |
| T2   | ~364.5 mPa        | + Uranium, Super Alloy, Osmium, Zeolite, Obsidian | Less byproduct, larger inventory       |
| T3   | ~13-38 Pa         | Same access as T2                              | **Select one specific ore, zero byproduct**; small inventory |

You bias output by **placing in the right biome** (T1/T2) or by **directly selecting the ore**
(T3 only). Canonical endgame: a T3 extractor on each rare-ore cave feeding an Auto-Crafter to
mass-produce rods.

---

## 7. Technology / Unlock System

There is no points-based skill tree. Three parallel systems unlock content:

### A. Ti auto-unlocks
Crossing a Ti threshold (or a specific lane milestone) **automatically** makes new buildings/recipes
appear in the build menu. Examples: Advanced Craft Station 175 kTi; Super Alloy Rod 750 MTi; DNA
Manipulator 700 MTi; Drone Station 62.5 GTi; Osmium Rod 39 GTi; Ore Extractor tiers by Pressure.

### B. Blueprint Microchips (manual, Ti-independent)
For content terraforming alone never grants (vehicles, equipment, decorations, some buildings):
- **Found** in containers worldwide (shipwrecks, bunkers, buried caches). Small square boxes almost always hold one.
- **Decoded** at a **Blueprints Screen** (1 Iron + 1 Silicon): each decode consumes one chip and unlocks one item.
- **Tiered & random**: within a tier the unlock order is random; you must clear a whole tier before the next opens.
- Distinct from equipable **Multi-Tool Microchips** (crafted) that give perks (faster build/mine, map markers, torch).

### C. Genetic pipeline (life unlocks)
Biology is unlocked by assembling DNA: **Genetic Extractor** (extract traits) -> **Genetic
Synthesizer** (combine 3 mandatory + up to 5 optional traits into Creature DNA) -> **Animal
Shelter** (spawns the animal, each DNA +1000% to output, max 5). The **DNA Manipulator** makes
plant/tree seeds. The **Animal Feeder** keeps mammals fed (unfed animals stop contributing biomass).

---

## 8. Spreading Life (seeds, larvae, eggs)

Life is its own Ti contributor via Biomass. The loop: collect/craft a biological seed/larva/egg,
place it in the matching building; it generates biomass at a rate scaled by the species multiplier.

- **Plants:** **Flower seeds** found in crates, each with an oxygen multiplier (Lirma 100% up to
  Golden Seed 600%) -> Vegetubes (oxygen) / Grass-Flower Spreaders (plant biomass). **Tree seeds**
  crafted in the DNA Manipulator -> Tree Spreaders (biggest plant-biomass producers). Mushroom
  seeds -> Food Growers. Grass spreads automatically from a seedless Flower Spreader.
- **Insects:** the Insects stage (~8 GTi) spawns wild larvae (Common/Uncommon/Rare). The
  **Incubator** crafts butterflies, bee larvae, and silkworms from larvae + Mutagen + Fertilizer.
  Butterflies -> Butterfly Dome/Farm; bees -> Beehives. Supporting recipes: **Mutagen** and
  **Fertilizer** (Biolab).
- **Animals:** progression Fish -> Amphibians -> Mammals. Collect **Phytoplankton**/eggs with a
  **Water Life Collector** -> Incubator makes species eggs -> Aquarium/Fish Farm (fish) or
  Amphibian Farm (frogs). Mammals use the genetic pipeline -> Animal Shelter.

### Rockets (terraforming accelerators)
Launched from the Launch Platform; each gives a permanent stacking global multiplier to its
machine type: Plants, Seeds Spreader, Insect Spreader, Animals Spreader, plus the auto-unlocked
**Asteroid Attraction** and **Magnetic Field Protection** rockets that trigger ore meteor showers.

---

## 9. Key Takeaways for Adaptation

These are the load-bearing mechanics any faithful adaptation should preserve:

1. **One global score (Ti) = sum of independent lanes** (Oxygen, Heat, Pressure, Biomass), each
   with tiny units and a per-second accumulation rate. The HUD shows total + per-lane rates.
2. **Fixed staged transformation** gated by Ti thresholds: atmosphere (sky/clouds/rain) -> water
   (wet ground/lakes) -> flora (moss/grass/trees) -> fauna (insects/fish/frogs/mammals) -> done.
   The world visibly changes at each stage.
3. **Positive feedback**: terraforming progress unlocks the tech/machine tiers that terraform faster.
4. **Machine tiers scale exponentially** (T1 -> T4 is ~1,000-2,000x), so the endgame is a few
   top-tier machines on a heavy power grid, not a sprawl of weak ones.
5. **Power is global and the recurring bottleneck**, forcing repeated grid upgrades.
6. **Survival gates the early game**: oxygen (suit/tanks), water (ice -> bottles -> collectors),
   food (growers). Breathable atmosphere (~32 GTi) removes the oxygen worry.
7. **Auto-mining (Ore Extractor)** with biome/tier-based ore bias and selectable ore at the top tier.
8. **Three unlock channels**: Ti thresholds (auto), Blueprint Microchips (found + decoded),
   genetic pipeline (life). Plus rockets as permanent global multipliers.
