using CombatExtended;
using HarmonyLib;
using RW_NodeTree.Tools;
using System;
using System.Reflection;
using Verse;

namespace RW_ModularizationWeapon.CombatExtended
{
    [HarmonyPatch(typeof(CompAmmoUser))]
    internal static class CompAmmoUser_Patcher
    {


        [HarmonyPostfix]
        [HarmonyPatch(
            nameof(CompAmmoUser.CompEquippable),
            MethodType.Getter
        )]
        private static void PostCompAmmoUser_CompEquippable(CompAmmoUser __instance, ref CompEquippable? __result)
        {
            ModularizationWeapon? comp = __instance.parent.RootNode() as ModularizationWeapon;
            if (comp != null)
            {
                __result = comp.TryGetComp<CompEquippable>();
                //Log.Message($"log {__instance}.PostCompAmmoUser_CompEquippable");
            }
        }
    }
}
