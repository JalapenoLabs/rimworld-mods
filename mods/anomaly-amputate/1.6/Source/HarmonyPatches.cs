using System;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnomalyAmputate;

[HarmonyPatch(typeof(ITab_Pawn_Health), "ShouldAllowOperations")]
internal static class HealthOperationsPatch
{
    private static readonly Func<ITab_Pawn_Health, Pawn> PawnForHealthGetter =
        AccessTools.MethodDelegate<Func<ITab_Pawn_Health, Pawn>>(AccessTools.PropertyGetter(typeof(ITab_Pawn_Health), "PawnForHealth"));

    private static readonly Func<ITab, Thing> SelThingGetter =
        AccessTools.MethodDelegate<Func<ITab, Thing>>(AccessTools.PropertyGetter(typeof(ITab), "SelThing"));

    [HarmonyPostfix]
    private static void AllowEntitiesOnHoldingPlatform(ITab_Pawn_Health __instance, ref bool __result)
    {
        if (__result)
        {
            return;
        }

        Pawn pawn = PawnForHealthGetter(__instance);
        if (pawn == null || pawn.Dead || !LimbUtility.IsGorehulk(pawn))
        {
            return;
        }

        CompHoldingPlatformTarget comp = pawn.TryGetComp<CompHoldingPlatformTarget>();
        if (comp == null || !comp.CurrentlyHeldOnPlatform)
        {
            return;
        }

        Thing selThing = SelThingGetter(__instance);
        if (selThing?.def?.AllRecipes == null)
        {
            return;
        }

        if (!selThing.def.AllRecipes.Any(recipe => recipe.AvailableNow && recipe.AvailableOnNow(pawn)))
        {
            return;
        }

        if (pawn.IsMutant && !pawn.mutant.Def.entitledToMedicalCare)
        {
            return;
        }

        __result = true;
    }
}

[HarmonyPatch(typeof(CompProducesBioferrite), nameof(CompProducesBioferrite.BioferritePerDay))]
internal static class BioferritePenaltyPatch
{
    private const float PenaltyPerLimb = 0.16f;

    [HarmonyPostfix]
    private static void ApplyLimbPenalty(Pawn pawn, ref float __result)
    {
        if (__result <= 0f || pawn == null || !LimbUtility.IsGorehulk(pawn))
        {
            return;
        }

        int missingLimbs = LimbUtility.CountMissingManipulationOrMovingLimbs(pawn);
        if (missingLimbs <= 0)
        {
            return;
        }

        float factor = Mathf.Max(0f, 1f - PenaltyPerLimb * missingLimbs);
        __result *= factor;
    }
}

[HarmonyPatch(typeof(CompStudiable), "AdjustedAnomalyKnowledge")]
internal static class StudyPenaltyPatch
{
    private const float PenaltyPerLimb = 0.16f;

    [HarmonyPostfix]
    private static void ApplyLimbPenalty(CompStudiable __instance, StringBuilder sb, ref float __result)
    {
        if (__result <= 0f || __instance == null)
        {
            return;
        }

        if (__instance.KnowledgeCategory != KnowledgeCategoryDefOf.Basic)
        {
            return;
        }

        if (__instance.parent is not Pawn pawn || !LimbUtility.IsGorehulk(pawn))
        {
            return;
        }

        int missingLimbs = LimbUtility.CountMissingManipulationOrMovingLimbs(pawn);
        if (missingLimbs <= 0)
        {
            return;
        }

        float factor = Mathf.Max(0f, 1f - PenaltyPerLimb * missingLimbs);
        float originalResult = __result;
        __result *= factor;

        if (sb == null)
        {
            return;
        }

        string content = sb.ToString();
        string finalLabel = "StatsReport_FinalValue".Translate();
        int finalIndex = content.LastIndexOf(finalLabel, StringComparison.Ordinal);
        sb.Clear();
        sb.Append(finalIndex >= 0 ? content.Substring(0, finalIndex) : content);
        sb.AppendLineIfNotEmpty();
        sb.AppendLine("AnomalyAmputation_StudyPenalty".Translate(missingLimbs, factor.ToStringPercent(), originalResult.ToStringDecimalIfSmall()));
        sb.Append(finalLabel);
        sb.Append(": ");
        sb.Append(__result.ToStringDecimalIfSmall());
    }
}

[HarmonyPatch(typeof(ContainmentUtility), nameof(ContainmentUtility.CanParticipateInEscape))]
internal static class EscapeEligibilityPatch
{
    [HarmonyPostfix]
    private static void BlockForDisarmedGorehulk(Pawn pawn, StringBuilder sb, ref bool __result)
    {
        if (!__result || pawn == null || !LimbUtility.IsGorehulk(pawn))
        {
            return;
        }

        if (LimbUtility.CountMissingLegs(pawn) >= 2)
        {
            __result = false;
            sb?.AppendLineIfNotEmpty();
            sb?.Append("  - ");
            sb?.Append("AnomalyAmputation_NoLegs".Translate());
            sb?.Append(": x0%");
        }
    }
}

[HarmonyPatch(typeof(Ability), nameof(Ability.CanCast), MethodType.Getter)]
internal static class AbilityRestrictionPatch
{
    [HarmonyPostfix]
    private static void DisableSpineLaunchWhenDisarmed(Ability __instance, ref AcceptanceReport __result)
    {
        if (!__result.Accepted || __instance?.def == null || __instance.def.defName != "SpineLaunch_Gorehulk")
        {
            return;
        }

        Pawn pawn = __instance.pawn;
        if (!LimbUtility.IsGorehulk(pawn))
        {
            return;
        }

        if (LimbUtility.CountMissingArms(pawn) >= 2 && LimbUtility.MissingJaw(pawn))
        {
            __result = "AnomalyAmputation_Disarmed".Translate();
        }
    }
}
