using CombatExtended;
using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace RW_ModularizationWeapon.CombatExtended
{
    
    [HarmonyPatch(typeof(BipodComp))]
    internal static class BipodComp_Patcher
    {


        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(BipodComp),
            nameof(BipodComp.ResetVerbProps)
        )]
        private static bool PreBipodComp_ResetVerbProps(BipodComp __instance, Thing? source)
        {
            ModularizationWeapon? comp = source as ModularizationWeapon;
            if (comp != null)
            {
                VerbPropertiesCE? props = source.TryGetComp<CompEquippable>()?.VerbProperties?.Find(x => x is VerbPropertiesCE && x.isPrimary) as VerbPropertiesCE;
                if (props != null)
                {
                    __instance.AssignVerbProps(source, props);
                    return false;
                }
            }
            return true;
        }
    }
}
