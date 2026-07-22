/*************************************************************************
 * Break Loyalty — a RimWorld (Ideology) mod by JalapenoLabs.
 *
 * Turns "unwaveringly loyal" from a permanent wall into a low-odds ceremony.
 * Core plumbing: the def references and the single mutation that breaks a
 * prisoner's loyalty.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using RimWorld;
using Verse;

namespace BreakLoyalty;

/// Strongly typed references to defs this mod declares in XML.
[DefOf]
public static class BreakLoyaltyDefOf {
    /// The ceremony precept, present on every ideoligion (it is `classic`).
    /// Used to open the ritual dialog directly from a prisoner.
    public static PreceptDef BreakLoyalty;

    /// Stacking negative memory applied to a prisoner each time a ceremony fails.
    /// Cleared when a later ceremony succeeds.
    public static ThoughtDef BreakLoyalty_FailedCeremony;

    static BreakLoyaltyDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(BreakLoyaltyDefOf));
    }
}

/// The one state change the whole mod exists to perform.
public static class BreakLoyaltyUtility {
    /// Clears a prisoner's unwavering loyalty so they can be recruited.
    ///
    /// `Pawn_GuestTracker.Recruitable` is a computed getter (it also honors the
    /// storyteller's "unwavering prisoners" difficulty), but its setter simply
    /// writes the backing field. Setting it true is therefore the correct, and
    /// only needed, way to lift the wall, no Harmony or reflection required.
    public static void MakeRecruitable(Pawn prisoner) {
        if (prisoner?.guest != null) {
            prisoner.guest.Recruitable = true;
        }
    }
}

[StaticConstructorOnStartup]
public static class BreakLoyaltyStartup {
    static BreakLoyaltyStartup() {
        Log.Message("[Break Loyalty] Loaded.");
    }
}
