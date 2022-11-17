using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;

namespace RW_ModularizationWeapon.CombatExtended
{
    internal static class CombatExtended_Command_Reload_Patcher
    {
        private static MethodInfo _PostCommand_Reload_GroupsWith = typeof(CombatExtended_Command_Reload_Patcher).GetMethod("PostCommand_Reload_GroupsWith", BindingFlags.Static | BindingFlags.NonPublic);
        private static Type CombatExtended_Command_Reload = GenTypes.GetTypeInAnyAssembly("CombatExtended.Command_Reload");
        private static Type CombatExtended_CompAmmoUser = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompAmmoUser");
        private static AccessTools.FieldRef<object, ThingComp> Command_Reload_compAmmo = null;
        private static MethodInfo CompAmmoUser_CurrentAmmo = null;

        private static void PostCommand_Reload_GroupsWith(Command_Action __instance, Gizmo other, ref bool __result)
        {
            __result = __result
                && Command_Reload_compAmmo(__instance) != null
                && Command_Reload_compAmmo(other) != null
                && CompAmmoUser_CurrentAmmo.Invoke(Command_Reload_compAmmo(__instance),null) == CompAmmoUser_CurrentAmmo.Invoke(Command_Reload_compAmmo(other), null);
        }


        public static void PatchCommand_Reload(Harmony patcher)
        {
            if (CombatExtended_Command_Reload != null && CombatExtended_CompAmmoUser != null)
            {
                CompAmmoUser_CurrentAmmo = CombatExtended_CompAmmoUser.GetMethod("get_CurrentAmmo", BindingFlags.Instance | BindingFlags.Public);
                Command_Reload_compAmmo = AccessTools.FieldRefAccess<ThingComp>(CombatExtended_Command_Reload, "compAmmo");
                MethodInfo target = CombatExtended_Command_Reload.GetMethod("GroupsWith", BindingFlags.Instance | BindingFlags.Public);
                patcher.Patch(
                    target,
                    postfix: new HarmonyMethod(_PostCommand_Reload_GroupsWith)
                    );
            }
        }
    }
}
