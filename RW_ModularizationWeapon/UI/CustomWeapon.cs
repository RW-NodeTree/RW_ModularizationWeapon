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
                return new Vector2(Verse.UI.screenWidth, Verse.UI.screenHeight);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, 0, inRect.width, 36), "AssembleWeapon".Translate());
            Text.Font = GameFont.Small;
            Widgets.BeginScrollView(new Rect(20, 48, inRect.width - 300, inRect.height - 175), ref ScrollViews[0], new Rect(0,0, inRect.width - 300,100));
            creaftingTable.GetTargetCompModularizationWeapon()?.DisplayUI();
            Widgets.EndScrollView();
        }

        private readonly Pawn pawn;
        private readonly CompCustomWeaponPort creaftingTable;
        private Vector2[] ScrollViews = new Vector2[1];
        internal WeaponPartNodeView selectedNodeView;
        internal WeaponPartNodeView rootNodeView;
    }
}
