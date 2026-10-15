/*************************************************************************
 * Synced Storyteller — seeded incident selection.
 *
 * Every 1000 ticks each storyteller comp is asked, per incident target, what
 * should happen now: whether an event occurs, its category, and which
 * incident is picked. This patch runs that decision on a seed built from the
 * world, the tick, the comp, and the target.
 *
 * All StorytellerComp overrides are patched, including those from other
 * mods, because each storyteller comp rolls its own dice.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SyncedStoryteller;

[HarmonyPatch]
internal static class StorytellerComp_MakeIntervalIncidents_Patch {
    private static IEnumerable<MethodBase> TargetMethods() {
        // DeclaredOnly: a comp that inherits its parent's override must not
        // patch the same method twice. Abstract comps are included because a
        // modded base comp can hold the override its concrete children use.
        const BindingFlags declaredInstance = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        Type[] parameterTypes = { typeof(IIncidentTarget) };

        foreach (Type compType in typeof(StorytellerComp).AllSubclasses()) {
            MethodInfo method = compType.GetMethod(
                nameof(StorytellerComp.MakeIntervalIncidents),
                declaredInstance,
                binder: null,
                parameterTypes,
                modifiers: null);
            if (method != null && !method.IsAbstract) {
                yield return method;
            }
        }
    }

    // Overrides may name the parameter differently, so bind it by position.
    [HarmonyPostfix]
    private static void Postfix(
        StorytellerComp __instance,
        [HarmonyArgument(0)] IIncidentTarget target,
        ref IEnumerable<FiringIncident> __result) {
        // The comp's position in the storyteller def, the same salt vanilla
        // uses for its threat cycles.
        int compIndex = Find.Storyteller.storytellerComps.IndexOf(__instance);
        __result = SeededIncidents(__result, StorySeed.ForTarget(target, compIndex));
    }

    /// Every override is an iterator, so the postfix only wraps a lazy
    /// sequence and the rolls happen later, inside MoveNext. The storyteller
    /// fires each incident between MoveNext calls, so the seeded state is
    /// popped before each yield. Otherwise the incident's execution would
    /// run on this stream instead of its own.
    private static IEnumerable<FiringIncident> SeededIncidents(IEnumerable<FiringIncident> incidents, int intervalSeed) {
        using IEnumerator<FiringIncident> enumerator = incidents.GetEnumerator();
        for (int step = 0; ; step++) {
            bool hasNext;
            Rand.PushState(Gen.HashCombineInt(intervalSeed, step));
            try {
                hasNext = enumerator.MoveNext();
            } finally {
                Rand.PopState();
            }

            if (!hasNext) {
                yield break;
            }
            yield return enumerator.Current;
        }
    }
}
