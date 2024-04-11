using RimWorld;
using RW_ModularizationWeapon.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RW_ModularizationWeapon
{
    /// <summary>
    /// `ThingComp` instance on thing for weapon modification
    /// </summary>
    public class CompCustomWeaponPort : ThingComp
    {
        public CompProperties_CustomWeaponPort Props => (CompProperties_CustomWeaponPort)props;


        public override void PostExposeData()
        {
            bool byref = selestedWeapon?.ParentHolder != null;
            Scribe_Values.Look(ref byref, "byref");
            if(byref) Scribe_References.Look(ref selestedWeapon, "selestedWeapon");
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
                    SetTarget(default(ThingDef),null);
                    CustomWeapon window = new CustomWeapon(selPawn, this);
                    Find.WindowStack.Add(window);
                };
                FloatMenuOption option = new FloatMenuOption("StartModifyWeapon".Translate(), action, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return option;
            }
        }


        public void SetTarget(ThingDef selectedDef, CustomWeapon customWeapon=null)
        {
            if (selectedDef == null)
            {
                selestedWeapon = null;
                customWeapon?.ResetInfoTags();
            }
            else SetTarget(ThingMaker.MakeThing(selectedDef), customWeapon);
        }


        public void SetTarget(CompModularizationWeapon selectedWeapon, CustomWeapon customWeapon=null)
        {
            if (selectedWeapon == null)
            {
                selestedWeapon = null;
                customWeapon?.ResetInfoTags();
            }
            else if (Props.filter.Allows(selectedWeapon))
            {
                this.selestedWeapon = selectedWeapon.parent;
                customWeapon?.ResetInfoTags();
            }
        }

        public LocalTargetInfo GetTarget() => selestedWeapon;


        public CompModularizationWeapon GetTargetCompModularizationWeapon() => selestedWeapon;



        private Thing selestedWeapon = null;
    }

    /// <summary>
    /// `ThingComp` properties for type `CompCustomWeaponPort`
    /// </summary>
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


        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            yield return new StatDrawEntry(
                StatCategoryDefOf.Basics,
                "CustomWeaponPort".Translate(),
                filter.AllowedDefCount.ToString() + "Allowd".Translate(),
                "CustomWeaponPortDesc".Translate(),
                100, null,
                from x in filter.AllowedThingDefs select new Dialog_InfoCard.Hyperlink(x)
                );
        }

        /// <summary>
        /// set witch weapon can modify of this port
        /// </summary>
        public ThingFilter filter = new ThingFilter();
    }
}
