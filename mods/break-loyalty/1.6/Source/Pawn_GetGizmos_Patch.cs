/*************************************************************************
 * Break Loyalty — selected-prisoner command.
 *
 * Adds a "Break loyalty" command to the command bar when an unwaveringly loyal
 * prisoner is selected, alongside vanilla actions like Strip. Clicking it opens
 * the ceremony dialog with that prisoner pre-assigned.
 *
 * A narrow postfix on Pawn.GetGizmos is the only race-agnostic seam for adding
 * a pawn gizmo; the alternative (a ThingComp) would have to be patched onto
 * every humanlike race def, including modded ones.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BreakLoyalty;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
internal static class Pawn_GetGizmos_BreakLoyalty {
    private static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("UI/Icons/Rituals/Trial");

    [HarmonyPostfix]
    private static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance) {
        foreach (Gizmo gizmo in __result) {
            yield return gizmo;
        }

        if (!ModsConfig.IdeologyActive || !BreakLoyaltyUtility.IsUnwaveringPrisonerOfColony(__instance)) {
            yield break;
        }
        Precept_Ritual ritual = BreakLoyaltyUtility.FindRitual();
        if (ritual == null) {
            yield break;
        }

        Command_Action command = new Command_Action {
            defaultLabel = "BreakLoyalty.Gizmo.Label".Translate(),
            defaultDesc = "BreakLoyalty.Gizmo.Desc".Translate(__instance.LabelShort),
            icon = Icon,
            action = delegate {
                BreakLoyaltyUtility.BeginCeremony(__instance, null);
            }
        };

        // Mirror the ritual-spot gizmo: grey out while the cooldown is running.
        int ticksLeft = ritual.abilityOnCooldownUntilTick - Find.TickManager.TicksGame;
        if (ticksLeft > 0) {
            command.Disable("BreakLoyalty.Gizmo.OnCooldown".Translate(ticksLeft.ToStringTicksToPeriod()));
        }

        yield return command;
    }
}
