using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon.Patch
{
    [HarmonyPatch(typeof(ThingDef))]
    internal static partial class ThingDef_Patcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(
            nameof(ThingDef.SpecialDisplayStats),
            typeof(StatRequest)
            )]
        private static void PostThingDef_SpecialDisplayStats(ThingDef __instance, StatRequest req, ref IEnumerable<StatDrawEntry> __result)
        {
            __result = SpecialDisplayStats(__result, __instance, req);
        }

        private static IEnumerable<StatDrawEntry> SpecialDisplayStats(IEnumerable<StatDrawEntry> originen, ThingDef def, StatRequest req)
        {
            foreach (StatDrawEntry entry in originen)
            {
                yield return entry;
            }
            ModularizationWeaponExtension? weaponExtension = def.GetModExtension<ModularizationWeaponExtension>();
            if (weaponExtension != null)
            {
                foreach (StatDrawEntry entry in weaponExtension.SpecialDisplayStats(req))
                {
                    yield return entry;
                }
            }
        }
    }
}
