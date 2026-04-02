using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AnomalyAmputate;

public static class LimbUtility
{
    private static readonly HashSet<string> TargetDefNames = new HashSet<string> { "Gorehulk" };

    private static readonly HashSet<BodyPartTagDef> LimbTags = new HashSet<BodyPartTagDef>
    {
        BodyPartTagDefOf.MovingLimbCore,
        BodyPartTagDefOf.ManipulationLimbCore
    };

    public static bool IsGorehulk(Pawn pawn)
    {
        if (pawn == null)
        {
            return false;
        }

        if (pawn.def != null && TargetDefNames.Contains(pawn.def.defName))
        {
            return true;
        }

        return pawn.kindDef != null && TargetDefNames.Contains(pawn.kindDef.defName);
    }

    public static int CountMissingManipulationOrMovingLimbs(Pawn pawn)
    {
        if (!IsGorehulk(pawn) || pawn.health?.hediffSet == null)
        {
            return 0;
        }

        return pawn.health.hediffSet.GetMissingPartsCommonAncestors()
            .Count(missing => missing.Part != null && HasLimbTag(missing.Part.def));
    }

    public static int CountMissingLegs(Pawn pawn)
    {
        if (!IsGorehulk(pawn) || pawn.health?.hediffSet == null)
        {
            return 0;
        }

        return pawn.health.hediffSet.GetMissingPartsCommonAncestors()
            .Count(missing => missing.Part != null && missing.Part.def.tags != null && missing.Part.def.tags.Contains(BodyPartTagDefOf.MovingLimbCore));
    }

    public static int CountMissingArms(Pawn pawn)
    {
        if (!IsGorehulk(pawn) || pawn.health?.hediffSet == null)
        {
            return 0;
        }

        return pawn.health.hediffSet.GetMissingPartsCommonAncestors()
            .Count(missing => missing.Part != null && missing.Part.def.tags != null && missing.Part.def.tags.Contains(BodyPartTagDefOf.ManipulationLimbCore));
    }

    public static bool MissingJaw(Pawn pawn)
    {
        if (!IsGorehulk(pawn) || pawn.health?.hediffSet == null)
        {
            return false;
        }

        BodyPartRecord jaw = pawn.RaceProps?.body?.AllParts?.FirstOrDefault(part => part.def?.defName == "Jaw");
        if (jaw == null)
        {
            return false;
        }

        return pawn.health.hediffSet.PartIsMissing(jaw);
    }

    private static bool HasLimbTag(BodyPartDef def)
    {
        if (def?.tags == null)
        {
            return false;
        }

        return def.tags.Any(LimbTags.Contains);
    }
}
