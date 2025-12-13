using CombatExtended;
using HarmonyLib;
using RW_NodeTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace RW_ModularizationWeapon.CombatExtended
{
    [HarmonyPatch(typeof(LoadoutPropertiesExtension))]
    internal static class LoadoutPropertiesExtension_Patcher
    {
        private static MethodInfo _LoadoutPropertiesExtension_TryGetComp = typeof(LoadoutPropertiesExtension_Patcher).GetMethod(nameof(LoadoutPropertiesExtension_TryGetComp), BindingFlags.Static | BindingFlags.NonPublic);


        [HarmonyTranspiler]
        [HarmonyPatch(
            "LoadWeaponWithRandAmmo"
        )]
        private static IEnumerable<CodeInstruction> LoadoutPropertiesExtension_LoadWeaponWithRandAmmo_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo TryGetComp = typeof(ThingCompUtility).GetMethod(nameof(ThingCompUtility.TryGetComp), BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(new Type[] { typeof(CompAmmoUser) });
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(TryGetComp)) yield return new CodeInstruction(OpCodes.Call, _LoadoutPropertiesExtension_TryGetComp);
                else yield return instruction;
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            "TryGenerateAmmoFor"
        )]
        private static IEnumerable<CodeInstruction> LoadoutPropertiesExtension_TryGenerateAmmoFor_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo TryGetComp = typeof(ThingCompUtility).GetMethod(nameof(ThingCompUtility.TryGetComp), BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(new Type[] { typeof(CompAmmoUser) });
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(TryGetComp)) yield return new CodeInstruction(OpCodes.Call, _LoadoutPropertiesExtension_TryGetComp);
                else yield return instruction;
            }
        }

        private static CompAmmoUser? LoadoutPropertiesExtension_TryGetComp(Thing? thing)
        {
            ModularizationWeapon? comp = thing as ModularizationWeapon;
            if (comp != null)
            {
                CompEquippable? equippable = thing.TryGetComp<CompEquippable?>();
                if (equippable != null)
                {
                    ReadOnlyCollection<(CompEquippable, Verb)> childVerbs = comp.ChildVariantVerbsOfPrimaryVerb;

                    CompAmmoUser? result = childVerbs[childVerbs.Count - 1].Item1.parent.TryGetComp<CompAmmoUser>();
                    if (result != null) return result;
                }
            }
            return thing.TryGetComp<CompAmmoUser>();
        }
    }
}
