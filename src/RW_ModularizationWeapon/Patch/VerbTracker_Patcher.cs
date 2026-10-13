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
                #if V13 || V14 || V15
                __result.icon = (ownerThing?.Graphic?.MatSingleFor(ownerThing)?.mainTexture as Texture2D) ?? __result.icon;
                __result.iconProportions = ownerThing?.Graphic?.drawSize ?? __result.iconProportions;
                Vector2 scale = ownerThing?.Graphic?.drawSize / ownerThing?.def?.size.ToVector2() ?? Vector2.one;
                __result.iconDrawScale = Math.Max(scale.x, scale.y);
                __result.shrinkable = verb != __instance.PrimaryVerb;
                #endif
                __result = new Command_ModeTarget(__result);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            nameof(VerbTracker.GetVerbsCommands)
        )]
        private static void PreVerbTracker_GetVerbsCommands(VerbTracker __instance, ref (ModularizationWeapon?, List<Tool>?, List<VerbProperties>?) __state)
        {
            __state.Item1 = (__instance.directOwner as CompEquippable)?.parent as ModularizationWeapon;
            if (__state.Item1 != null)
            {
                __state.Item2 = __state.Item1.def.tools;
                __state.Item1.def.tools = __instance.directOwner.Tools;
                __state.Item3 = ThingDef_verbs(__state.Item1.def);
                ThingDef_verbs(__state.Item1.def) = __instance.directOwner.VerbProperties;
            }
        }

        [HarmonyFinalizer]
        [HarmonyPatch(
            nameof(VerbTracker.GetVerbsCommands)
        )]
        private static void FinalVerbTracker_GetVerbsCommands(ref (ModularizationWeapon?, List<Tool>?, List<VerbProperties>?) __state, ref IEnumerable<Command> __result)
        {
            if (__state.Item1 != null)
            {
                __result = new List<Command>(__result);
                __state.Item1.def.tools = __state.Item2;
                ThingDef_verbs(__state.Item1.def) = __state.Item3;
            }
        }

        internal static readonly AccessTools.FieldRef<ThingDef, List<VerbProperties>?> ThingDef_verbs = AccessTools.FieldRefAccess<ThingDef, List<VerbProperties>?>("verbs");
    }
}
