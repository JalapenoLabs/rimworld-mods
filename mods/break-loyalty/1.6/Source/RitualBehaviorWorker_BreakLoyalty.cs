/*************************************************************************
 * Break Loyalty — ritual behavior worker.
 *
 * The XML behavior handles gathering the audience and having the speaker
 * escort the prisoner to the ritual spot. This worker only cleans up
 * afterward, returning the prisoner to a cell, mirroring the vanilla
 * conversion ritual.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using RimWorld;
using Verse;

namespace BreakLoyalty;

public class RitualBehaviorWorker_BreakLoyalty : RitualBehaviorWorker {
    public RitualBehaviorWorker_BreakLoyalty() { }

    public RitualBehaviorWorker_BreakLoyalty(RitualBehaviorDef def) : base(def) { }

    public override void PostCleanup(LordJob_Ritual ritual) {
        base.PostCleanup(ritual);

        Pawn speaker = ritual.PawnWithRole("speaker");
        Pawn subject = ritual.PawnWithRole("subject");
        if (subject != null && subject.IsPrisonerOfColony) {
            // Send the prisoner back to prison and stop them bolting the instant
            // the ceremony breaks up.
            WorkGiver_Warden_TakeToBed.TryTakePrisonerToBed(subject, speaker);
            subject.guest.WaitInsteadOfEscapingFor(1250);
        }
    }
}
