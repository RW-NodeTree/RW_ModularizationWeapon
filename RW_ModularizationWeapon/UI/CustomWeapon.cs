using RimWorld;
using RW_NodeTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using Verse;
using Verse.AI;
using Verse.Noise;

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
            this.optionalTitle = "ModifyWeapon".Translate();
            ResetSelections();
        }


        protected override float Margin => 4;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(1024 + Margin * 2, Math.Max(Verse.UI.screenHeight, 576));
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


        private StateInfoTags InfoTags
        {
            get
            {
                if (stateInfoTags == null && SelectedThingForInfoCard != null)
                {
                    stateInfoTags = new StateInfoTags(348, SelectedThingForInfoCard);
                }
                return stateInfoTags;
            }
        }

        internal void ResetInfoTags()
        {
            stateInfoTags = null;
        }

        internal void ResetSelections()
        {
            selections.Clear();
            (string id, CompModularizationWeapon parent) = SelectedPartForChange;
            if (parent != null && id != null)
            {
                Thing OrginalPart = targetMode ? parent.GetTargetPart(id).Thing : parent.ChildNodes[id];
                selections.Add((OrginalPart, OrginalPart?.def));
                if (parent.AllowPart(null, id))
                {
                    if(OrginalPart != null) selections.Add((default(Thing), default(ThingDef)));
                }
                else if (OrginalPart == null) selections.Clear();
                selections.AddRange(
                    from x
                    in pawn.Map.listerThings.AllThings
                    where
                        (x?.Spawned ?? false) &&
                        parent.AllowPart(x, id) &&
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
        /// ╔═══════════════╤═══════╗
        /// ║               │       ║
        /// ║    weapon     │       ║
        /// ║    perview    │       ║
        /// ║               │ info  ║
        /// ╟───────┬───────┤ card  ║
        /// ║ tree  │target │       ║
        /// ║ view  │picker │       ║
        /// ║       │       │       ║
        /// ╚═══════╧═══════╧═══════╝
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

            //stopwatch.Restart();
            //long[] spans = new long[5];
            // Widgets.Label(new Rect(0, 0, inRect.width, 36), "AssembleWeapon".Translate());
            CompModularizationWeapon weapon = creaftingTable.GetTargetCompModularizationWeapon();
            weapon?.SwapTargetPart();
            targetMode = !targetMode;
            //if (weapon != null)
            //{
            //    weapon.SwapTargetPart();
            //spans[0] = stopwatch.ElapsedTicks;
            //weapon.NodeProccesser.UpdateNode();
            //spans[1] = stopwatch.ElapsedTicks;
            //Log.Message($"weapon.UsingTargetPart : {weapon.UsingTargetPart}");
            //}
            //if (weapon == null)
            //{
            //    ThingDef Gun_Test_Modularization = DefDatabase<ThingDef>.GetNamed("Gun_Test_Modularization");
            //    creaftingTable.SetTarget(Gun_Test_Modularization,this);
            //    weapon = creaftingTable.GetTargetCompModularizationWeapon();
            //    //if (Prefs.DevMode) Log.Message($"Gun_Test_Modularization : {Gun_Test_Modularization}; thing : {thing}");
            //}

            GUI.color = _Border;
            Widgets.DrawLineHorizontal(0, 480, 684);
            Widgets.DrawLineVertical(683, 0, inRect.height);
            Widgets.DrawLineVertical(340, 480, inRect.height - 480);
            GUI.color = Color.white;



            #region infoCard
            InfoTags?.Draw(new Rect(684, 0, 340, inRect.height - 48));
            #endregion


            #region weaponPerview

            Rect BoxSize = new Rect(8, 8, 668, 464);
            Rect BoxInnerSize = new Rect(214, 112, 256, 256);
            //Log.Message($"draw size : {weapon.NodeProccesser.GetAndUpdateDrawSize(weapon.parent.def.defaultPlacingRot)}");
            if (Widgets.ButtonInvisible(BoxSize))
            {
                Widgets.DrawHighlightSelected(BoxSize);
                SelectedPartForChange = (null, null);
            }
            else Widgets.DrawHighlightIfMouseover(BoxSize);
            if (weapon != null)
            {
                Widgets.ThingIcon(BoxInnerSize, weapon);
            }
            else
            {
                Widgets.DrawBoxSolid(new Rect(BoxInnerSize.x, BoxInnerSize.y + (BoxInnerSize.height - 10) / 2, BoxInnerSize.width, 10), Color.gray);
                Widgets.DrawBoxSolid(new Rect(BoxInnerSize.x + (BoxInnerSize.width - 10) / 2, BoxInnerSize.y, 10, BoxInnerSize.height), Color.gray);
            }
            Text.Font = GameFont.Small;
            #endregion


            #region treeView

            BoxSize = new Rect(0, 481, 340, inRect.height - 481);
            Vector2 ScrollViewSize = weapon?.TreeViewDrawSize(new Vector2(276, 48)) ?? Vector2.zero;
            ScrollViewSize.x = Math.Max(ScrollViewSize.x, ScrollViewSize.y < BoxSize.height ? 340 : 304);
            //if (Prefs.DevMode) Log.Message($"creaftingTable.GetTargetCompModularizationWeapon() : {creaftingTable.GetTargetCompModularizationWeapon()}; ScrollViewSize : {ScrollViewSize}");
            Widgets.BeginScrollView(
                BoxSize,
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
            BoxSize = new Rect(341, 481, 342, inRect.height - 481);
            //Widgets.DrawBoxSolid(new Rect(350, 0, inRect.width - 700, inRect.height), Color.black);
            ScrollViewSize = new Vector2(BoxSize.width, selections.Count * 48);
            ScrollViewSize.x = ScrollViewSize.y < inRect.height ? ScrollViewSize.x : ScrollViewSize.x - GUI.skin.verticalScrollbar.fixedWidth;
            Widgets.BeginScrollView(
                BoxSize,
                ref ScrollViews[1],
                new Rect(Vector2.zero, ScrollViewSize)
            );

            (string idForChange, CompModularizationWeapon partForChange) = SelectedPartForChange;
            for(int i = 0; i < selections.Count; i++)
            {
                Rect rect = new Rect(0, i * 48, ScrollViewSize.x, 48);
                if (rect.y + rect.height >= ScrollViews[1].y && rect.y <= ScrollViews[1].y + BoxSize.height)
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
                            try
                            {
                                Widgets.ThingIcon(new Rect(rect.x + 1, rect.y + 1, rect.height - 2, rect.height - 2), selThing);
                            }
                            catch(Exception ex)
                            {
                                Log.Error(ex.ToString());
                            }
                            if (comp_targetModeParent != null)
                            {
                                selThing.holdingOwner = comp_targetModeParent.ChildNodes;
                                comp.NodeProccesser.ResetRenderedTexture();
                            }
                            Widgets.Label(new Rect(rect.x + 48, rect.y + 1, rect.width - 49, rect.height - 2), selThing.Label);
                            if(Widgets.ButtonInvisible(rect))
                            {
                                weapon?.SwapTargetPart();
                                targetMode = !targetMode;
                                partForChange.SetTargetPart(idForChange, selThing);
                                weapon?.SwapTargetPart();
                                targetMode = !targetMode;
                                ResetInfoTags();
                            }
                        }
                        else if(selDef == null)
                        {
                            Widgets.DrawTextureFitted(new Rect(rect.x + 1, rect.y + 1, rect.height - 2, rect.height - 2), partForChange.Props.WeaponAttachmentPropertiesById(idForChange).UITexture, 1);
                            Widgets.Label(new Rect(rect.x + 48, rect.y + 1, rect.width - 49, rect.height - 2), "setEmpty".Translate());
                            if (Widgets.ButtonInvisible(rect))
                            {
                                weapon?.SwapTargetPart();
                                targetMode = !targetMode;
                                partForChange.SetTargetPart(idForChange, null);
                                weapon?.SwapTargetPart();
                                targetMode = !targetMode;
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
                            Widgets.ThingIcon(new Rect(rect.x + 1, rect.y + 1, rect.height - 2, rect.height - 2), selThing);
                            Widgets.Label(new Rect(rect.x + 48, rect.y + 1, rect.width - 49, rect.height - 2), selThing.Label);
                            if (Widgets.ButtonInvisible(rect))
                            {
                                creaftingTable.SetTarget(selThing, this);
                                ResetInfoTags();
                            }
                        }
                        else if (selDef != null)
                        {
                            Widgets.ThingIcon(new Rect(rect.x + 1, rect.y + 1, rect.height - 2, rect.height - 2), selDef);
                            Widgets.Label(new Rect(rect.x + 48, rect.y + 1, rect.width - 49, rect.height - 2), selDef.label);
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


            if (Widgets.ButtonText(new Rect(684, inRect.height - 48, 340,48), "apply".Translate()))
            {
                Close(false);
                if(weapon != null)
                {
                    weapon.SwapTargetPart();
                    targetMode = !targetMode;
                    //weapon.ApplyTargetPart(pawn.Position,pawn.Map);
                    //if (!weapon.parent.Spawned) GenPlace.TryPlaceThing(weapon.parent, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                    Job job = new Job(DefDatabase<JobDef>.GetNamed("ModularizationWeapon_Apply"), creaftingTable.parent);
                    pawn.jobs.EndCurrentJob(JobCondition.Incompletable, false, false);
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
                goto ret;
            }
            //spans[2] = stopwatch.ElapsedTicks;

            weapon?.SwapTargetPart();
            targetMode = !targetMode;
            //if (weapon != null)
            //{
            //    weapon.SwapTargetPart();
            //Log.Message($"weapon.UsingTargetPart : {weapon.UsingTargetPart}");
            //spans[3] = stopwatch.ElapsedTicks;
            //weapon.NodeProccesser.UpdateNode();
            //spans[4] = stopwatch.ElapsedTicks;
            //}
            //Log.Message($"[{spans[0]},{spans[1]},{spans[2]},{spans[3]},{spans[4]}]");
            //drawed = true;
            ret:;
            GUI.skin.horizontalScrollbar.normal.background = horizontalScrollbar;
            GUI.skin.horizontalScrollbarThumb.normal.background = horizontalScrollbarThumb;
            GUI.skin.verticalScrollbar.normal.background = verticalScrollbar;
            GUI.skin.verticalScrollbarThumb.normal.background = verticalScrollbarThumb;
        }

        //private static readonly Stopwatch stopwatch = new Stopwatch();
        private static readonly Color32 _Border = new Color32(97, 108, 122, 255);
        private readonly Pawn pawn;
        private readonly CompCustomWeaponPort creaftingTable;
        private readonly HashSet<(string, CompModularizationWeapon)> selected = new HashSet<(string, CompModularizationWeapon)>();
        private readonly List<(Thing,ThingDef)> selections = new List<(Thing, ThingDef)>();
        private Vector2[] ScrollViews = new Vector2[2];
        private StateInfoTags stateInfoTags = null;
        private bool targetMode = false;
    }
}
