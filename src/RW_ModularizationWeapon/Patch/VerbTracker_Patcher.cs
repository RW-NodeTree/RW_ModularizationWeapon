using HarmonyLib;
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
        [HarmonyPrefix]
        [HarmonyPatch(
            "CreateVerbTargetCommand",
            typeof(Thing),
            typeof(Verb)
        )]
        private static void PreVerbTracker_CreateVerbTargetCommand(VerbTracker __instance, ref Thing ownerThing, Verb verb, ref ModularizationWeapon? __state)
        {
            __state = ownerThing as ModularizationWeapon;
            CompEquippable? equippable = ownerThing.TryGetComp<CompEquippable>();
            if (__state != null && equippable != null && equippable == verb.verbTracker?.directOwner as CompEquippable)
            {
                ReadOnlyCollection<(CompEquippable, Verb)> variants = __state.ChildVariantVerbsOfVerb(equippable.AllVerbs.IndexOf(verb));
                ownerThing = variants[variants.Count - 1].Item1.parent ?? ownerThing;
            }
            else
            {
                __state = null;
            }
            //if (Prefs.DevMode) Log.Message(verb + " : " + ownerThing + " : " + comp.Props.VerbIconVerbInstanceSource);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            "CreateVerbTargetCommand",
            typeof(Thing),
            typeof(Verb)
        )]
        private static void PostVerbTracker_CreateVerbTargetCommand(VerbTracker __instance, Thing ownerThing, Verb verb, ref Command_VerbTarget? __result, ModularizationWeapon? __state)
        {
            if (__result != null && __state != null)
            {
                __result.icon = (ownerThing?.Graphic?.MatSingleFor(ownerThing)?.mainTexture as Texture2D) ?? __result.icon;
                __result.iconProportions = ownerThing?.Graphic?.drawSize ?? __result.iconProportions;
                Vector2 scale = ownerThing?.Graphic?.drawSize / ownerThing?.def?.size.ToVector2() ?? Vector2.one;
                __result.iconDrawScale = Math.Max(scale.x, scale.y);
                __result.shrinkable = verb != __instance.PrimaryVerb;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            nameof(VerbTracker.ExposeData)
        )]
        private static void PreVerbTracker_ExposeData(VerbTracker __instance, ref List<Verb?>? __state)
        {
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                __state = __instance.AllVerbs;
            }
        }

        [HarmonyFinalizer]
        [HarmonyPatch(
            nameof(VerbTracker.ExposeData)
        )]
        private static void FinalVerbTracker_ExposeData(VerbTracker __instance, List<Verb?>? __state)
        {
            if (__state != null)
            {
                __instance.AllVerbs?.SortBy(x => __state.IndexOf(x));
            }
        }
    }
}
