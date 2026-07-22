/*************************************************************************
 * Break Loyalty — ritual outcome worker.
 *
 * Resolves the ceremony. Ritual quality, summed from the outcome comps and
 * hard-capped by the def's maxQuality (0.25), IS the success probability. A
 * flawlessly run ceremony on a disillusioned, well-treated prisoner reaches
 * the design ceiling; a rushed one on a devoted loyalist barely moves off the
 * base.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace BreakLoyalty;

public class RitualOutcomeEffectWorker_BreakLoyalty : RitualOutcomeEffectWorker_FromQuality {
    public RitualOutcomeEffectWorker_BreakLoyalty() { }

    public RitualOutcomeEffectWorker_BreakLoyalty(RitualOutcomeEffectDef def) : base(def) { }

    // A single stochastic roll decides the outcome, so the weighted
    // outcome-chance table the base class would apply does not fit here.
    public override bool SupportsAttachableOutcomeEffect => false;

    public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual) {
        Pawn subject = jobRitual.PawnWithRole("subject");
        float chance = Mathf.Clamp01(GetQuality(jobRitual, progress));
        bool success = subject != null && Rand.Chance(chance);

        if (subject != null) {
            MemoryThoughtHandler memories = subject.needs?.mood?.thoughts?.memories;
            if (success) {
                BreakLoyaltyUtility.MakeRecruitable(subject);
                // A win wipes the resentment left by earlier failed attempts.
                memories?.RemoveMemoriesOfDef(BreakLoyaltyDefOf.BreakLoyalty_FailedCeremony);
            } else {
                memories?.TryGainMemory(BreakLoyaltyDefOf.BreakLoyalty_FailedCeremony);
            }
        }

        RitualOutcomePossibility outcome = OutcomeFor(success);
        string text = outcome.description.Formatted(jobRitual.Ritual.Label).CapitalizeFirst();
        text += "\n\n" + OutcomeQualityBreakdownDesc(chance, progress, jobRitual);

        LookTargets lookAt = subject != null ? (LookTargets)subject : jobRitual.selectedTarget;
        Find.LetterStack.ReceiveLetter(
            "OutcomeLetterLabel".Translate(outcome.label.Named("OUTCOMELABEL"), jobRitual.Ritual.Label.Named("RITUALLABEL")),
            text,
            success ? LetterDefOf.RitualOutcomePositive : LetterDefOf.RitualOutcomeNegative,
            lookAt);
    }

    /// Picks the narration matching the roll. Positive/negative entries are
    /// declared in the def's outcomeChances; the chance values there are unused
    /// because the real probability is the ritual quality computed above.
    private RitualOutcomePossibility OutcomeFor(bool success) {
        foreach (RitualOutcomePossibility possibility in def.outcomeChances) {
            if (possibility.Positive == success) return possibility;
        }
        return def.outcomeChances[success ? def.outcomeChances.Count - 1 : 0];
    }
}
