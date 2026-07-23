/*************************************************************************
 * Sky Islands Minimap Fix, a RimWorld compatibility patch by JalapenoLabs.
 *
 * Dubs Mint Minimap's terrain blitter reads `def.texturePath.Length` without a
 * null check. Sky Islands terrain renders through custom graphics and leaves
 * `texturePath` null, so flying to a sky island made the minimap throw a
 * NullReferenceException every frame, which blanked the minimap and, because
 * it fires in OnGUI before the inspect pane, stopped the needs/health tabs from
 * drawing. This guards that one method so such terrain reads as blank instead.
 *
 * Applies only if Dubs Mint Minimap is present. Requires Harmony.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace SkyIslandsMinimapFix;

[StaticConstructorOnStartup]
public static class SkyIslandsMinimapFix {
    static SkyIslandsMinimapFix() {
        Type minimap = AccessTools.TypeByName("DubsMintMinimap.MainTabWindow_MiniMap");
        if (minimap == null) {
            return; // Dubs Mint Minimap not installed; nothing to patch.
        }

        MethodInfo blitter = AccessTools.Method(minimap, "Blitter", new[] { typeof(TerrainDef) });
        if (blitter == null) {
            Log.Warning("[Sky Islands Minimap Fix] Blitter(TerrainDef) not found; Dubs Mint Minimap may have changed. Skipping.");
            return;
        }

        new Harmony("jalapenolabs.rimworld.skyislandsminimapfix")
            .Patch(blitter, prefix: new HarmonyMethod(typeof(SkyIslandsMinimapFix), nameof(BlitterPrefix)));
        Log.Message("[Sky Islands Minimap Fix] Guarded Dubs Mint Minimap against null terrain texture paths.");
    }

    /// Skips the vanilla body for terrain with a null texture path, returning a
    /// blank tile color, which is exactly what the vanilla method returns for
    /// texture-less terrain that does have a non-null (empty) path.
    public static bool BlitterPrefix(TerrainDef def, ref Color __result) {
        if (def != null && def.texturePath == null) {
            __result = Color.black;
            return false;
        }
        return true;
    }
}
