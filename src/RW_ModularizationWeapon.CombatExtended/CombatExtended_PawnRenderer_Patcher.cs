using CombatExtended;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon.CombatExtended
{
    // When version <= 1.4
    internal static class CombatExtended_PawnRenderer_Patcher
    {
        private static MethodInfo _PerHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh = typeof(CombatExtended_PawnRenderer_Patcher).GetMethod(nameof(PerHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        private static MethodInfo _FinalHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh = typeof(CombatExtended_PawnRenderer_Patcher).GetMethod(nameof(FinalHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        private static Type Harmony_PawnRenderer_DrawEquipmentAiming = GenTypes.GetTypeInAnyAssembly("CombatExtended.HarmonyCE.Harmony_PawnRenderer")?.GetNestedType("Harmony_PawnRenderer_DrawEquipmentAiming", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public) ?? GenTypes.GetTypeInAnyAssembly("CombatExtended.HarmonyCE.Harmony_PawnRenderer_DrawEquipmentAiming");


        private static void PerHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh(
            Thing? eq,
            ref (GunDrawExtension?, Vector2) __state)
        {
            ModularizationWeapon? comp = eq as ModularizationWeapon;
            if (comp != null && eq != null)
            {
                eq.def.modExtensions = eq.def.modExtensions ?? new List<DefModExtension>();
                GunDrawExtension? targetExtension = eq.def.GetModExtension<GunDrawExtension>();
                if (targetExtension == null)
                {
                    targetExtension = new GunDrawExtension();
                    targetExtension.DrawSize = eq.def.graphicData.drawSize;
                    eq.def.modExtensions.Add(targetExtension);
                }
                __state = (targetExtension, targetExtension.DrawSize);
                targetExtension.DrawSize = eq.Graphic.drawSize;
            }
        }

        private static void FinalHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh((GunDrawExtension?, Vector2) __state)
        {
            (GunDrawExtension? extension, Vector2 drawSize) = __state;
            if (extension != null)
            {
                extension.DrawSize = drawSize;
            }
        }


        public static void PatchDrawMesh(Harmony patcher)
        {
            //Log.Message($"Try Patch CE; Harmony_PawnRenderer_DrawEquipmentAiming={Harmony_PawnRenderer_DrawEquipmentAiming}; GunDrawExtension={GunDrawExtension}");
            if (Harmony_PawnRenderer_DrawEquipmentAiming != null)
            {
                //Log.Message("Patching CE");
                MethodInfo target = Harmony_PawnRenderer_DrawEquipmentAiming.GetMethod("DrawMesh", BindingFlags.Static | BindingFlags.NonPublic);
                patcher.Patch(
                    target,
                    new HarmonyMethod(_PerHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh),
                    null,
                    null,
                    new HarmonyMethod(_FinalHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh)
                    );
            }
        }
    }
}
