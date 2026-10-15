**Source-of-truth policy.** The USER is the primary source of truth. This file is the SECOND
source of truth for Gravship Wars. The codebase is **not** a source of truth. Whenever a design
decision is made or changed, update this file in the same change.

This file documents the current design and the roadmap. It does not document history.

# Gravship Wars

A RimWorld 1.6 mod that adds ship-to-ship combat to the Odyssey gravship.

Odyssey gave the gravship flight, vacuum, and orbit. It did not give it a war. This mod arms the
gravship, gives it a heat economy to manage while fighting, gives the crew a way to survive losing,
and puts hostile ships in orbit to fight.

## Dependencies

Hard requirements, and nothing else:

- RimWorld base game
- **Odyssey** DLC (`Ludeon.RimWorld.Odyssey`)
- **Harmony** (`brrainz.harmony`)

Explicitly **not** required: Vehicle Framework, Save Our Ship 2, or any other content mod. Every
system here is built on vanilla and Odyssey seams. If a feature cannot be built without a third
dependency, redesign the feature.

## Relationship to Save Our Ship 2

SOS2 (workshop `1909914131`, packageId `kentington.saveourship2`) is the inspiration and the
reference point, not the blueprint. See `notes.md` for its metadata and local path.

**Learn from what it did well:**

- Ship combat as a *positional* problem. Where you put a weapon on the hull determines what it can
  cover. Layout is a real decision.
- Heat as the limiter on energy weapons. Firing lasers is not free; a ship that overcommits cooks
  its own crew.
- Boarding as the resolution. The most memorable SOS2 fights ended with pawns fighting pawns, not
  with a health bar hitting zero.
- Weapons that need people. A crewed ship feels crewed.

**Learn from what it did poorly:**

- **Crew starvation.** SOS2 wanted a gunner on nearly every weapon. A 7-pawn colony cannot staff a
  warship. Combat degenerated into a staffing puzzle before it became a tactical one.
- **Enemy ships as bullet sponges.** Hostile ships were mostly static. They rarely maneuvered,
  rarely made choices, and the fight was decided by whether your hull out-teched theirs.
- **A parallel game.** SOS2 rebuilt so much of RimWorld that its systems stopped composing with the
  base game. Prefer vanilla seams so that mood, health, work priorities, and raids keep working.
- **Opaque numbers.** Dodge, accuracy, and heat were governed by formulas the player could not see
  or predict.
- **Static ships in the boarding venue.** Its shared space map put your ship in one corner and the
  boarder in the other, both parked. Ships that cannot move read as rigid and unreal.
- **A ranged combat mode that fought the player.** Its side-by-side ranged battle demanded constant
  swapping between your ship's view and the enemy's, which made a good idea tiring to operate.

The last two are answered directly by the two-layer design in **Ship-to-ship combat**.

**The rule:** this mod is inspired by SOS2 and shares none of its code, assets, or defs. Everything
here is written from scratch against Odyssey.

## Odyssey systems this mod builds on

Verified against `RimWorld/Data/Odyssey/Defs` and `RimWorld/Data/Core/Defs`.

| System | Def / class | Why it matters |
|--------|-------------|----------------|
| Orbit layer | `PlanetLayerDef` `Orbit`, `isSpace: true` | Where combat happens. Whitelists incidents, conditions, and arrivals. |
| Space maps | `WorldObjectDef` `Space`, `worldObjectClass: SpaceMapParent`, `mapGenerator: Space` | A generated map in orbit that a gravship can fly to. The venue for ship-to-ship fights. |
| Space biome | `BiomeDef` `Space`, `inVacuum: true`, `constantOutdoorTemperature: -75` | Vacuum and cold are already modeled. Do not reinvent them. |
| Gravship in flight | `WorldObjectDef` `Gravship`, `worldObjectClass: Gravship`, `IncidentTargetTags: [Gravship]` | The player's ship is already an incident target while in transit. Interception events hook here. |
| Gravship attachment | `CompProperties_GravshipFacility` (`requiresLOS`, `maxSimultaneous`, `maxDistance`) | The vanilla way to say "this building belongs to a gravship and there is a limit on how many." |
| Gravship-only placement | `terrainAffordanceNeeded: Substructure` | The clean gate for "buildable on a ship, not on dirt." |
| Vanilla shield | `GravshipShieldGenerator` | 200W, 500 shield HP, radius 24.9, max 2 per ship, 10000-tick charge for a 6000-tick active window. A charge-and-burn timer, not a sustained defense. |
| Pilot seat | `PilotConsole`, `PilotSubpersonaCore` | Established pattern: a manned console, and a late-game AI core that replaces the pawn. |
| Vacuum tolerance | `StatDef` `VacuumResistance`, `OxygenPump`, `VacBarrier` | Hull breaches are already lethal and already have counterplay. |
| Mortar reference | `Turret_Mortar` / `Artillery_Mortar` | `CompProperties_ChangeableProjectile` (one shell at a time), `forcedMissRadius: 9`, `minRange: 29.9`, `range: 500`, `warmupTime: 4.0`, shells drawn from the `MortarShells` category. |

