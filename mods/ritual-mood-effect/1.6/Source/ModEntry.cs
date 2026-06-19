using HarmonyLib;
using Verse;

namespace RitualMoodEffect;

public class ModEntry : Mod {
    private static readonly Harmony HarmonyInstance =
        new Harmony("jalapenolabs.rimworld.ritualmoodeffect");

    public ModEntry(ModContentPack content) : base(content) {
        HarmonyInstance.PatchAll();
        Log.Message("[RitualMoodEffect] Loaded ritual portrait mood overlay.");
    }
}
