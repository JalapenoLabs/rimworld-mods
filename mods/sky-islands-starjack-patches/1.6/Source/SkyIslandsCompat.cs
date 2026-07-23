/*************************************************************************
 * Sky Islands — Starjack Patches, a RimWorld patch mod by JalapenoLabs.
 *
 * Softens the space-adaptation downsides for pawns while they are on a Sky
 * Islands map. Sky islands float at high altitude on a child-of-orbit planet
 * layer that the Sky Islands mod marks non-space for habitability, so
 * space-adapted colonists read as fully "planetside" and take the harshest
 * penalties. These patches treat a sky island as the middle ground it is.
 *
 * Requires the Odyssey DLC and the Sky Islands mod.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using RimWorld.Planet;
using Verse;

namespace SkyIslandsStarjackPatches;

/// Single source of truth for detecting a Sky Islands map.
///
/// The Sky Islands mod's own code identifies its maps by the planet layer's
/// defName ("Avatar_SkyIslandsLayer"), so we match on that string rather than
/// taking a hard assembly reference on the mod.
public static class SkyIslandsCompat {
    public const string LayerDefName = "Avatar_SkyIslandsLayer";

    private static bool IsSkyIslandTile(PlanetTile tile) {
        return tile.Valid && tile.LayerDef != null && tile.LayerDef.defName == LayerDefName;
    }

    /// True when the pawn is on a sky island (spawned or in a parent that is).
    public static bool IsOnSkyIsland(Pawn pawn) {
        return pawn != null && pawn.SpawnedOrAnyParentSpawned && IsSkyIslandTile(pawn.Tile);
    }

    /// True when the spawned thing sits on a sky island map.
    public static bool IsOnSkyIsland(Thing thing) {
        return thing != null && thing.Spawned && thing.Map != null && IsSkyIslandTile(thing.Map.Tile);
    }
}
