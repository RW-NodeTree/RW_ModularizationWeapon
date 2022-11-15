using RimWorld;
using RW_NodeTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using Verse.AI;
using Verse.Noise;
using static HarmonyLib.Code;

namespace RW_ModularizationWeapon.UI
{
    public class CustomWeapon : Window
    {

        public CustomWeapon(Pawn pawn, CompCustomWeaponPort creaftingTable)
        {
            this.forcePause = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.soundAppear = SoundDefOf.InfoCard_Open;
            this.soundClose = SoundDefOf.InfoCard_Close;
            this.pawn = pawn;
            this.creaftingTable = creaftingTable;
            ResetSelections();
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(Math.Max(Verse.UI.screenWidth, 1024), Math.Max(Verse.UI.screenHeight, 576));
            }
        }


        public (string, CompModularizationWeapon) SelectedPartForChange
        {
            get
            {
                if (selected.Count > 0) return selected.First();
                return (null,null);
            }
            set
            {
                if (SelectedPartForChange != value)
                {
                    selected.Clear();
                    if(value.Item1 != null && value.Item2 != null) selected.Add(value);
                    ResetInfoTags();
                    ResetSelections();
                }
            }
        }

        public Thing SelectedThingForInfoCard
        {
            get
            {
                (string id, CompModularizationWeapon comp) = SelectedPartForChange;
                if (id != null && comp != null) return comp.ChildNodes[id] ?? creaftingTable.GetTargetCompModularizationWeapon();
                return creaftingTable.GetTargetCompModularizationWeapon();
            }
        }


        public StateInfoTags InfoTags
        {
            get
            {
                if(stateInfoTags == null && SelectedThingForInfoCard != null) stateInfoTags = new StateInfoTags(348, SelectedThingForInfoCard);
                return stateInfoTags;
            }
        }

        public void ResetInfoTags()
        {
            stateInfoTags = null;
        }

