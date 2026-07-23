/*************************************************************************
 * Break Loyalty — cooldown-ready chime.
 *
 * The ritual's cooldown lives on Precept_Ritual.abilityOnCooldownUntilTick.
 * The vanilla "sendMessageOnCooldownComplete" flag only fires for pawn
 * abilities, not ritual cooldowns, so this watches the ritual and posts a
 * small message the moment it becomes available again.
 *
 * Auto-instantiated by the game (all GameComponent subclasses are).
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using RimWorld;
using Verse;

namespace BreakLoyalty;

public class GameComponent_BreakLoyaltyCooldown : GameComponent {
    // Sampled ~every 4s at 1x speed; the cooldown is in the range of days, so
    // this catches the transition promptly without ticking work every frame.
    private const int CheckInterval = 250;

    private bool wasOnCooldown;

    public GameComponent_BreakLoyaltyCooldown(Game game) { }

    public override void GameComponentTick() {
        if (Find.TickManager.TicksGame % CheckInterval != 0 || !ModsConfig.IdeologyActive) {
            return;
        }

        Precept_Ritual ritual = BreakLoyaltyUtility.FindRitual();
        if (ritual == null) {
            return;
        }

        bool onCooldown = ritual.abilityOnCooldownUntilTick > Find.TickManager.TicksGame;
        // Fire only on the observed on -> off edge, so loading a save whose
        // cooldown already expired never chimes spuriously.
        if (wasOnCooldown && !onCooldown) {
            Messages.Message("BreakLoyalty.CooldownReady".Translate(),
                MessageTypeDefOf.NeutralEvent, historical: false);
        }
        wasOnCooldown = onCooldown;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref wasOnCooldown, "wasOnCooldown", defaultValue: false);
    }
}