## Architecture

Follow the repo-standard order of thought: XML first, then comps and worker classes, then a vanilla
subclass, and Harmony only as the last mile.

- Weapons are `ThingDef`s with `thingClass` set to a `Building_TurretGun` subclass where needed.
- Ship membership is `CompProperties_GravshipFacility`, not a custom registry.
- Per-ship shared state (heat, in particular) lives in a `MapComponent`, keyed to the grav engine.
- Firing rules (space-only, ground-allowed, heat cost) are a `DefModExtension` read by shared code,
  never a hardcoded def-name check.
- Layout: `About/`, `1.6/{Assemblies,Defs,Source,Languages}`, `mod.csproj` at the mod root with
  `EnableDefaultCompileItems=false`, `net480`, `OutputPath=1.6/Assemblies`.

## Heat

Heat is the mod's central resource and its main departure from SOS2.

**Model.** One heat pool per gravship, held by a `MapComponent` and attributed to the grav engine.
Energy weapons and active shields add heat. Radiator buildings drain it. The pool is visible as a
gizmo on the engine and on every heat-producing building.

**Overflow is the failure mode.** When the pool is full, further heat spills into normal room
temperature via the vanilla heat system. The ship does not explode; the crew starts cooking. That
keeps the failure legible, keeps vanilla coolers and vents relevant inside the ship, and produces
the RimWorld-shaped outcome where a bad fight becomes a heatstroke crisis.

**Why not room temperature alone.** A vacuum does not conduct heat, so venting a laser bank
"outdoors" at -75C would be free and wrong. Radiators are the only way to shed heat while in space.

**Shields.** Gravship Wars adds its own shield tier that trades vanilla's timer for heat: it stays
up as long as you can pay for it in heat, and absorbed damage converts to heat directly. A ship that
tanks everything overheats and has to drop its shield. That is the sustained defense.

Vanilla's `GravshipShieldGenerator` stays useful and stays untouched. Because enemy fire arrives as
real projectiles on the ship map (see **Ship-to-ship combat**), its `CompProjectileInterceptor`
catches ship-to-ship rounds with no patch at all. Its 100-second window on a 4-hour charge is the
wrong shape for a sustained defense and exactly the right shape for a panic button: eat one heavy
volley, then live with the cooldown.

## Weapons

All weapon buildings require `terrainAffordanceNeeded: Substructure` and a
`CompProperties_GravshipFacility` link to the grav engine. A shared `DefModExtension` carries
`firesInSpace`, `firesOnGround`, `heatPerShot`, and `magazineCapacity` so the rules stay data-driven.

Default: space only, inert on the ground. The manned turret family is the exception.

### Who operates a weapon

**The gunnery console operates everything except the manned turrets.** One pawn at the console
directs the directional cannons, laser cannons, beam lasers, missile launchers, and auto turrets.
No weapon in that group needs a body standing next to it. This is the answer to SOS2's crew
starvation and it collapses a staffing puzzle into one job worth doing well.

**Manned turrets keep their gunner.** A 360-degree arc is the strongest positional advantage on the
ship, and putting a pawn inside is what it costs. That pawn is exposed, occupied, and lost if the
emplacement is breached. The trade is legible and it is the balance.

Shell-fed weapons still need someone to reload them. That is hauling work, not gunnery, and it keeps
the rest of the colony in the fight without demanding dedicated crew.

### Directional cannon

