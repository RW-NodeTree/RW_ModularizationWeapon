using RimWorld;
using RW_ModularizationWeapon.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
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
            switch (Scribe.mode)
            {
                case LoadSaveMode.Saving:
                {
                    bool byref = selestedWeapon?.ParentHolder != null;
                    if(byref) Scribe_References.Look(ref selestedWeapon, "selestedWeapon");
                    else Scribe_Deep.Look(ref selestedWeapon, "selestedWeapon");
                    break;
                }
                case LoadSaveMode.LoadingVars:
                {
                    XmlNode xmlNode = Scribe.loader?.curXmlParent?["selestedWeapon"];
                    if (xmlNode == null) return;
                    if (xmlNode.ChildNodes.Count == 1 && xmlNode.FirstChild.NodeType == XmlNodeType.Text) Scribe_References.Look(ref selestedWeapon, "selestedWeapon");
                    else Scribe_Deep.Look(ref selestedWeapon, "selestedWeapon");
                    break;
                }
                default:
                {
                    if(selestedWeapon != null) Scribe_Deep.Look(ref selestedWeapon, "selestedWeapon");
                    else Scribe_References.Look(ref selestedWeapon, "selestedWeapon");
                    break;
                }

            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {

            foreach (FloatMenuOption c in base.CompFloatMenuOptions(selPawn))
            {
                yield return c;
            }
            string disable = null;
            if (selPawn.WorkTagIsDisabled(WorkTags.ManualDumb)) disable = "CanNotDoingManualDumbWork".Translate("StartModifyWeapon".Translate());
            if (!(parent.TryGetComp<CompPowerTrader>()?.PowerOn ?? true)) disable = "NoPower".Translate("StartModifyWeapon".Translate());
            if (!(parent.TryGetComp<CompRefuelable>()?.HasFuel ?? true)) disable = "NoFuel".Translate("StartModifyWeapon".Translate());
            if (selPawn.CanReserveAndReach(new LocalTargetInfo(this.parent), PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
            {
                Action action = delegate ()
                {
                    SetTarget(default(ThingDef),null);
                    CustomWeapon window = new CustomWeapon(selPawn, this);
                    Find.WindowStack.Add(window);
                };
                FloatMenuOption option = new FloatMenuOption(disable ?? "StartModifyWeapon".Translate(), disable == null ? action : null, MenuOptionPriority.Default, null, null, 0f, null, null);
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
