using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon.Patch
{
    [HarmonyPatch(typeof(ThingWithComps))]
    internal static partial class Thing_Patcher
    {

        [HarmonyPrefix]
        [HarmonyPatch(
            nameof(ThingWithComps.InitializeComps)
        )]
        private static void PreThing_InitializeComps(ThingWithComps __instance, ref List<CompProperties>? __state)
        {
            __state = __instance.def.comps;
            __instance.def.comps = ModularizationWeapon.CompPropertiesFromThing(__instance);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            nameof(ThingWithComps.InitializeComps)
        )]
        private static IEnumerable<CodeInstruction> TranspileThing_InitializeComps(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo comps = typeof(ThingWithComps).GetField("comps", BindingFlags.Instance | BindingFlags.NonPublic)!;
            MethodInfo RestoreComps = typeof(ModularizationWeapon).GetMethod(nameof(ModularizationWeapon.RestoreComps), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;
            foreach (CodeInstruction instruction in instructions)
            {
                if(instruction.StoresField(comps))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, comps);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, RestoreComps);
                }
                yield return instruction;
            }
        }

        [HarmonyFinalizer]
        [HarmonyPatch(
            nameof(ThingWithComps.InitializeComps)
        )]
        private static void FinalThing_InitializeComps(ThingWithComps __instance, ref List<CompProperties>? __state)
        {
            __instance.def.comps = __state;
        }
    }
}
