using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon.UI
{
    internal class CustomWeapon : Window
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
        /// <summary>
        /// +-------+----------------+-------+
        /// |  new  |                |       |
        /// +-------+                |       |
        /// | tree  |                |       |
        /// | view  |    weapon      | info  |
        /// |       |    perview     | card  |
        /// |       |                |       |
        /// |       |                |       |
        /// |       |                |       |
        /// +-------+----------------+-------+
        /// </summary>
        /// <param name="inRect"></param>
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            // Widgets.Label(new Rect(0, 0, inRect.width, 36), "AssembleWeapon".Translate());
            if(creaftingTable.GetTargetCompModularizationWeapon() == null)
            {
                ThingDef Gun_Test_Modularization = DefDatabase<ThingDef>.GetNamed("Gun_Test_Modularization");
                Thing thing = ThingMaker.MakeThing(Gun_Test_Modularization);
                creaftingTable.SetTarget(thing);
                //if (Prefs.DevMode) Log.Message($"Gun_Test_Modularization : {Gun_Test_Modularization}; thing : {thing}");
            }
            
            Text.Font = GameFont.Small;

            Vector2 TreeViewSize = creaftingTable.GetTargetCompModularizationWeapon()?.TreeViewDrawSize(new Vector2(300, 48)) ?? Vector2.zero;
            TreeViewSize.x = Math.Max(TreeViewSize.x, 348);
            //if (Prefs.DevMode) Log.Message($"creaftingTable.GetTargetCompModularizationWeapon() : {creaftingTable.GetTargetCompModularizationWeapon()}; TreeViewSize : {TreeViewSize}");
            Widgets.DrawBox(new Rect(0, 75, 350,inRect.height - 75));
            Widgets.BeginScrollView(
                new Rect(1, 76, 348, inRect.height - 77),
                ref ScrollViews[0],
                new Rect(Vector2.zero, TreeViewSize)
            );
            creaftingTable.GetTargetCompModularizationWeapon()?.DrawChildTreeView(
                Vector2.zero,
                48,
                TreeViewSize.x,
                (string id,Thing part, CompModularizationWeapon Parent)=>
                {
                    if(selected.Contains((id, Parent)))
                    {
                        selected.Clear();
                    }
                    else
                    {
                        selected.Clear();
                        selected.Add((id, Parent));
                    }
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
                },
                (string id,Thing part, CompModularizationWeapon Parent)=>
                {

                },
                selected
            );
            Widgets.EndScrollView();
            //Widgets.EndGroup();
            Widgets.DrawBoxSolid(new Rect(350, 0, inRect.width - 700, inRect.height), Color.black);


            Widgets.DrawBox(new Rect(inRect.width - 350, 0, 350, inRect.height));
            Widgets.BeginScrollView(
                new Rect(inRect.width - 349, 1, 348, inRect.height - 2),
                ref ScrollViews[1],
                new Rect(0,0,348,0)
            );


            Widgets.EndScrollView();
        }

        private readonly Pawn pawn;
        private readonly CompCustomWeaponPort creaftingTable;
        private readonly HashSet<(string, CompModularizationWeapon)> selected = new HashSet<(string, CompModularizationWeapon)>();
        private Vector2[] ScrollViews = new Vector2[2];
    }
}
