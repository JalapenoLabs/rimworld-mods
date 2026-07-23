/*************************************************************************
 * Sky Islands — Starjack Patches: low-gravity movement penalty.
 *
 * The "low gravity adapted" gene (MoveSpeed_Space) applies its full planetside
 * penalty (-0.2 move, -0.1 work) via ConditionalStatAffecter_NotInSpace, which
 * fires on a sky island because the layer is non-space. This affecter is added
 * to that gene with counteracting positive offsets, so on a sky island the two
 * combine into a softened half-penalty. It contributes nothing anywhere else.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using RimWorld;
using Verse;

namespace SkyIslandsStarjackPatches;

public class ConditionalStatAffecter_OnSkyIsland : ConditionalStatAffecter {
    public override string Label => "SkyIslandsStarjack.OnSkyIsland".Translate();

    public override bool Applies(StatRequest req) {
        return req.HasThing && SkyIslandsCompat.IsOnSkyIsland(req.Thing);
    }
}
