/*************************************************************************
 * Break Loyalty — ritual outcome comps.
 *
 * Each comp turns one property of the target prisoner into a quality offset
 * via an XML curve, so every factor is visible in the ritual's predicted and
 * final quality breakdown. Caster social skill and spectator count are covered
 * by the vanilla comps (RitualOutcomeComp_PawnStatScaled,
 * RitualOutcomeComp_ParticipantCount); the comps here add the factors vanilla
 * has no equivalent for.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using RimWorld;
using UnityEngine;
using Verse;

namespace BreakLoyalty;

/// Shared base for factors read off the ceremony's target prisoner. Subclasses
/// supply a raw metric; the XML curve maps it to a quality offset. Implements
/// both the runtime path (`Count`, during the ritual) and the pre-ritual
/// prediction path (`GetQualityFactor`, in the assignment dialog).
public abstract class RitualOutcomeComp_BreakLoyaltyTarget : RitualOutcomeComp_Quality {
    public override bool DataRequired => false;

    protected abstract float MetricFor(Pawn subject);

    /// Value shown in the "x / max" column of the quality breakdown.
    protected virtual string CountLabel(Pawn subject, float metric) {
        return metric.ToString("0.#");
    }

    public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data) {
        Pawn subject = ritual.PawnWithRole("subject");
        return subject != null ? MetricFor(subject) : 0f;
    }

    public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget,
            RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data) {
        Pawn subject = assignments?.FirstAssignedPawn("subject");
        float metric = subject != null ? MetricFor(subject) : 0f;
        float quality = curve != null ? curve.Evaluate(metric) : 0f;
        return new QualityFactor {
            label = label.CapitalizeFirst(),
            count = CountLabel(subject, metric),
            qualityChange = ExpectedOffsetDesc(positive: true, quality),
            quality = quality,
            positive = true,
            priority = 2f
        };
    }
}

/// Disillusionment with the prisoner's own faction. A loyalist whose faction is
/// hostile is far easier to turn than one whose faction is your ally. Metric
/// runs 0 (allied, +100 goodwill) to 200 (hostile, -100 goodwill).
public class RitualOutcomeComp_TargetDisillusionment : RitualOutcomeComp_BreakLoyaltyTarget {
    protected override float MetricFor(Pawn subject) {
        Faction faction = subject.Faction;
        if (faction == null || faction.IsPlayer) return 0f;
        return Mathf.Clamp(100 - faction.PlayerGoodwill, 0f, 200f);
    }
}

/// The prisoner already shares your colony's ideoligion. Metric is 1 or 0.
public class RitualOutcomeComp_TargetSharesColonyIdeo : RitualOutcomeComp_BreakLoyaltyTarget {
    protected override float MetricFor(Pawn subject) {
        Ideo colonyIdeo = Faction.OfPlayer?.ideos?.PrimaryIdeo;
        return colonyIdeo != null && subject.Ideo == colonyIdeo ? 1f : 0f;
    }
}

/// Bonds with your colonists. Counts free colonists who think well of the
/// prisoner (opinion at or above the threshold), covering friends, lovers, and
/// family alike since all raise opinion.
public class RitualOutcomeComp_TargetColonyBonds : RitualOutcomeComp_BreakLoyaltyTarget {
    public int minOpinion = 20;

    protected override float MetricFor(Pawn subject) {
        Map map = subject.MapHeld;
        if (map == null || subject.relations == null) return 0f;

        int bonds = 0;
        foreach (Pawn colonist in map.mapPawns.FreeColonistsSpawned) {
            if (colonist != subject && colonist.relations.OpinionOf(subject) >= minOpinion) {
                bonds++;
            }
        }
        return bonds;
    }
}

/// The prisoner's contentment. A prisoner treated well is more persuadable than
/// a miserable one. Metric is current mood as whole percentage points (0..100),
/// kept integer so both the prediction panel and the result letter read cleanly.
public class RitualOutcomeComp_TargetContentment : RitualOutcomeComp_BreakLoyaltyTarget {
    protected override float MetricFor(Pawn subject) {
        float mood = subject.needs?.mood?.CurLevelPercentage ?? 0.5f;
        return Mathf.Round(mood * 100f);
    }

    protected override string CountLabel(Pawn subject, float metric) {
        return Mathf.RoundToInt(metric) + "%";
    }
}
