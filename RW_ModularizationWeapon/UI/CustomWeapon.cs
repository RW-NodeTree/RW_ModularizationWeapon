using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

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
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(Math.Max(Verse.UI.screenWidth, 1024), Math.Max(Verse.UI.screenHeight, 576));
            }
        }

        public Thing SelectedThing
        {
            get
            {
                if (selectedThing != null) return selectedThing;
                if(selected.Count > 0)
                {
                    (string id, CompModularizationWeapon comp) = selected.First();
                    if (id != null && comp != null) return comp.GetPart(id) ?? creaftingTable.GetTargetCompModularizationWeapon();
                }
                return creaftingTable.GetTargetCompModularizationWeapon();
            }
            set
            {
                selectedThing = value;
                UpdateStatInfos();
            }
        }


        public void UpdateStatInfos()
        {
            Thing selected = SelectedThing;
            statInfos.Clear();
            if (selected != null)
            {
                List<StatDrawEntry> cache = new List<StatDrawEntry>();
                cache.AddRange(from x in selected.def.SpecialDisplayStats(StatRequest.For(selected)) select x);
                cache.AddRange(from x in (IEnumerable<StatDrawEntry>)StatsReportUtility_StatsToDraw.Invoke(null, new object[] { selected }) where x.ShouldDisplay select x);
                cache.RemoveAll((StatDrawEntry de) => de.stat != null && !de.stat.showNonAbstract);
                statInfos.AddRange(from sd in cache orderby sd.category.displayOrder, sd.DisplayPriorityWithinCategory descending, sd.LabelCap select (false, sd));
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
            // Widgets.Label(new Rect(0, 0, inRect.width, 36), "AssembleWeapon".Translate());
            CompModularizationWeapon weapon = creaftingTable.GetTargetCompModularizationWeapon();
            if (weapon == null)
            {
                ThingDef Gun_Test_Modularization = DefDatabase<ThingDef>.GetNamed("Gun_Test_Modularization");
                creaftingTable.SetTarget(Gun_Test_Modularization,this);
                weapon = creaftingTable.GetTargetCompModularizationWeapon();
                //if (Prefs.DevMode) Log.Message($"Gun_Test_Modularization : {Gun_Test_Modularization}; thing : {thing}");
            }


            #region weaponPerview
            GUI.color = new Color32(97, 108, 122, 255);
            Widgets.DrawBox(new Rect(0, 0, 350, 350));
            GUI.color = Color.white;
            //Log.Message($"draw size : {weapon.NodeProccesser.GetAndUpdateDrawSize(weapon.parent.def.defaultPlacingRot)}");
            if (Widgets.ButtonInvisible(new Rect(7, 7, 336, 336)))
            {
                Widgets.DrawHighlightSelected(new Rect(7, 7, 336, 336));

            }
            else Widgets.DrawHighlightIfMouseover(new Rect(7, 7, 336, 336));
            if (weapon != null) Widgets.ThingIcon(new Rect(7, 7, 336, 336), weapon);
            else
            {
                Widgets.DrawBoxSolid(new Rect(71, 170, 208, 10), Color.gray);
                Widgets.DrawBoxSolid(new Rect(170, 71, 10, 208), Color.gray);
            }
            Text.Font = GameFont.Small;
            #endregion


            #region treeView
            Vector2 TreeViewSize = weapon?.TreeViewDrawSize(new Vector2(300, 48)) ?? Vector2.zero;
            TreeViewSize.x = Math.Max(TreeViewSize.x, 348);
            //if (Prefs.DevMode) Log.Message($"creaftingTable.GetTargetCompModularizationWeapon() : {creaftingTable.GetTargetCompModularizationWeapon()}; TreeViewSize : {TreeViewSize}");
            GUI.color = new Color32(97, 108, 122, 255);
            Widgets.DrawBox(new Rect(0, 358, 350,inRect.height - 358));
            GUI.color = Color.white;
            Widgets.BeginScrollView(
                new Rect(1, 359, 348, inRect.height - 360),
                ref ScrollViews[0],
                new Rect(Vector2.zero, TreeViewSize)
            );
            weapon?.DrawChildTreeView(
                Vector2.zero,
                48,
                TreeViewSize.x,
                (string id,Thing part, CompModularizationWeapon Parent)=>
                {
                    if (selected.Contains((id, Parent)))
                    {
                        selected.Clear();
                    }
                    else
                    {
                        selected.Clear();
                        selected.Add((id, Parent));
                    }
                    SelectedThing = null;
                },
                (string id,Thing part, CompModularizationWeapon Parent)=>
                {
                    if (selected.Contains((id, Parent)))
                    {
                        selected.Clear();
                    }
                    else
                    {
                        selected.Clear();
                        selected.Add((id, Parent));
                    }
                    SelectedThing = null;
                },
                (string id,Thing part, CompModularizationWeapon Parent)=>
                {

                },
                selected
            );
            Widgets.EndScrollView();
            //Widgets.EndGroup();
            #endregion


            #region targetPicker
            Widgets.DrawBoxSolid(new Rect(350, 0, inRect.width - 700, inRect.height), Color.black);


            #endregion


            #region infoCard
            GUI.color = new Color32(97, 108, 122, 255);
            Widgets.DrawBox(new Rect(inRect.width - 350, 0, 350, inRect.height));
            GUI.color = Color.white;
            float infoCardWidth = inRect.height - 2 > infoCardMaxHeight ? 348 : 332;
            Widgets.BeginScrollView(
                new Rect(inRect.width - 349, 1, 348, inRect.height - 2),
                ref ScrollViews[1],
                new Rect(0,0, infoCardWidth, infoCardMaxHeight)
            );
            Widgets.BeginGroup(new Rect(-8, 0, infoCardWidth + 8, infoCardMaxHeight));
            Text.Font = GameFont.Small;
            infoCardMaxHeight = 0;
            for(int i = 0; i < statInfos.Count; i++)
            {
                (bool open, StatDrawEntry stat) = statInfos[i];
                infoCardMaxHeight += stat.Draw(
                    0,
                    infoCardMaxHeight,
                    infoCardWidth,
                    open,
                    false,
                    false,
                    delegate ()
                    {
                        open = !open;
                    },
                    delegate () { },
                    ScrollViews[1],
                    new Rect(0,0, 0, inRect.height - 2)
                );
                if (open)
                {
                    string explanationText = stat.GetExplanationText(StatRequest.For(SelectedThing));
                    float num3 = Text.CalcHeight(explanationText, infoCardWidth) + 10f;
                    if (infoCardMaxHeight + 2 >= ScrollViews[1].y && infoCardMaxHeight <= ScrollViews[1].y + inRect.height - 2)
                        Widgets.DrawBoxSolid(new Rect(8, infoCardMaxHeight, infoCardWidth, 2), new Color32(51, 153, 255, 96));

                    infoCardMaxHeight += 2;

                    if (infoCardMaxHeight + num3 >= ScrollViews[1].y && infoCardMaxHeight <= ScrollViews[1].y + inRect.height - 2)
                    {
                        GUI.color = new Color32(51, 153, 255, 255);
                        Widgets.DrawHighlightSelected(new Rect(8, infoCardMaxHeight, infoCardWidth, num3));
                        GUI.color = Color.white;
                        Widgets.Label(new Rect(8, infoCardMaxHeight, infoCardWidth, num3), explanationText);
                    }
                    infoCardMaxHeight += num3;
                }
                statInfos[i] = (open, stat);
            }
            Widgets.EndGroup();
            Widgets.EndScrollView();
            #endregion
        }

        public override void Close(bool doCloseSound = true)
        {
            CompModularizationWeapon weapon = creaftingTable.GetTargetCompModularizationWeapon();
            if (weapon != null) weapon.ShowTargetPart = false;
            base.Close(doCloseSound);
        }

        private static readonly Color32 Border = new Color32(97, 108, 122, 255);
        private readonly Pawn pawn;
        private readonly CompCustomWeaponPort creaftingTable;
        private readonly HashSet<(string, CompModularizationWeapon)> selected = new HashSet<(string, CompModularizationWeapon)>();
        private readonly List<(bool,StatDrawEntry)> statInfos = new List<(bool,StatDrawEntry)>();
        private readonly MethodInfo StatsReportUtility_StatsToDraw = typeof(StatsReportUtility).GetMethod("StatsToDraw", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(Thing) }, null);
        private Vector2[] ScrollViews = new Vector2[2];
        private Thing selectedThing = null;
        private float infoCardMaxHeight = 0;
    }
}
