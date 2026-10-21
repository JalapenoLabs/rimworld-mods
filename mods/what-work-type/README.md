# What Work Type Is This?

A RimWorld mod that adds a **Work type** row to every building's info card, so
you know which Work tab column to raise to get the building used.

Open the info card (the "i" button) on a long-range mineral scanner and it reads
**Work type: Research**. A stove reads **Cook**; a fueled smithy reads
**Smith, Haul** (crafting plus refueling). Hover the row to see each job, the
skill it uses, and where it ranks among the other jobs in its work type, since
that rank decides which job a colonist picks first at equal priority.

Requires **Harmony**. Safe to add or remove mid-save.

## How it works

Nothing is hand-mapped. Every job in RimWorld is a `WorkGiverDef` that belongs to
one work type and declares what it works on. A building is listed under a job
when either:

- the job lists it as a workbench (`fixedBillGiverDefs`), or
- the job's scanner searches for that exact building, or for a group the building
  belongs to (`PotentialWorkThingRequest`).

Because this reads the declarations every job already makes, buildings and jobs
from other mods appear automatically. One Harmony postfix on
`ThingDef.SpecialDisplayStats` adds the row; it covers both the architect-menu
card and the placed-building card.

### What is deliberately left out

Some request groups match nearly every building, so they say nothing about what a
building is for. Jobs found only through these groups are not shown:

| Group | Vanilla jobs it carries |
| --- | --- |
| `BuildingArtificial` | Repair, fix broken down building |
| `PotentialBillGiver` | Bill jobs with several workbenches (matched precisely via their workbench list instead) |
| `Everything`, `HaulableEver`, `HaulableAlways`, `HaulableEverOrMinifiable` | General hauling |

Research asks for no targets until a project is selected, so it is mapped to the
research bench group it uses while active. A job whose targets are only known at
runtime (its request is `Undefined`) cannot be matched from defs alone and is not
shown.

## Building

From the monorepo root:

```shell
mage build what-work-type
```

The compiled assembly is written to `1.6/Assemblies/WhatWorkType.dll`.
