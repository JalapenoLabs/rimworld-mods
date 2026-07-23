# Sky Islands: Starjack Patches

A patch mod that treats a **Sky Islands** map as the middle ground it is for
space-adapted pawns, rather than the harshest "planetside" case.

Requires the **Odyssey** DLC and the **Sky Islands** mod.

## Why

Sky islands float at high altitude on a child-of-`OrbitLayer` planet layer that
the Sky Islands mod deliberately marks `isSpace = false` so the maps stay
habitable (atmosphere, plants, no vacuum). A side effect is that Odyssey's
space-adaptation mechanics read a sky island as full "planetside" and apply
their harshest penalties there, which feels wrong for a floating island 25km up.

## What it changes (only while on a sky island)

| Mechanic | Vanilla planetside | On a sky island |
| --- | --- | --- |
| Shipborn "space habitat" mood | -2 ("Planetary habitat") | neutral (no thought) |
| "Low gravity adapted" gene | -0.2 move, -0.1 work | -0.1 move, -0.05 work (halved) |

Everywhere else, vanilla behavior is untouched. Both effects are environment
based, so they apply to any pawn with these traits on a sky island, not only
starjacks.

## How it works

No Harmony. The shipborn mood swaps the thought's `workerClass` for a subclass
that returns "no thought" on the sky-island layer. The movement penalty is
softened by a small custom `ConditionalStatAffecter` added to the gene that
contributes counteracting positive offsets only on a sky island. Sky-island
detection matches the layer defName the Sky Islands mod uses
(`Avatar_SkyIslandsLayer`).

## Building

```shell
mage build sky-islands-starjack-patches
```
