using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RW_ModularizationWeapon.Patch
{
    [HarmonyPatch(typeof(Command_VerbTarget))]
    internal static class Command_VerbTarget_Patcher
    {


        [HarmonyPrefix]
        [HarmonyPatch(
            nameof(Command_VerbTarget.IconDrawColor),
            MethodType.Getter
        )]
        private static bool PreIconDrawColor(Command_VerbTarget __instance, ref Color __result)
        {
            CompEquippable? compEquippable = __instance.verb?.verbTracker?.directOwner as CompEquippable;
            ModularizationWeapon? weapon = compEquippable?.parent as ModularizationWeapon;
            if (weapon != null && weapon.GetComp<CompEquippable>() == compEquippable)
            {
                ReadOnlyCollection<(CompEquippable, Verb)> variants = weapon.ChildVariantVerbsOfVerb(compEquippable.AllVerbs.IndexOf(__instance.verb));
                Thing part = variants[variants.Count - 1].Item1.parent;
                if (part != null)
                {
                    __result = part.DrawColor;
                    return false;
                }
            }
            return true;
        }


        [HarmonyPrefix]
        [HarmonyPatch(
            nameof(Command_VerbTarget.ProcessInput)
        )]
        private static bool PreProcessInput(Command_VerbTarget __instance, Event ev)
        {
            if (ev.keyCode == KeyCode.Mouse1)
            {

                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                VerbTracker? verbTracker = __instance.verb?.verbTracker;
                if (verbTracker != null && __instance.verb?.tool == null)
                {
                    List<Verb?> verbList = verbTracker.AllVerbs;
                    if (verbList.Remove(__instance.verb))
                    {
                        verbList.Insert(0, __instance.verb);
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
