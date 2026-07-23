/*************************************************************************
 * Willing Hands — dev tools.
 *
 * The natural path (15 days of sustained comfort and mood, then a small daily
 * roll) is slow to reach on purpose, which makes it hard to test. These debug
 * actions expose the mechanic on demand. They appear under a "Willing Hands"
 * category in the debug menu (God-mode / dev actions) when Ideology is active.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using LudeonTK;
using RimWorld;
using Verse;

namespace WillingHands;

public static class WillingHandsDebugActions {
    private const string Category = "Willing Hands";

    [DebugAction(Category, "Force join request",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap,
        requiresIdeology = true)]
    private static void ForceJoinRequest(Pawn p) {
        if (!Require(p, out MapComponent_SlaveBelonging tracker)) {
            return;
        }
        tracker.DebugForceJoinRequest(p);
    }

    [DebugAction(Category, "Settle slave (fill belonging)",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap,
        requiresIdeology = true)]
    private static void SettleSlave(Pawn p) {
        if (!Require(p, out MapComponent_SlaveBelonging tracker)) {
            return;
        }
        tracker.DebugSettle(p);
        Messages.Message(p.LabelShortCap + " is now settled; they may ask to join on an upcoming check.",
            p, MessageTypeDefOf.TaskCompletion, historical: false);
    }

    [DebugAction(Category, "Log belonging state",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap,
        requiresIdeology = true)]
    private static void LogBelongingState(Pawn p) {
        if (!Require(p, out MapComponent_SlaveBelonging tracker)) {
            return;
        }
        Log.Message(tracker.DebugStateFor(p));
    }

    /// Shared guard: the target must be a colony slave on a mapped tile.
    private static bool Require(Pawn p, out MapComponent_SlaveBelonging tracker) {
        tracker = null;
        if (p?.Map == null || !p.IsSlaveOfColony) {
            Messages.Message("Willing Hands: pick a slave of the colony.",
                MessageTypeDefOf.RejectInput, historical: false);
            return false;
        }
        tracker = p.Map.GetComponent<MapComponent_SlaveBelonging>();
        return tracker != null;
    }
}
