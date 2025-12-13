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
            CombatExtended_PawnRenderer_Patcher.PatchDrawMesh(patcher);
        }

        public static Harmony patcher = new Harmony("RW_ModularizationWeapon.Patch.CombatExtended");
    }
}
