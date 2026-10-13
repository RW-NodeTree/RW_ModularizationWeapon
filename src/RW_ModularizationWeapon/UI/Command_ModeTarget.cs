
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon.UI
{
    public class Command_ModeTarget : Command_VerbTarget
    {
        static Command_ModeTarget()
        {
            ParameterExpression a = Expression.Parameter(typeof(Command_VerbTarget));
            ParameterExpression b = Expression.Parameter(typeof(Command_VerbTarget));
            FieldInfo[] fieldInfos = typeof(Command_VerbTarget).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            BinaryExpression[] binaryExpressions = new BinaryExpression[fieldInfos.Length];
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                FieldInfo fieldInfo = fieldInfos[i];
                MemberExpression a_value = Expression.Field(a, fieldInfo);
                MemberExpression b_value = Expression.Field(b, fieldInfo);
                binaryExpressions[i] = Expression.Assign(a_value, b_value);
            }
            CopyData = Expression.Lambda<Action<Command_VerbTarget, Command_VerbTarget>>(Expression.Block(binaryExpressions), a, b).Compile();
        }

        public Command_ModeTarget(Command_VerbTarget original) : base()
        {
            CopyData(this, original);
        }

        public ModularizationWeapon Weapon => (ModularizationWeapon)verb.EquipmentCompSource.parent;

        public override float GetWidth(float maxWidth) => 250;

        public override bool GroupsWith(Gizmo other)
        {
            if (
                other is Command_ModeTarget target &&
                target.Weapon == Weapon
            )
            {
                return base.GroupsWith(other);
            }
            return false;
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
                ModularizationWeapon weapon = Weapon;

                butRect.x += 8;
                butRect.y += 8;
                butRect.width -= 16;
                butRect.height -= 16;
                float xMin = butRect.xMax - butRect.height;
                childButton = butRect;
                childButton.xMin = xMin + 4;
                GUI.color = weapon.CurrentModeColor;
                Widgets.DrawTextureFitted(childButton, weapon.CurrentModeIcon, 1);

                childButton = butRect;
                childButton.xMax = xMin - 4;
                GUI.color = Color.white;
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(childButton, weapon.CurrentModeName);

                if(Widgets.ButtonInvisible(butRect))
                {
                    ReadOnlyCollection<CompProperties_ModularizationWeaponEquippable> weaponProperties = weapon.InheritableProperties;
                    List<FloatMenuOption> options = new List<FloatMenuOption>(weaponProperties.Count);
                    for (int i = 0; i < options.Capacity; i++)
                    {
                        uint finalIndex = (uint)i;
                        CompProperties_ModularizationWeaponEquippable properties = weaponProperties[i];
                        options.Add(new FloatMenuOption(properties.Name,() => weapon.CurrentMode = finalIndex));
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

        private static readonly Action<Command_VerbTarget, Command_VerbTarget> CopyData;

    }
}