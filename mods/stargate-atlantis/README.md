# Rimworld Stargate Atlantis

Work-in-progress mod adding Stargate Atlantis content to RimWorld 1.6.

## Building from source

This mod is part of the [rimworld-mods monorepo](https://github.com/JalapenoLabs/rimworld-mods), which provides the shared RimWorld DLLs and build tooling. **You must clone the monorepo — not this repository directly — in order to build.**

### Prerequisites
- [.NET 9.0+](https://dotnet.microsoft.com/download)
- [Mage](https://magefile.org/) — `go install github.com/magefile/mage/mage@latest`

### Build

From the monorepo root:

```shell
mage build StargateAtlantis
```

The resulting `StargateAtlantis.dll` lands in `1.6/Assemblies/`.

## Credits

Alex Navarro — alex@jalapenolabs.io
Patreon: https://www.jalapenolabs.io/patreon
