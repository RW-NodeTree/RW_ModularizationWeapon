using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon.Patch
{
    [HarmonyPatch(typeof(CompEquippable))]
    internal static partial class CompEquippable_Patcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(
            nameof(CompEquippable.VerbProperties),
            MethodType.Getter
        )]
        private static bool PreCompEquippable_GetVerbProperties(CompEquippable __instance, ref List<VerbProperties> __result)
        {
            if (__instance.parent.GetComp<CompEquippable>() != __instance)
            {
                return true;
            }
            __result = ModularizationWeapon.VerbPropertiesFromThing(__instance.parent);
            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(
            nameof(CompEquippable.Tools),
            MethodType.Getter
        )]
        private static bool PreCompEquippable_GetTools(CompEquippable __instance, ref List<Tool> __result)
        {
            if (__instance.parent.GetComp<CompEquippable>() != __instance)
            {
                return true;
            }
            __result = ModularizationWeapon.ToolsFromThing(__instance.parent);
            return false;
        }
    }
}
