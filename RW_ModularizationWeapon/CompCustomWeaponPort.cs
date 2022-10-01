using RimWorld;
using RW_ModularizationWeapon.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RW_ModularizationWeapon
{
    public class CompCustomWeaponPort : ThingComp
    {
        public CompProperties_CustomWeaponPort Props => (CompProperties_CustomWeaponPort)props;


        public override void PostExposeData()
        {
            Scribe_Values.Look(ref privteGen, "privteGen");
            if(privteGen) Scribe_References.Look(ref selestedWeapon, "selestedWeapon");
            else Scribe_Deep.Look(ref selestedWeapon, "selestedWeapon");
        }


        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {

            foreach (FloatMenuOption c in base.CompFloatMenuOptions(selPawn))
            {
                yield return c;
            }
            CompPowerTrader compPower = this.parent.TryGetComp<CompPowerTrader>();
            if (compPower != null)
            {
                if (!compPower.PowerOn)
                {
                    yield break;
                }
            }
            if (selPawn.CanReserveAndReach(new LocalTargetInfo(this.parent), PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
            {
                Action action = delegate ()
                {
                    CustomWeapon window = new CustomWeapon(selPawn, this);
                    Find.WindowStack.Add(window);
                };
                FloatMenuOption option = new FloatMenuOption("StartAssembleWeapon".Translate(), action, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return option;
            }
        }


        public void SetTarget(ThingDef selectedDef)
        {
            if (selectedDef == null) selestedWeapon = null;
            else SetTarget(ThingMaker.MakeThing(selectedDef));
            privteGen = selestedWeapon != null;
        }


        public void SetTarget(CompModularizationWeapon selectedWeapon)
        {
            if ((CompModularizationWeapon)this.selestedWeapon != null) ((CompModularizationWeapon)this.selestedWeapon).ShowTargetPart = false;
            if (selectedWeapon == null) selestedWeapon = null;
            else if (Props.filter.Allows(selectedWeapon)) this.selestedWeapon = selectedWeapon.parent;
            if ((CompModularizationWeapon)this.selestedWeapon != null) ((CompModularizationWeapon)this.selestedWeapon).ShowTargetPart = true;
            privteGen = false;
        }


        public LocalTargetInfo GetTarget() => selestedWeapon;


        public CompModularizationWeapon GetTargetCompModularizationWeapon() => selestedWeapon;


        private bool privteGen = false;

        private Thing selestedWeapon = null;
    }


    public class CompProperties_CustomWeaponPort : CompProperties
    {
        public CompProperties_CustomWeaponPort()
        {
            base.compClass = typeof(CompCustomWeaponPort);
        }


        public IEnumerable<ThingDef> AllowsMakedDefs => filter.AllowedThingDefs;


        public override void ResolveReferences(ThingDef parentDef)
        {
            filter.ResolveReferences();
        }

        public ThingFilter filter = new ThingFilter();
    }
}
