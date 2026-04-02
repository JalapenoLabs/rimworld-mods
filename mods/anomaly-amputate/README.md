# RimWorld Anomaly Amputate

**Prototype mod that lets you surgically disarm captured anomaly entities.**

This project reuses the build infrastructure from JalapenoLabs' Electricity Meter and pivots it to Anomaly content. Gorehulks strapped to a holding platform now expose the standard medical "Add bill" interface so you can remove specific limbs while they are contained.

## Gameplay Features
- **Containment surgery bills**: Gorehulks on your holding platforms can be anesthetized and receive the vanilla *Remove Body Part* surgery.
- **Containment strength reduction**: Each amputated arm or leg drops the minimum containment strength requirement by 15, making uprisings less likely.
- **Resource throttling**: Missing limbs impose a 16% penalty per limb on bioferrite generation and Basic knowledge yields.
- **Escape lockout**: When both legs are gone a gorehulk can no longer attempt containment escapes.
- **Combat shutdown**: Removing both arms and the jaw prevents the spine-launch ranged attack.

Currently this work targets gorehulks only while the Anomaly surgery framework is proven out.

## Building from source
Requires .NET 9.0 or later and the RimWorld 1.6 assemblies (already referenced under `RimworldForCICD/Managed`).

Clone the repository inside `steamapps/common/RimWorld/Mods` and then either:

```shell
make
```

or

```shell
dotnet build .vscode
```

The resulting `AnomalyAmputate.dll` lands in `1.6/Assemblies/`.

## Credits
- Original build tooling by JalapenoLabs (MIT Licensed).
- Converted for Anomaly containment surgery experiments.
