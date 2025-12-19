
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon.UI
{
    public class Command_ModeTarget : Command_VerbTarget
    {
        public Command_ModeTarget(Command_VerbTarget original, ModularizationWeapon weapon)
        {
            Weapon              =  weapon;
            //base
            verb                =  original.verb;
            drawRadius          =  original.drawRadius;
            defaultDesc         =  original.defaultDesc;
            tutorTag            =  original.defaultDesc;
#if V13 || V14
            disabled            =  original.disabled;
#else
            disabled            =  original.Disabled;
#endif
            disabledReason      =  original.disabledReason;
            defaultDescPostfix  =  original.defaultDescPostfix;
            shrinkable          =  original.shrinkable;
            groupKey            =  original.groupKey;
            activateSound       =  original.activateSound;
            defaultLabel        =  original.defaultLabel;
            defaultIconColor    =  original.defaultIconColor;
            iconOffset          =  original.iconOffset;
            iconDrawScale       =  original.iconDrawScale;
            iconTexCoords       =  original.iconTexCoords;
            iconProportions     =  original.iconProportions;
            iconAngle           =  original.iconAngle;
            icon                =  original.icon;
            hotKey              =  original.hotKey;
        }
        public ModularizationWeapon Weapon { get; }

        public override float GetWidth(float maxWidth) => groupedVerbs(this).NullOrEmpty() ? 250 : base.GetWidth(maxWidth);

        public override bool GroupsWith(Gizmo other)
        {
            if (other is not Command_ModeTarget)
            {
                return false;
            }
            return base.GroupsWith(other);
        }

        protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
        {
            bool rightPanel = butRect.width >= butRect.height * 3;
            Rect childButton = butRect;
            if (rightPanel)
            {
                childButton.width = butRect.height;
                butRect.xMin += butRect.height;
                Widgets.DrawWindowBackgroundTutor(butRect);
            }
            GizmoResult result = base.GizmoOnGUIInt(childButton, parms);
            if (rightPanel)
            {
                // childButton = butRect;
                butRect.x += 8;
                butRect.y += 8;
                butRect.width -= 16;
                butRect.height -= 16;
                float xMin = butRect.xMax - butRect.height;
                childButton = butRect;
                childButton.xMin = xMin + 4;
                GUI.color = Weapon.CurrentModeColor;
                Widgets.DrawTextureFitted(childButton, Weapon.CurrentModeIcon, 1);

                childButton = butRect;
                childButton.xMax = xMin - 4;
                GUI.color = Color.white;
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(childButton, Weapon.CurrentModeName);

                if(Widgets.ButtonInvisible(butRect))
                {
                    ReadOnlyCollection<WeaponProperties> weaponProperties = Weapon.ProtectedProperties;
                    List<FloatMenuOption> options = new List<FloatMenuOption>(Math.Max(1, weaponProperties.Count));
                    for (int i = 0; i < options.Capacity; i++)
                    {
                        uint finalIndex = (uint)i;
                        WeaponProperties properties = weaponProperties[i];
                        options.Add(new FloatMenuOption(properties.Name,() => Weapon.CurrentMode = finalIndex));
                    }
                    if (options.Count > 1)
                    {
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }
                else
                {
                    Widgets.DrawHighlightIfMouseover(butRect);
                }
            }
            return result;
        }

        private static readonly AccessTools.FieldRef<Command_VerbTarget, List<Verb>> groupedVerbs = AccessTools.FieldRefAccess<Command_VerbTarget, List<Verb>>("groupedVerbs");

    }
}