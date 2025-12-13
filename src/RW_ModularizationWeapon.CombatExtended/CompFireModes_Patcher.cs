using CombatExtended;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace RW_ModularizationWeapon.CombatExtended
{
    [HarmonyPatch(typeof(CompFireModes))]
    internal static class CompFireModes_Patcher
    {


        [HarmonyPrefix]
        [HarmonyPatch(
            "get_Verb"
        )]
        private static bool PreCompFireModes_Verb(CompFireModes __instance, ref Verb? __result)
        {
            List<CompEquippable> comps = new List<CompEquippable>();
            ModularizationWeapon? comp = __instance.parent as ModularizationWeapon;
            while (comp != null)
            {
                CompEquippable? compEquippable = comp.TryGetComp<CompEquippable>();
                if(compEquippable != null) comps.Add(compEquippable);
                comp = comp.ParentPart;
            }
            for (int i = comps.Count - 1; i >= 0; i--)
            {
                List<Verb?>? verbs = comps[i]?.AllVerbs;
                if (verbs != null)
                {
                    foreach (Verb? verb in verbs)
                    {
                        if (verb != null && verb.verbProps.isPrimary && verb.EquipmentSource == __instance.parent)
                        {
                            __result = verb;
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
