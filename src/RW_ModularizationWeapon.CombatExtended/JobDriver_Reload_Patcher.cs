using CombatExtended;
using HarmonyLib;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Verse;
using Verse.AI;

namespace RW_ModularizationWeapon.CombatExtended
{
    [HarmonyPatch(typeof(JobDriver_Reload))]
    internal static class JobDriver_Reload_Patcher
    {

        
        [HarmonyPostfix]
        [HarmonyPatch(
            "get_weapon"
        )]
        private static void PostJobDriver_Reload_weapon(ref ThingWithComps? __result)
        {
            StackTrace stackTrace = new StackTrace();
            string name = stackTrace.GetFrame(1).GetMethod().Name;
            if(name == "get_compReloader" || name == "GetReport") return;
            ModularizationWeapon? weapon = __result?.RootNode() as ModularizationWeapon;
            __result = weapon ?? __result;
        }
    }
}
