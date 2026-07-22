# Break Loyalty

A RimWorld mod that turns "unwaveringly loyal" from a permanent wall into a
low-odds gamble. It adds the **Break Loyalty** ceremony, a ritual that can
shatter a prisoner's loyalty to their faction and make them recruitable.

Requires the **Ideology** DLC.

## How it works

Start the ceremony from the rituals menu and assign a **speaker** and an
unwaveringly loyal **prisoner**. The speaker escorts the prisoner before an
audience and makes the case. When it ends, one roll decides the outcome.

The success chance is the ritual's quality, and it is **capped at 25%**. It
climbs toward that ceiling with:

| Factor | Effect |
| --- | --- |
| Speaker's social impact | Up to +8% |
| Spectators (up to 4 count) | Up to +5% |
| Faction disillusionment | Lower goodwill between the prisoner's faction and you raises the odds, up to +6% |
| Shared ideoligion | +5% if the prisoner already follows your colony's ideoligion |
| Bonds with colonists | Friends, lovers, and family among your colonists, up to +5% |
| Prisoner's contentment | A well-treated prisoner is more persuadable, up to +4% |

Base chance is 5%.

- **Cooldown:** five days, colony-wide. One attempt at a time, so choose the
  moment.
- **Failure:** the prisoner gains a stacking -10 mood memory (15 days, up to
  five stacks). A later success clears all of it.
- **Success:** the prisoner's unwavering loyalty is gone; recruit them as
  normal.

## Design notes

The ceremony is built entirely on the vanilla Ideology ritual framework, no
Harmony patches. It is a `classic`, invisible precept, so it is present in
every colony regardless of chosen memes, the same mechanism vanilla uses for
Conversion and public executions. The cooldown is a native `AbilityGroupDef`.
Breaking loyalty sets `Pawn_GuestTracker.Recruitable`, whose setter writes the
backing field directly.

## Building

From the monorepo root:

```shell
mage build break-loyalty
```

The compiled assembly is written to `1.6/Assemblies/BreakLoyalty.dll`.
