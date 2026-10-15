/*************************************************************************
 * Synced Storyteller — seeded incident execution.
 *
 * Choosing "raid" is half the story; the raid's faction, strategy, arrival
 * method, and spawn point are rolled when the incident executes. This patch
 * runs each fire on a seed built from the world, the tick, the incident, and
 * the target. TryFire is also the path for queued incidents, so delayed
 * events are covered too.
 *
 * In dev mode every fire is logged, so two players can compare logs line by
 * line to see where their stories matched and where they split.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SyncedStoryteller;

[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.TryFire))]
internal static class Storyteller_TryFire_Patch {
    // Harmony binds arguments by the original's parameter name, which is `fi`.
    [HarmonyPrefix]
    private static void Prefix([HarmonyArgument("fi")] FiringIncident firingIncident, out bool __state) {
        // defName rather than shortHash: shortHash depends on how def hash
        // collisions resolve, while the name is identical in both games.
        int incidentSalt = GenText.StableStringHash(firingIncident.def.defName);
        Rand.PushState(StorySeed.ForTarget(firingIncident.parms.target, incidentSalt));
        __state = true;
    }

    [HarmonyPostfix]
    private static void Postfix([HarmonyArgument("fi")] FiringIncident firingIncident, bool __result) {
        if (!Prefs.DevMode) {
            return;
        }

        string outcome = __result
            ? "fired"
            : "could not fire";
        Log.Message(
            $"[Synced Storyteller] Tick {Find.TickManager.TicksGame}: "
            + $"{firingIncident.def.defName} on {firingIncident.parms.target} {outcome}.");
    }

    /// Pops in a finalizer so an incident that throws cannot leave the seeded
    /// state on Rand's stack. `__state` guards the case where the prefix
    /// threw before it pushed.
    [HarmonyFinalizer]
    private static Exception Finalizer(Exception __exception, bool __state) {
        if (__state) {
            Rand.PopState();
        }
        return __exception;
    }
}
