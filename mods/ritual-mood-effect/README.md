# RimWorld Ritual Mood Effect

**Adds the colonist bar's mood / break-risk color band to colonist portraits in the Begin Ritual dialog.**

The vanilla colonist bar at the top of the screen tints each portrait gold, orange, or pulsing red when a colonist is at minor, major, or extreme mental break risk. The Begin Ritual dialog (used by Ideology rituals, Anomaly psychic rituals, and Odyssey gravship launches) draws its own portraits and omits this cue, making it easy to assign someone who is one bad thought away from snapping.

This mod restores parity by overlaying the same color band on every pawn portrait inside that dialog.

## How it works

A single Harmony postfix on `RimWorld.PawnRoleSelectionWidgetBase.DrawPawnPortraitInternal` reuses the game's own `MoodThresholdExtensions.CurrentMoodThresholdFor` and `MoodThreshold.GetColor` helpers. Because every ritual-style dialog (`Dialog_BeginRitual`, `Dialog_BeginPsychicRitual`) routes its portraits through that single base method, the patch covers all three out of the box.

The overlay respects the vanilla "Show mood on portraits" preference and skips dead, downed, or break-immune pawns, exactly as the colonist bar does.

## Building from source

Requires .NET 9.0 SDK and the shared `RimworldForCICD/Managed` assemblies referenced by the parent monorepo.

From the repository root:

```shell
mage build ritual-mood-effect
```

The resulting `RitualMoodEffect.dll` lands in `1.6/Assemblies/`.

## License

MIT. See `LICENSE`.
