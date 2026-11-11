# Terraformation

A RimWorld mod, in development, about landing on a dead, airless, lethally cold red
planet and terraforming it back to a living world. Inspired by The Planet Crafter:
you build and power terraforming machines to raise a global Terraformation Index
across four lanes (Oxygen, Heat, Pressure, and Biomass), and the world changes as
the index climbs through fifteen phases.

Requires the **Odyssey** DLC.

## Status

The mod is a skeleton today: its About metadata, the project file, and the `1.6`
folder layout. It adds no gameplay content yet.

- [PLAN.md](PLAN.md) is the full design: the score model, the fifteen phases, the
  machines, the atmosphere and survival system, and the Gaia storyteller.
- [ART.md](ART.md) is the art direction.
- [scripts/](scripts/README.md) holds the placeholder-art helper used until final
  art is made.

## Building

From the monorepo root:

```shell
mage build planet-terraforming
```

The compiled assembly is written to `1.6/Assemblies/Terraformation.dll`.
