/*************************************************************************
 * Willing Hands — the join-request letter.
 *
 * Offers the player a choice: welcome the slave as a colonist, or keep them
 * enslaved. Accepting converts them via RecruitUtility.Recruit, which clears
 * guest status and ignores the "unwaveringly loyal" flag, so a willing slave
 * can always be welcomed.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WillingHands;

public class ChoiceLetter_SlaveJoinRequest : ChoiceLetter {
    public Pawn slave;

    public override bool CanShowInLetterStack =>
        base.CanShowInLetterStack
        && slave != null && !slave.Dead && slave.Spawned && slave.IsSlaveOfColony;

    public override IEnumerable<DiaOption> Choices {
        get {
            if (ArchivedOnly) {
                yield return Option_Close;
                yield break;
            }

            DiaOption accept = new DiaOption("WillingHands.Accept".Translate());
            accept.action = delegate {
                Pawn joiner = slave;
                RecruitUtility.Recruit(joiner, Faction.OfPlayer);
                joiner.needs?.mood?.thoughts?.memories?.TryGainMemory(WillingHandsDefOf.WillingHands_ChoseToJoin);
                Messages.Message("WillingHands.Joined".Translate(joiner.LabelShortCap), joiner, MessageTypeDefOf.PositiveEvent);
                Find.LetterStack.RemoveLetter(this);
            };
            accept.resolveTree = true;
            yield return accept;

            DiaOption refuse = new DiaOption("WillingHands.Refuse".Translate());
            refuse.action = delegate {
                slave?.Map?.GetComponent<MapComponent_SlaveBelonging>()?.NotifyDeclined(slave);
                Find.LetterStack.RemoveLetter(this);
            };
            refuse.resolveTree = true;
            yield return refuse;

            if (lookTargets.IsValid()) {
                yield return Option_JumpToLocationAndPostpone;
            }
            yield return Option_Postpone;
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref slave, "slave");
    }
}
