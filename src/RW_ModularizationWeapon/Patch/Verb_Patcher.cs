using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon.Patch
{
    [HarmonyPatch(typeof(Verb))]
    internal static partial class Verb_Patcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(
            nameof(Verb.EquipmentSource),
            MethodType.Getter
        )]
        private static void PostVerb_GetEquipmentSource(Verb __instance, ref ThingWithComps? __result)
        {
            ModularizationWeapon? weapon = __result as ModularizationWeapon;
            CompEquippable? equippable = __result?.GetComp<CompEquippable>();
            if (weapon != null && equippable != null && equippable == __instance.verbTracker?.directOwner as CompEquippable)
            {
                ReadOnlyCollection<(CompEquippable, Verb)> variants = weapon.ChildVariantVerbsOfVerb(equippable.AllVerbs.IndexOf(__instance));
                __result = variants[variants.Count - 1].Item1.parent ?? __result;
            }
            //if (Prefs.DevMode) Log.Message(verb + " : " + ownerThing + " : " + comp.Props.VerbIconVerbInstanceSource);
        }


        [HarmonyPostfix]
        [HarmonyPatch(
            nameof(Verb.UIIcon),
            MethodType.Getter
        )]
        private static void PostVerb_UIIcon(Verb __instance, ref Texture2D __result)
        {
            ThingWithComps? equipmentSource = __instance.EquipmentSource;
            if(equipmentSource != null && __result == equipmentSource.def.uiIcon)
            {
                __result = (equipmentSource.Graphic?.MatSingleFor(equipmentSource)?.mainTexture as Texture2D) ?? __result;
            }
        }
    }
}
