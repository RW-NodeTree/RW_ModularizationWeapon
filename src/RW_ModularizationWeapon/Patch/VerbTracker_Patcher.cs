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
                if(verb != null && verb.tool == null && verb.verbProps != null && verb.verbProps.isPrimary)
                {
                    __result = new Command_ModeTarget(__result, weapon);
                }
            }
        }

        #if DEBUG
        [HarmonyPostfix]
        [HarmonyPatch(
            nameof(VerbTracker.GetVerbsCommands)
        )]
        private static void DEBUG_PostVerbTracker_GetVerbsCommands(VerbTracker __instance, ref IEnumerable<Command> __result)
        {
            __result = DEBUG_GetVerbsCommandsExceptionCatcher(__instance, __result);
        }

        private static IEnumerable<Command> DEBUG_GetVerbsCommandsExceptionCatcher(VerbTracker instance, IEnumerable<Command> commands)
        {
            int pos = 0;
            try
            {
                List<Command> result = new List<Command>();
                foreach(var command in commands)
                {
                    result.Add(command);
                    pos++;
                }
                return result;
            }
            catch(Exception ex)
            {
                throw new Exception(
                    $"VerbTracker.GetVerbsCommands throw Exception:\n" +
                    $"pos : {pos}.\n" +
                    $"instance : {instance.directOwner}\n"+
                    $"    As CompEquippable : {instance.directOwner as CompEquippable}\n"+
                    $"        Parent : {(instance.directOwner as CompEquippable)?.parent}\n"
                , ex);
            }
        }
        #endif
    }
}
