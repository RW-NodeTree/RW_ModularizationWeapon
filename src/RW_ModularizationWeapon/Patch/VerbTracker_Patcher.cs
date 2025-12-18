using HarmonyLib;
using RW_ModularizationWeapon.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon.Patch
{
    [HarmonyPatch(typeof(VerbTracker))]
    internal static partial class VerbTracker_Patcher
    {

        [HarmonyPostfix]
        [HarmonyPatch(
            "CreateVerbTargetCommand",
            typeof(Thing),
            typeof(Verb)
        )]
        private static void PostVerbTracker_CreateVerbTargetCommand(VerbTracker __instance, Thing ownerThing, Verb verb, ref Command_VerbTarget? __result)
        {
            ModularizationWeapon? weapon = ownerThing as ModularizationWeapon;
            if (__result != null && weapon != null)
            {
                __result.icon = (ownerThing?.Graphic?.MatSingleFor(ownerThing)?.mainTexture as Texture2D) ?? __result.icon;
                __result.iconProportions = ownerThing?.Graphic?.drawSize ?? __result.iconProportions;
                Vector2 scale = ownerThing?.Graphic?.drawSize / ownerThing?.def?.size.ToVector2() ?? Vector2.one;
                __result.iconDrawScale = Math.Max(scale.x, scale.y);
                __result.shrinkable = verb != __instance.PrimaryVerb;
                if(__result.verb?.verbProps?.isPrimary ?? false)
                {
                    __result = new Command_ModeTarget(__result, weapon);
                }
            }
        }
    }
}
