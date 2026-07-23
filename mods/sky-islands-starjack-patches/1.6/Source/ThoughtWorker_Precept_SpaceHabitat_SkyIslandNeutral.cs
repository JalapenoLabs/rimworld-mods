/*************************************************************************
 * Sky Islands — Starjack Patches: shipborn habitat mood.
 *
 * Odyssey's "space habitat" precept gives shipborn pawns +4 in space and -2
 * on a planet, deciding purely on tile.LayerDef.isSpace. A sky island is
 * neither, so this worker returns no thought there: a neutral middle ground.
 * Everywhere else it defers to vanilla.
 *
 * Selected in place of the vanilla worker by a PatchOperation on the
 * SpaceHabitat_Mood thought's workerClass.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using RimWorld;
using Verse;

namespace SkyIslandsStarjackPatches;

public class ThoughtWorker_Precept_SpaceHabitat_SkyIslandNeutral : ThoughtWorker_Precept_SpaceHabitat {
    protected override ThoughtState ShouldHaveThought(Pawn p) {
        if (SkyIslandsCompat.IsOnSkyIsland(p)) {
            return ThoughtState.Inactive;
        }
        return base.ShouldHaveThought(p);
    }
}
