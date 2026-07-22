/*************************************************************************
 * Break Loyalty — ritual outcome worker.
 *
 * Resolves the ceremony. Ritual quality, summed from the outcome comps and
 * hard-capped by the def's maxQuality (0.25), IS the success probability, so
 * the dialog's quality number is literally the chance to break loyalty. A
 * flawless ceremony on a disillusioned, well-treated prisoner reaches the
 * design ceiling; a rushed one on a devoted loyalist barely moves off the base.
 *
 * The def declares no outcomeChances: the vanilla two-outcome table normalizes
 * its numbers in a way that would contradict the single quality-as-chance roll
 * used here, so this worker owns the roll and writes its own result letter.
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

    public override bool SupportsAttachableOutcomeEffect => false;

    /// Relabels the dialog's headline number from the generic "Expected quality"
    /// to what it actually represents here: the chance to break loyalty.
    public override string ExpectedQualityLabel() {
        return "BreakLoyalty.ChanceLabel".Translate();
    }

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

        string body = (success ? "BreakLoyalty.Outcome.Success" : "BreakLoyalty.Outcome.Failure").Translate();
        body += "\n\n" + OutcomeQualityBreakdownDesc(chance, progress, jobRitual);

        string label = (success ? "BreakLoyalty.OutcomeLetter.Success" : "BreakLoyalty.OutcomeLetter.Failure").Translate();
        LookTargets lookAt = subject != null ? (LookTargets)subject : jobRitual.selectedTarget;
        Find.LetterStack.ReceiveLetter(
            label, body,
            success ? LetterDefOf.RitualOutcomePositive : LetterDefOf.RitualOutcomeNegative,
            lookAt);
    }
}
