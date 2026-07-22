/*************************************************************************
 * Break Loyalty — right-click launcher.
 *
 * Adds a float-menu option on an unwaveringly loyal prisoner to begin the
 * ceremony then and there, with the prisoner pre-assigned as the subject. The
 * ceremony is held at the prisoner's own tile (their cell), so it can be
 * started without walking a colonist over to a ritual spot.
 *
 * FloatMenuOptionProvider subclasses are discovered and instantiated
 * automatically by FloatMenuMakerMap, so no def registration is needed.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BreakLoyalty;

public class FloatMenuOptionProvider_BreakLoyalty : FloatMenuOptionProvider {
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;

    protected override bool AppliesInt(FloatMenuContext context) {
        return ModsConfig.IdeologyActive;
    }

    protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context) {
        // Only unwaveringly loyal prisoners of the colony are valid subjects.
        if (clickedPawn == null || !clickedPawn.IsPrisonerOfColony) return null;
        if (clickedPawn.guest == null || clickedPawn.guest.Recruitable) return null;

        Precept_Ritual ritual = FindRitual();
        if (ritual == null) return null;

        Pawn organizer = context.FirstSelectedPawn;
        return new FloatMenuOption(
            "BreakLoyalty.FloatMenu.Begin".Translate(clickedPawn.LabelShort),
            delegate {
                ritual.ShowRitualBeginWindow(
                    new TargetInfo(clickedPawn.Position, clickedPawn.Map),
                    forObligation: null,
                    selectedPawn: organizer,
                    forcedForRole: new Dictionary<string, Pawn> { { "subject", clickedPawn } });
            });
    }

    /// The ceremony precept lives on the player's ideoligion (it is `classic`,
    /// so always present). Returns null if Ideology is somehow absent.
    private static Precept_Ritual FindRitual() {
        Ideo ideo = Faction.OfPlayer?.ideos?.PrimaryIdeo;
        if (ideo == null) return null;
        foreach (Precept precept in ideo.PreceptsListForReading) {
            if (precept is Precept_Ritual ritual && ritual.def == BreakLoyaltyDefOf.BreakLoyalty) {
                return ritual;
            }
        }
        return null;
    }
}
