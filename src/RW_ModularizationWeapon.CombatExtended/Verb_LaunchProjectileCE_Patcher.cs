using CombatExtended;
using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace RW_ModularizationWeapon.CombatExtended
{
    [HarmonyPatch(typeof(Verb_LaunchProjectileCE))]
    internal static class Verb_LaunchProjectileCE_Patcher
    {

        [HarmonyPostfix]
        [HarmonyPatch(
            nameof(Verb_LaunchProjectileCE.ShotsPerBurst),
            MethodType.Getter
        )]
        private static void PostVerb_LaunchProjectileCE_ShotsPerBurst(Verb_LaunchProjectileCE __instance, ref int __result)
        {
            ModularizationWeapon? comp = (__instance.verbTracker?.directOwner as CompEquippable)?.parent as ModularizationWeapon;
            if (comp != null)
            {
                __result = __instance.verbProps.burstShotCount;
                //Log.Message($"log {__instance}.ShotsPerBurst = {__result}");
            }
        }
    }
}
