# Sky Islands Minimap Fix

Fixes a crash where **Dubs Mint Minimap** throws a `NullReferenceException` on
**Sky Islands** terrain.

## The bug

Dubs Mint Minimap's `MainTabWindow_MiniMap.Blitter(TerrainDef)` reads
`def.texturePath.Length` with no null check. Sky Islands terrain renders through
custom graphics and leaves `texturePath` null, so flying to a sky island blanked
the minimap and threw every frame. Because that fires in `OnGUI` before the
inspect pane draws, it also stopped the needs and health tabs from rendering.

## The fix

A one-method Harmony prefix: when a terrain's `texturePath` is null, return a
blank tile color instead of dereferencing it. This is exactly the result the
vanilla method already returns for texture-less terrain with a non-null path, so
nothing else changes, and it does not affect how terrain looks in the world.

Applies only when Dubs Mint Minimap is installed (reached by reflection, so no
compile-time coupling). Requires Harmony.

## Building

```shell
mage build sky-islands-minimap-fix
```
