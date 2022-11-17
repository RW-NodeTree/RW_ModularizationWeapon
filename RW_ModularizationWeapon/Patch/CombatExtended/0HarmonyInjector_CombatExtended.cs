using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_ModularizationWeapon.CombatExtended
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInjector_CombatExtended
    {
        static HarmonyInjector_CombatExtended()
        {
            CombatExtended_Command_Reload_Patcher.PatchCommand_Reload(patcher);
        }

        public static Harmony patcher = new Harmony("RW_ModularizationWeapon.Patch.CombatExtended");
    }
}