- Fires existing vanilla mortar shells in the direction it faces.
- Magazine of 3 shells, one shell type at a time. Changing type requires unloading.
- Console-operated. Needs a colonist to reload, and reloading is short.
- No heat.

### Laser cannon

- Fires discrete bolts. High energy cost per shot.
- Console-operated. No reloading.
- Adds heat.

### Beam laser

- Sweeps a continuous beam for 3 seconds per attack, then a 2-second cooldown. High sustained energy
  draw while firing.
- Console-operated.
- Adds heat, and more of it than the laser cannon.

### Gravship manned turret

- A 2x2 walkable emplacement (`passability: PassThroughOnly`, as the vanilla shield generator does)
  that shows the gunner inside while in use.
- Fires in any direction, 360 degrees, 2 shells per volley.
- Has a minimum range. It cannot defend itself up close.
- Shoots over gravship walls. It does not shoot over walls that are not part of the gravship.
- Visually mortar-like: shells arc across the map.
- Mechanically better than a mortar: roughly 50% tighter, so `forcedMissRadius` near 4.5 against the
  mortar's 9, with a faster shell.
- **Works on the ground.** This is the one weapon that earns its keep between flights.
- Needs a colonist to reload.

### Gravship manned laser turret

Same emplacement and arc as the manned turret, with the laser cannon's economics: no reloading,
high energy per shot, adds heat.

### Gravship auto turret

The manned turret without a gunner, console-operated like the rest. Longer target acquisition and
slower traverse are the price of not spending a pawn.

### Missile launcher

- A 1x2 emplacement that can sit anywhere on the ship, including the interior.
- Reaches anywhere on the map from any direction, like a mortar.
- Roughly 60% tighter than a mortar, so `forcedMissRadius` near 3.6, with faster and deadlier
  missiles.
- Console-operated. Consumes missile ammunition, not shells, and needs a colonist to reload.

### Gunnery console

The single crewed station for every weapon above except the manned turrets. One pawn, one job.
Mirrors Odyssey's `PilotConsole`. Gunner Shooting skill drives accuracy across every weapon it
directs, which makes a dedicated gunner worth training rather than worth cloning.

A gunnery subpersona core is the late-game automation tier, mirroring `PilotSubpersonaCore`: it
frees the pawn at a cost in accuracy or reaction time.

## Ammunition

Three tiers, all craftable, all using vanilla category plumbing where possible.

1. **Vanilla mortar shells.** Reused as-is by the directional cannon and the manned turret.
2. **Ion shells.** A tier-2 parallel of every vanilla shell type: ion explosive, ion incendiary, ion
   EMP, ion tox, and so on. Research-gated and built from gravlite-tier materials so they read as
   Odyssey-native rather than bolted on.
3. **Missiles.** A parallel line covering the same payload types, consumed only by the missile
   launcher.

## Escape pods

When a fight is lost, the crew needs an out that is not "reload the save."

Built on vanilla transport-pod machinery (`CompTransporter` / `CompLaunchable`) rather than a new
launch system. A pod is single-use, needs no fuel, and targets the nearest habitable surface tile
below. Launching is available manually, and prompted when the ship is critically damaged.

## Ship-to-ship combat

Ships must move. A ship parked in the corner of a shared map is the thing that made SOS2's boarding
fights feel rigid, and a fullscreen mode that constantly swaps between your ship and theirs is the
thing that made its ranged fights hard to use. This design avoids both.

### The engine constraint that decides the shape

RimWorld cells are integer `IntVec3`. Buildings have four rotations. Pathfinding, regions, rooms,
temperature, and the roof grid are all cell-indexed. A ship grid translated or rotated to an
arbitrary angle cannot host pawn pathfinding or hold atmosphere, so the interior map can never be
the thing that moves.

The resolution is to stop asking it to. Maneuver and interior are two layers.

### Layer 1: the ship interior, a normal RimWorld map

Never moves, never rotates. Pawns walk, man turrets, haul shells, fight fires, suffocate, and bleed.
Everything RimWorld is already good at keeps working untouched.

### Layer 2: the tactical layer, a custom view

Both ships are objects with position, facing, and velocity on a free continuous plane. They move and
rotate at any angle because nothing on this layer needs cells: no pathfinding, no rooms, no
temperature. Rendering and simulation are entirely the mod's own.