        public void ResetSelections()
        {
            selections.Clear();
            (string id, CompModularizationWeapon parent) = SelectedPartForChange;
            if (parent != null && id != null)
            {
                Thing OrginalPart = parent.OrginalPart(id).Thing;
                selections.Add((OrginalPart, OrginalPart?.def));
                if (parent.NodeProccesser.AllowNode(null, id))
                {
                    if(OrginalPart != null) selections.Add((default(Thing), default(ThingDef)));
                }
                else if (OrginalPart == null) selections.Clear();
                selections.AddRange(
                    from x
                    in pawn.Map.listerThings.AllThings
                    where
                        (x?.Spawned ?? false) &&
                        parent.NodeProccesser.AllowNode(x, id) &&
                        pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false)
                    select (x, x.def)
                );
            }
            else
            {
                selections.AddRange(
                    from x
                    in pawn.Map.listerThings.AllThings
                    where
                        (x?.Spawned ?? false) &&
                        ((CompModularizationWeapon)x) != null &&
                        creaftingTable.Props.filter.Allows(x) &&
                        pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false)
                    select (x, x.def)
                );
                selections.AddRange(
                    from x
                    in creaftingTable.Props.AllowsMakedDefs
                    where
                        x?.GetCompProperties<CompProperties_ModularizationWeapon>() != null &&
                        x.GetCompProperties<CompProperties_ModularizationWeapon>().allowCreateOnCraftingPort
                    select (default(Thing), x)
                );
            }

        }


        /// <summary>
        /// +-------+-------+-------+
        /// |weapon |       |       |
        /// |perview|       |       |
        /// +-------+       |       |
        /// |       |target | info  |
        /// | tree  |picker | card  |
        /// | view  |       |       |
        /// |       |       |       |
        /// |       |       |       |
        /// +-------+-------+-------+
        /// </summary>
        /// <param name="inRect"></param>
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Texture2D horizontalScrollbar = GUI.skin.horizontalScrollbar.normal.background;
            Texture2D horizontalScrollbarThumb = GUI.skin.horizontalScrollbarThumb.normal.background;
            Texture2D verticalScrollbar = GUI.skin.verticalScrollbar.normal.background;
            Texture2D verticalScrollbarThumb = GUI.skin.verticalScrollbarThumb.normal.background;


            GUI.skin.horizontalScrollbar.normal.background = TexUI.HighlightTex;
            GUI.skin.horizontalScrollbarThumb.normal.background = TexUI.HighlightTex;
            GUI.skin.verticalScrollbar.normal.background = TexUI.HighlightTex;
            GUI.skin.verticalScrollbarThumb.normal.background = TexUI.HighlightTex;


            // Widgets.Label(new Rect(0, 0, inRect.width, 36), "AssembleWeapon".Translate());
            CompModularizationWeapon weapon = creaftingTable.GetTargetCompModularizationWeapon();
            //if (weapon == null)
            //{
            //    ThingDef Gun_Test_Modularization = DefDatabase<ThingDef>.GetNamed("Gun_Test_Modularization");
            //    creaftingTable.SetTarget(Gun_Test_Modularization,this);
            //    weapon = creaftingTable.GetTargetCompModularizationWeapon();
            //    //if (Prefs.DevMode) Log.Message($"Gun_Test_Modularization : {Gun_Test_Modularization}; thing : {thing}");
            //}


            #region weaponPerview
            GUI.color = _Border;
            Widgets.DrawBox(new Rect(0, 0, 350, 350));
            GUI.color = Color.white;
            //Log.Message($"draw size : {weapon.NodeProccesser.GetAndUpdateDrawSize(weapon.parent.def.defaultPlacingRot)}");
            if (Widgets.ButtonInvisible(new Rect(7, 7, 336, 336)))
            {
                Widgets.DrawHighlightSelected(new Rect(7, 7, 336, 336));
                SelectedPartForChange = (null, null);
            }
            else Widgets.DrawHighlightIfMouseover(new Rect(7, 7, 336, 336));
            if (weapon != null)
            {
                Rot4 defaultPlacingRot = weapon.parent.def.defaultPlacingRot;
                Vector2 drawSize = weapon.NodeProccesser.GetAndUpdateDrawSize(defaultPlacingRot);
                if (defaultPlacingRot.IsHorizontal) drawSize = drawSize.Rotated();
                IntVec2 before = weapon.parent.def.size;
                weapon.parent.def.size = new IntVec2((int)Math.Ceiling(drawSize.x), (int)Math.Ceiling(drawSize.y));
                Widgets.ThingIcon(new Rect(7, 7, 336, 336), weapon);
                weapon.parent.def.size = before;
            }
            else
            {
                Widgets.DrawBoxSolid(new Rect(71, 170, 208, 10), Color.gray);
                Widgets.DrawBoxSolid(new Rect(170, 71, 10, 208), Color.gray);
            }
            Text.Font = GameFont.Small;
            #endregion


            #region treeView
            Vector2 ScrollViewSize = weapon?.TreeViewDrawSize(new Vector2(284, 48)) ?? Vector2.zero;
            ScrollViewSize.x = Math.Max(ScrollViewSize.x, ScrollViewSize.y < inRect.height - 360 ? 348 : 332);
            //if (Prefs.DevMode) Log.Message($"creaftingTable.GetTargetCompModularizationWeapon() : {creaftingTable.GetTargetCompModularizationWeapon()}; ScrollViewSize : {ScrollViewSize}");
            GUI.color = _Border;
            Widgets.DrawBox(new Rect(0, 358, 350,inRect.height - 358));
            GUI.color = Color.white;
            Widgets.BeginScrollView(
                new Rect(1, 359, 348, inRect.height - 360),
                ref ScrollViews[0],
                new Rect(Vector2.zero, ScrollViewSize)
            );
            weapon?.DrawChildTreeView(
                Vector2.zero,
                ScrollViews[0].y,
                48,
                ScrollViewSize.x,
                ScrollViewSize.y,
                (string id,Thing part, CompModularizationWeapon Parent)=>
                {
                    if (SelectedPartForChange == (id, Parent))
                        SelectedPartForChange = (null, null);
                    else
                        SelectedPartForChange = (id, Parent);
                },
                (string id,Thing part, CompModularizationWeapon Parent)=>
                {
                    if (SelectedPartForChange == (id, Parent))
                        SelectedPartForChange = (null, null);
                    else
                        SelectedPartForChange = (id, Parent);
                },
                (string id, Thing part, CompModularizationWeapon Parent) =>
                {
                    if (SelectedPartForChange == (id, Parent))
                        SelectedPartForChange = (null, null);
                    else
                        SelectedPartForChange = (id, Parent);
                },
                selected
            );
            Widgets.EndScrollView();
            //Widgets.EndGroup();
            #endregion


            #region targetPicker
            //Widgets.DrawBoxSolid(new Rect(350, 0, inRect.width - 700, inRect.height), Color.black);
            ScrollViewSize = new Vector2(inRect.width - 700, selections.Count * 48);
            ScrollViewSize.x = ScrollViewSize.y < inRect.height ? ScrollViewSize.x : ScrollViewSize.x - GUI.skin.verticalScrollbar.fixedWidth;
            Widgets.BeginScrollView(
                new Rect(350, 0, inRect.width - 700, inRect.height),
                ref ScrollViews[1],
                new Rect(Vector2.zero, ScrollViewSize)
            );

            (string idForChange, CompModularizationWeapon partForChange) = SelectedPartForChange;
            for(int i = 0; i < selections.Count; i++)
            {
                Rect rect = new Rect(0, i * 48, ScrollViewSize.x, 48);
                if (rect.y + 48 >= ScrollViews[1].y && rect.y <= ScrollViews[1].y + inRect.height)
                {
                    (Thing selThing, ThingDef selDef) = selections[i];

                    CompModularizationWeapon comp = selThing;
                    if (partForChange != null && idForChange != null)
                    {
                        if (partForChange.ChildNodes[idForChange] == selThing) Widgets.DrawBoxSolidWithOutline(rect, new Color32(51, 153, 255, 64), new Color32(51, 153, 255, 96));
                        Widgets.DrawHighlightIfMouseover(rect);//hover
                        if (selThing != null)
                        {
                            ThingStyleDef styleDef = selThing.StyleDef;
                            CompChildNodeProccesser comp_targetModeParent = (selDef.graphicData != null && (styleDef == null || styleDef.UIIcon == null) && selDef.uiIconPath.NullOrEmpty() && !(selThing is Pawn || selThing is Corpse)) ? comp?.ParentProccesser : null;
                            if (comp_targetModeParent != null)
                            {
                                selThing.holdingOwner = null;
                                comp.NodeProccesser.ResetRenderedTexture();
                            }
                            Widgets.ThingIcon(new Rect(1, rect.y + 1, 46, 46), selThing);
                            if (comp_targetModeParent != null)
                            {
                                selThing.holdingOwner = comp_targetModeParent.ChildNodes;
                                comp.NodeProccesser.ResetRenderedTexture();
                            }
                            Widgets.Label(new Rect(48, rect.y + 1, rect.width - 49, rect.height - 2), selThing.Label);
                            if(Widgets.ButtonInvisible(rect))
                            {
                                partForChange.SetTargetPart(idForChange, selThing);
                                ResetInfoTags();
                            }
                        }
                        else if(selDef == null)
                        {
                            Widgets.DrawTextureFitted(new Rect(1, rect.y + 1, 46, 46), partForChange.Props.WeaponAttachmentPropertiesById(idForChange).UITexture,1);
                            Widgets.Label(new Rect(48, rect.y + 1, rect.width - 49, rect.height - 2), "setEmpty".Translate());
                            if (Widgets.ButtonInvisible(rect))
                            {
                                partForChange.SetTargetPart(idForChange, null);
                                ResetInfoTags();
                            }
                        }
                    }
                    else
                    {
                        if (weapon?.parent == selThing) Widgets.DrawBoxSolidWithOutline(rect, new Color32(51, 153, 255, 64), new Color32(51, 153, 255, 96));
                        Widgets.DrawHighlightIfMouseover(rect);//hover
                        if (selThing != null)
                        {
                            Widgets.ThingIcon(new Rect(1, rect.y + 1, 46, 46), selThing);
                            Widgets.Label(new Rect(48, rect.y + 1, rect.width - 49, rect.height - 2), selThing.Label);
                            if (Widgets.ButtonInvisible(rect))
                            {
                                creaftingTable.SetTarget(selThing, this);
                                ResetInfoTags();
                            }
                        }
                        else if (selDef != null)
                        {
                            Widgets.ThingIcon(new Rect(1, rect.y + 1, 46, 46), selDef);
                            Widgets.Label(new Rect(48, rect.y + 1, rect.width - 49, rect.height - 2), selDef.label);
                            if (Widgets.ButtonInvisible(rect))
                            {
                                creaftingTable.SetTarget(selDef, this);
                                ResetInfoTags();
                            }
                        }
                    }
                }

            }

            Widgets.EndScrollView();
            #endregion


            #region infoCard
            GUI.color = _Border;
            Widgets.DrawBox(new Rect(inRect.width - 350, 0, 350, inRect.height - 48));
            GUI.color = Color.white;
            InfoTags?.Draw(new Rect(inRect.width - 349, 1, 348, inRect.height - 50));
            #endregion

            if(Widgets.ButtonText(new Rect(inRect.width - 350, inRect.height - 48, 350,48), "Apply"))
            {
                Close(false);
                if(weapon != null)
                {
                    //weapon.ApplyTargetPart(pawn.Position,pawn.Map);
                    //if (!weapon.parent.Spawned) GenPlace.TryPlaceThing(weapon.parent, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                    Job job = new Job(DefDatabase<JobDef>.GetNamed("ModularizationWeapon_Apply"), creaftingTable.parent);
                    pawn.jobs.EndCurrentJob(JobCondition.Incompletable, false, false);
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
            }

            GUI.skin.horizontalScrollbar.normal.background = horizontalScrollbar;
            GUI.skin.horizontalScrollbarThumb.normal.background = horizontalScrollbarThumb;
            GUI.skin.verticalScrollbar.normal.background = verticalScrollbar;
            GUI.skin.verticalScrollbarThumb.normal.background = verticalScrollbarThumb;
        }


        public override void Close(bool doCloseSound = true)
        {
            CompModularizationWeapon weapon = creaftingTable.GetTargetCompModularizationWeapon();
            if (weapon != null)
            {
                weapon.ShowTargetPart = false;
            }
            base.Close(doCloseSound);
        }

        private static readonly Color32 _Border = new Color32(97, 108, 122, 255);
        private readonly Pawn pawn;
        private readonly CompCustomWeaponPort creaftingTable;
        private readonly HashSet<(string, CompModularizationWeapon)> selected = new HashSet<(string, CompModularizationWeapon)>();
        private readonly List<(Thing,ThingDef)> selections = new List<(Thing, ThingDef)>();
        private Vector2[] ScrollViews = new Vector2[2];
        private StateInfoTags stateInfoTags = null;
    }
}
