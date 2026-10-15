/*************************************************************************
 * Synced Storyteller — a RimWorld mod by JalapenoLabs.
 *
 * Makes the storyteller deterministic per world seed, so two players who
 * start the same world on the same tile get the same events on the same
 * days, however differently they play.
 *
 * RimWorld draws nearly all randomness from one global stream (Verse.Rand):
 * pawn AI, combat, plants, weather, and the storyteller all pull from it, so
 * two games diverge within minutes of their first different click. This mod
 * gives the storyteller its own seeded state at the two places it rolls
 * dice: when it decides what happens, and when an incident executes.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SyncedStoryteller;

/// Builds the seeds the storyteller patches push onto Rand's state stack.
public static class StorySeed {
    /// Every seed derives from the world's `persistentRandomValue`. It is the
    /// first `Rand.Int` drawn after world generation seeds Rand from the seed
    /// string, so equal seed strings produce equal values. Vanilla already
    /// uses it to time Cassandra's and Phoebe's threat cycles.
    ///
    /// Each seed also folds in the current tick and the incident target, so a
    /// difference between the two colonies (an extra caravan, an event that
    /// could not fire) cannot shift any other roll.
    public static int ForTarget(IIncidentTarget target, int salt) {
        int worldValue = Find.World.info.persistentRandomValue;
        return Gen.HashCombineInt(worldValue, Find.TickManager.TicksGame, target.ConstantRandSeed, salt);
    }
}

[StaticConstructorOnStartup]
public static class SyncedStorytellerStartup {
    static SyncedStorytellerStartup() {
        Harmony harmony = new Harmony("jalapenolabs.rimworld.syncedstoryteller");
        harmony.PatchAll();
        // The count covers every storyteller comp override plus TryFire, so a
        // modded storyteller that was picked up shows here.
        Log.Message($"[Synced Storyteller] Loaded, {harmony.GetPatchedMethods().Count()} methods seeded.");
    }
}
