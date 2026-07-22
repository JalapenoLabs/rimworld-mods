/*************************************************************************
 * Break Loyalty — ritual role: the ceremony's target.
 *
 * Restricts the "subject" role to prisoners who are actually unwaveringly
 * loyal, the only pawns the ceremony has any reason to act on.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using RimWorld;
using Verse;

namespace BreakLoyalty;

/// Target role for the Break Loyalty ceremony. Mirrors vanilla
/// `RitualRoleConvertee`: a role a pawn can fill directly, never an ideoligion
/// role slot, so `AppliesToRole` always declines.
public class RitualRole_LoyalPrisoner : RitualRole {
    public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget,
            LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null,
            Precept_Ritual precept = null, bool skipReason = false) {
        reason = null;

        if (!p.RaceProps.Humanlike) {
            if (!skipReason) reason = "MessageRitualRoleMustBeHumanlike".Translate(LabelCap);
            return false;
        }
        if (!p.IsPrisonerOfColony) {
            if (!skipReason) reason = "BreakLoyalty.RoleReason.MustBePrisoner".Translate();
            return false;
        }
        // Recruitable == false is exactly the "unwaveringly loyal" state. On
        // difficulties without unwavering prisoners the getter returns true for
        // everyone, so this correctly leaves the role with no candidates.
        if (p.guest == null || p.guest.Recruitable) {
            if (!skipReason) reason = "BreakLoyalty.RoleReason.MustBeUnwavering".Translate();
            return false;
        }
        return true;
    }

    public override bool AppliesToRole(Precept_Role role, out string reason,
            Precept_Ritual ritual = null, Pawn pawn = null, bool skipReason = false) {
        reason = null;
        return false;
    }
}
