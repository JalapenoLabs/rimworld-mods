/*************************************************************************
 * Break Loyalty — a RimWorld (Ideology) mod by JalapenoLabs.
 *
 * Turns "unwaveringly loyal" from a permanent wall into a low-odds ceremony.
 * Core plumbing: def references, the single mutation that breaks a prisoner's
 * loyalty, and the shared launch helpers used by both the right-click option
 * and the selected-prisoner command.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using System.Collections.Generic;
using HarmonyLib;
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

/// The state change the mod exists to perform, plus the helpers that launch the
/// ceremony from a prisoner (shared by the float-menu option and the gizmo).
public static class BreakLoyaltyUtility {
    /// Clears a prisoner's unwavering loyalty so they can be recruited.
    ///
    /// `Pawn_GuestTracker.Recruitable` is a computed getter (it also honors the
    /// storyteller's "unwavering prisoners" difficulty), but its setter simply
    /// writes the backing field. Setting it true is therefore the correct, and
    /// only needed, way to lift the wall, no reflection required.
    public static void MakeRecruitable(Pawn prisoner) {
        if (prisoner?.guest != null) {
            prisoner.guest.Recruitable = true;
        }
    }

    /// True when the pawn is exactly what the ceremony targets: an unwaveringly
    /// loyal prisoner of the colony.
    public static bool IsUnwaveringPrisonerOfColony(Pawn pawn) {
        return pawn != null
            && pawn.IsPrisonerOfColony
            && pawn.guest != null
            && !pawn.guest.Recruitable;
    }

    /// The ceremony precept on the player's ideoligion (present because it is
    /// `classic`). Null only if Ideology is somehow absent.
    public static Precept_Ritual FindRitual() {
        Ideo ideo = Faction.OfPlayer?.ideos?.PrimaryIdeo;
        if (ideo == null) {
            return null;
        }
        foreach (Precept precept in ideo.PreceptsListForReading) {
            if (precept is Precept_Ritual ritual && ritual.def == BreakLoyaltyDefOf.BreakLoyalty) {
                return ritual;
            }
        }
        return null;
    }

    /// Opens the ritual dialog with the prisoner pre-assigned, sited at their
    /// own tile so the ceremony can be held in the cell.
    public static void BeginCeremony(Pawn prisoner, Pawn organizer) {
        Precept_Ritual ritual = FindRitual();
        if (ritual == null || prisoner?.Map == null) {
            return;
        }
        ritual.ShowRitualBeginWindow(
            new TargetInfo(prisoner.Position, prisoner.Map),
            forObligation: null,
            selectedPawn: organizer,
            forcedForRole: new Dictionary<string, Pawn> { { "subject", prisoner } });
    }
}

[StaticConstructorOnStartup]
public static class BreakLoyaltyStartup {
    static BreakLoyaltyStartup() {
        new Harmony("jalapenolabs.rimworld.breakloyalty").PatchAll();
        Log.Message("[Break Loyalty] Loaded.");
    }
}
