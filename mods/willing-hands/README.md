# Willing Hands

A RimWorld mod where well-treated slaves may choose to become full colonists,
the mirror of a slave rebellion. Where rebellion builds on neglect, belonging
builds on good treatment.

Requires the **Ideology** DLC.

## How it works

Keep a slave in high **comfort** and good **mood** over an extended period and
they build a quiet, hidden sense of belonging. Once settled, each day carries a
small chance they send a letter asking to become a colonist.

- **Accept** and they join freely, even an unwaveringly loyal slave, because a
  willing choice is not recruitment. They arrive with a "chose to belong" mood
  boost.
- **Refuse** and they stay enslaved, with a long pause before they might ask
  again.

Let their treatment slip and the streak erodes, twice as fast as it built. The
road to a rebellion and the road to a new colonist run through the same care.

## Defaults

| Setting | Value |
| --- | --- |
| Comfort threshold | 65% |
| Mood threshold | 85% |
| Sustained contentment required | 15 days |
| Ask chance once settled | 5% per day |
| Pause after refusal | 10 days |

## Design notes

No Harmony patches. A `MapComponent` samples spawned slaves on a slow cadence
and holds the contentment streak off-pawn, so nothing telegraphs the coming
request. Conversion goes through `RecruitUtility.Recruit`, which clears guest
status and never checks the "unwaveringly loyal" flag. The request uses the
same `ChoiceLetter` machinery as vanilla's "a pawn wants to join" letters.

## Building

From the monorepo root:

```shell
mage build willing-hands
```

The compiled assembly is written to `1.6/Assemblies/WillingHands.dll`.
