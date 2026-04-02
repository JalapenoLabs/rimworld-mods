using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnomalyAmputate;

public class ModEntry : Mod
{
    private static readonly Harmony HarmonyInstance = new Harmony("jalapenolabs.rimworld.anomalyamputate");

    public ModEntry(ModContentPack content) : base(content)
    {
        HarmonyInstance.PatchAll();
        InjectStatPart();
        Log.Message("[AnomalyAmputate] Loaded gorehulk amputation support.");
    }

    private static void InjectStatPart()
    {
        StatDef stat = StatDefOf.MinimumContainmentStrength;
        stat.parts ??= new List<StatPart>();
        if (stat.parts.OfType<StatPart_GorehulkAmputation>().Any())
        {
            return;
        }
        stat.parts.Add(new StatPart_GorehulkAmputation());
    }
}

public class StatPart_GorehulkAmputation : StatPart
{
    private const float ContainmentReductionPerLimb = 15f;

    public override void TransformValue(StatRequest req, ref float val)
    {
        if (!TryGetGorehulk(req, out Pawn pawn))
        {
            return;
        }

        int missingLimbs = LimbUtility.CountMissingManipulationOrMovingLimbs(pawn);
        if (missingLimbs <= 0)
        {
            return;
        }

        float reduction = missingLimbs * ContainmentReductionPerLimb;
        val = Mathf.Max(0f, val - reduction);
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!TryGetGorehulk(req, out Pawn pawn))
        {
            return null;
        }

        int missingLimbs = LimbUtility.CountMissingManipulationOrMovingLimbs(pawn);
        if (missingLimbs <= 0)
        {
            return null;
        }

        float reduction = missingLimbs * ContainmentReductionPerLimb;
        return "AnomalyAmputation_ContainmentReduction".Translate(missingLimbs, reduction.ToString("F0"));
    }

    private static bool TryGetGorehulk(StatRequest req, out Pawn pawn)
    {
        pawn = req.Thing as Pawn;
        return pawn != null && LimbUtility.IsGorehulk(pawn);
    }
}
