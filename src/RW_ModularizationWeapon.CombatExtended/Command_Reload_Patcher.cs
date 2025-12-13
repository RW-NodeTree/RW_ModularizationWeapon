using CombatExtended;
using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace RW_ModularizationWeapon.CombatExtended
{
    [HarmonyPatch(typeof(Command_Reload))]
    internal static class Command_Reload_Patcher
    {


        [HarmonyPostfix]
        [HarmonyPatch(
            nameof(Command_Reload.GroupsWith)
        )]
        private static void PostCommand_Reload_GroupsWith(Command_Reload __instance, Gizmo other, ref bool __result)
        {
            Command_Reload? command_Reload = other as Command_Reload;
            __result = command_Reload != null
            && __instance.compAmmo != null
            && command_Reload.compAmmo != null
            && __instance.compAmmo.CurrentAmmo == command_Reload.compAmmo.CurrentAmmo;
        }
    }
}