Free rotation here is what makes weapon placement matter. Mounts on the interior map are fixed, so
the ship's facing on the tactical layer determines which weapons can bear. Turning to bring the port
battery around is a real decision with a real cost, and hull layout becomes the positional problem
SOS2 got right.

**The tactical layer is an overlay, not a mode.** It lives pinned in the corner of the ship view
showing relative bearing, range, and facing. The player stays on their ship. Expand it only to give
piloting orders. Nobody is forced to swap views to fight, which is the specific failure being
designed out.

### The bridge between layers

This is the core mechanic and the reason the design works.

**Incoming fire.** The tactical layer resolves whether a shot hits, using range, relative velocity,
facing, and gunner skill. On a hit it spawns a real projectile on the player's ship map, arriving
from the enemy's true bearing and aimed at a real cell. From there vanilla takes over completely:
shield generators intercept it, hulls breach, Odyssey's vacuum vents the room, fires start, pawns
get hurt, and the colony has a crisis.

**Outgoing fire.** The gunnery console targets the enemy by bearing and subsystem. The tactical
layer resolves the shot and degrades an abstract enemy subsystem: engines, weapons, shields, or
hull. The enemy needs no interior map until docking happens, which is a large implementation saving.

**No ship HP bar, on either side.** Player ship health is emergent from how wrecked the actual rooms
are. That is a strictly better answer than SOS2's bullet sponges, and it costs nothing to build
because the damage is already real.

### Docking

Docking is the deliberate, dramatic beat where the two layers collapse into one.

Reaching a docking solution requires closing to contact and matching facing, then landing a docking
clamp or winch claw. On success the game builds a single combat map: the player's ship stamped on
one side, the enemy's on the other, joined at the docking corridor. Odyssey already has the
machinery to place a gravship's things and terrain onto a map, so this reuses the launch and land
path rather than inventing one.

From that point it is an ordinary RimWorld map, and boarding needs no separate system at all. Pawns
in vacsuits walk out through the corridor or across the hull; vacuum, `VacuumResistance`, and
`OxygenPump` already govern who survives outside. Breaking the clamps ends the docked state, tears
the joined map down, and returns both ships to the tactical layer.

Because docking produces one real merged map, there is no view to swap and nothing to teleport
pawns between. EVA is only available while docked. Spacewalking between two ships maneuvering at
speed is a large amount of work for very little play, and cutting it removes the teleport problem
entirely.

### How fights start

- Fly to a visible hostile ship in orbit and engage it.
- Fly to empty space and get intercepted. The `Gravship` world object already carries the `Gravship`
  incident target tag, so an `IncidentWorker` can hook it directly.

Battle state lives on a `GameComponent` so it survives the player moving between maps, and is
persisted with Scribe.

### The open problem

Enemy behavior decides whether this mod is fun. A ship that holds station and trades fire is a
shooting gallery no matter how good the weapons are. The enemy must make decisions the player can
read from the tactical overlay: close or open the range, turn to protect a damaged flank, drop
shields to vent heat, break off, or commit to docking. Solve this before tuning weapon numbers.

**Scope warning.** The tactical layer is the single largest piece of this mod. Its rendering,
movement simulation, targeting, damage mapping, and save format together outweigh every weapon,
shell, and shield combined. Plan for it accordingly.

## Roadmap

1. Heat pool, radiators, and the heat-driven shield. Everything else depends on the heat economy
   existing first.
2. Weapon buildings, the gunnery console, and their placement and firing rules.
3. Ion shells and missiles.
4. The tactical layer: movement, facing, rendering, and the incoming-fire bridge onto the ship map.
   Prove the bridge with a scripted enemy before building real AI.
5. Enemy ship generation and combat AI.
6. Docking, the merged combat map, and boarding.
7. Interception incident and orbital hostile contacts.
8. Escape pods.
9. Localization for all supported languages via `/translate-mod`.

## Open decisions

- **Research tree shape.** Whether weapons gate behind one "gravship weaponry" project or a branch
  per weapon family.
- **Enemy ship AI depth.** See **Ship-to-ship combat**.
- **Tactical layer time scale.** Whether maneuvering runs on normal game speed or its own clock, and
  what pausing does to it.
