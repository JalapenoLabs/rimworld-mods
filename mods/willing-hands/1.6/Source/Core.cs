/*************************************************************************
 * Willing Hands — a RimWorld (Ideology) mod by JalapenoLabs.
 *
 * Well-treated slaves may choose to become full colonists. The mirror of a
 * slave rebellion: contentment, not suppression, is what builds.
 *
 * Core plumbing: tuning constants and def references.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using RimWorld;
using Verse;

namespace WillingHands;

/// Tuning for the belonging system. Constants for now; a natural home for mod
/// settings later.
public static class WillingHandsTuning {
    /// A slave counts as content only above BOTH thresholds (need levels, 0..1).
    public const float ComfortThreshold = 0.65f;
    public const float MoodThreshold = 0.85f;

    /// Sustained contentment required before a slave might ask to join. 15 days.
    public const int RequiredContentTicks = 900000;

    /// How often the belonging tracker samples each slave. ~1 in-game hour.
    public const int CheckIntervalTicks = 2500;

    /// Chance per day, once fully settled, that a slave asks to join.
    public const float AskChancePerDay = 0.05f;

    /// A lapse in treatment erodes progress this many times faster than it built.
    public const float LapseDecayMultiplier = 2f;

    /// Quiet period after an ask is sent, so letters never stack up. 5 days.
    public const int AskCooldownTicks = 300000;

    /// Longer pause after the player refuses. 10 days.
    public const int DeclineCooldownTicks = 600000;
}

/// Strongly typed references to defs this mod declares in XML.
[DefOf]
public static class WillingHandsDefOf {
    public static LetterDef WillingHands_JoinRequest;
    public static ThoughtDef WillingHands_ChoseToJoin;

    static WillingHandsDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(WillingHandsDefOf));
    }
}

[StaticConstructorOnStartup]
public static class WillingHandsStartup {
    static WillingHandsStartup() {
        Log.Message("[Willing Hands] Loaded.");
    }
}
