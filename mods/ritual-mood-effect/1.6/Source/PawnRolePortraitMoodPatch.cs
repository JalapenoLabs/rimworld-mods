using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RitualMoodEffect;

// Postfix the shared portrait drawer that every ritual-style dialog routes
// its pawn portraits through. The drawer lives on a generic base class,
// PawnRoleSelectionWidgetBase<RoleType>, with two closed instantiations:
//   * PawnRoleSelectionWidgetBase<RitualRole>          (Ideology rituals,
//                                                       Odyssey gravship launches)
//   * PawnRoleSelectionWidgetBase<PsychicRitualRoleDef> (Anomaly psychic rituals)
//
// We enumerate both via TargetMethods so a single postfix covers every dialog.
//
// Reuses the vanilla MoodThreshold helpers used by ColonistBarColonistDrawer,
// so the color cue inside the dialog is identical to the topbar's.
[HarmonyPatch]
internal static class DrawPawnPortraitInternal_MoodOverlay {
    private const string TargetMethodName = "DrawPawnPortraitInternal";

    // Match the vanilla colonist bar: minor/major use a soft wash, extreme is heavier.
    private const float FillAlphaMinorOrMajor = 0.10f;
    private const float FillAlphaExtreme = 0.15f;
    private const int InnerBorderThickness = 2;

    // The original method computes Rect(r.x, r.y, r.width * scale, 50f * scale)
    // for the portrait area; the 20f-tall label band sits beneath it. We mirror
    // the same math so the overlay covers the portrait only.
    private const float PortraitHeight = 50f;

    private static IEnumerable<MethodBase> TargetMethods() {
        yield return AccessTools.Method(
            typeof(PawnRoleSelectionWidgetBase<RitualRole>), TargetMethodName);
        yield return AccessTools.Method(
            typeof(PawnRoleSelectionWidgetBase<PsychicRitualRoleDef>), TargetMethodName);
    }

    [HarmonyPostfix]
    private static void OverlayMoodBand(Rect r, Pawn pawn, bool dragging, float scale) {
        if (!ShouldDrawOverlay(pawn)) {
            return;
        }

        MoodThreshold threshold = MoodThresholdExtensions.CurrentMoodThresholdFor(pawn);
        if (threshold == MoodThreshold.None) {
            return;
        }

        Rect portraitRect = new Rect(r.x, r.y, r.width * scale, PortraitHeight * scale);
        Color color = threshold.GetColor();
        float fillAlpha = threshold >= MoodThreshold.Major ? FillAlphaExtreme : FillAlphaMinorOrMajor;

        // Soft wash inside the rect, mirroring ColonistBarColonistDrawer.DrawColonist.
        Widgets.DrawBoxSolid(portraitRect, color.ToTransparent(fillAlpha));

        // Saturated inside-edge border so the band reads at a glance.
        // Drawn inside the rect (not expanded) because dialog slots are tightly
        // packed and an outward atlas would clip neighbouring portraits.
        Color previousGuiColor = GUI.color;
        GUI.color = color;
        Widgets.DrawBox(portraitRect, InnerBorderThickness);
        GUI.color = previousGuiColor;
    }

    // Mirrors the gating in ColonistBarColonistDrawer.DrawColonist so we never
    // overlay a band on a pawn the topbar itself would skip.
    private static bool ShouldDrawOverlay(Pawn pawn) {
        if (!Prefs.VisibleMood) {
            return false;
        }
        if (pawn == null || pawn.Dead || pawn.Downed) {
            return false;
        }
        if (pawn.needs?.mood == null || pawn.mindState == null) {
            return false;
        }
        return pawn.mindState.mentalBreaker.CanDoRandomMentalBreaks;
    }
}
