using HarmonyLib;
using Verse;

namespace RW_ModularizationWeapon.CombatExtended
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInjector
    {
        static HarmonyInjector()
        {
            patcher.PatchAll();
        }

        public static Harmony patcher = new Harmony("RW_ModularizationWeapon.Patch.CombatExtended");
    }
}
