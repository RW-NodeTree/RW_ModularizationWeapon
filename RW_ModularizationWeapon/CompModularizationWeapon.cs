using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RimWorld;
using RW_NodeTree;
using RW_NodeTree.Rendering;
using RW_NodeTree.Tools;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon
{
    public class CompModularizationWeapon : CompBasicNodeComp, IEnumerable<(string,Thing)>
    {
        public CompProperties_ModularizationWeapon Props => (CompProperties_ModularizationWeapon)props;

        public CompModularizationWeapon ParentPart => ParentProccesser?.parent;

        public CompModularizationWeapon RootPart
        {
            get
            {
                CompModularizationWeapon result = null;
                CompModularizationWeapon current = this;
                while (current != null)
                {
                    result = current;
                    current = current.ParentPart;
                }
                return result;
            }
        }

        public override void PostPostMake()
        {
            CompChildNodeProccesser nodeProccesser = NodeProccesser;
            if(nodeProccesser != null)
            {
                foreach(WeaponAttachmentProperties properties in Props.attachmentProperties)
                {
                    ThingDef def = properties.defultThing;
                    if(def != null)
                    {
                        Thing thing = ThingMaker.MakeThing(def, GenStuff.RandomStuffFor(def));
                        thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
                        nodeProccesser.AppendChild(thing, properties.id);
                    }
                }
            }
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref showTargetPart, "showTargetPart");
            Scribe_Values.Look(ref usingTargetPart, "usingTargetPart");
            Scribe_Collections.Look(ref targetPartsWithId, "targetPartsWithId", LookMode.Value, LookMode.LocalTargetInfo);
        }


        #region Condition
        public bool Unchangeable(string id) => internal_Unchangeable(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal bool internal_Unchangeable(Thing thing, WeaponAttachmentProperties properties)
        {
            if(thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                if (comp != null && comp.Validity)
                {
                    return comp.Props.unchangeable || properties.unchangeable;
                }
                else
                {
                    return properties.unchangeable;
                }
            }
            return false;
        }


        public bool NotDraw(string id) => internal_NotDraw(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal bool internal_NotDraw(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                if (comp != null && comp.Validity)
                {
                    return comp.Props.notDrawInParent || properties.notDraw;
                }
                else
                {
                    return properties.notDraw;
                }
            }
            return false;
        }


        public bool NotUseTools(string id) => internal_NotUseTools(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal bool internal_NotUseTools(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                if (comp != null && comp.Validity)
                {
                    return comp.Props.notAllowParentUseTools || properties.notUseTools;
                }
                else
                {
                    return properties.notUseTools;
                }
            }
            return false;
        }
        

        public bool NotUseVerbProperties(string id) => internal_NotUseVerbProperties(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal bool internal_NotUseVerbProperties(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                if (comp != null && comp.Validity)
                {
                    return comp.Props.notAllowParentUseVerbProperties && properties.notUseVerbProperties;
                }
                else
                {
                    return properties.notUseVerbProperties;
                }
            }
            return false;
        }
        

        public bool VerbPropertiesAffectByOtherPart(string id) => internal_VerbPropertiesAffectByOtherPart(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal bool internal_VerbPropertiesAffectByOtherPart(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                if (comp != null && comp.Validity)
                {
                    return comp.Props.verbPropertiesAffectByOtherPart && properties.verbPropertiesAffectByOtherPart;
                }
                else
                {
                    return properties.verbPropertiesAffectByOtherPart;
                }
            }
            return false;
        }
        

        public bool ToolsAffectByOtherPart(string id) => internal_ToolsAffectByOtherPart(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal bool internal_ToolsAffectByOtherPart(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                if (comp != null && comp.Validity)
                {
                    return comp.Props.toolsAffectByOtherPart && properties.toolsAffectByOtherPart;
                }
                else
                {
                    return properties.toolsAffectByOtherPart;
                }
            }
            return false;
        }

        #endregion


        #region TargetPart
        public bool ShowTargetPart
        {
            get
            {
                bool result = false;
                CompModularizationWeapon current = this;
                while(!result && current != null)
                {
                    result = current.showTargetPart;
                    current = current.ParentPart;
                }
                return result;
            }
            set
            {
                //Log.Message($"ShowTargetPart {parent} : {value}; org : {ShowTargetPart}");

                showTargetPart = value;
                UsingTargetPart = ShowTargetPart;
                RootNode?.UpdateNode();
            }
        }

        private bool UsingTargetPart
        {
            get => usingTargetPart;
            set
            {
                //Log.Message($"UsingTargetPart {parent} : {value}; org : {usingTargetPart}");
                if (usingTargetPart != value)
                {
                    usingTargetPart = value;
                    foreach (string id in NodeProccesser.RegiestedNodeId)
                    {
                        LocalTargetInfo cache;
                        if(targetPartsWithId.TryGetValue(id, out cache))
                        {
                            targetPartsWithId[id] = ChildNodes[id];
                            ChildNodes[id] = cache.Thing;
                        }
                        else
                        {
                            CompModularizationWeapon comp = ChildNodes[id];
                            if (comp != null)
                            {
                                comp.targetModeParent = NodeProccesser;
                                comp.UsingTargetPart = value;
                            }
                        }
                    }
                }
            }
        }


        public LocalTargetInfo OrginalPart(string id) => UsingTargetPart ? ((targetPartsWithId.TryGetValue(id)).Thing ?? ChildNodes[id]) : ChildNodes[id];


        public bool SetTargetPart(string id, LocalTargetInfo targetInfo)
        {
            if (id != null && NodeProccesser.AllowNode(targetInfo.Thing, id))
            {

                //Log.Message($"SetTargetPart {id} : {targetInfo}; {UsingTargetPart}");
                if (UsingTargetPart)
                {
                    if(!targetPartsWithId.ContainsKey(id)) targetPartsWithId.Add(id, ChildNodes[id]);
                    ChildNodes[id] = targetInfo.Thing;
                    if (targetPartsWithId[id].Thing == targetInfo.Thing) targetPartsWithId.Remove(id);
                    NeedUpdate = true;
                    RootNode?.UpdateNode();
                }
                else
                {
                    if (targetInfo.Thing == ChildNodes[id])
                        targetPartsWithId.Remove(id);
                    else if ((targetInfo.Thing?.Spawned ?? true))
                        targetPartsWithId.SetOrAdd(id, targetInfo);
                }
                return true;
            }
            return false;
        }


        public void ResetTargetPart()
        {
            targetPartsWithId.Clear();
        }


        public void ApplyTargetPart(IntVec3 pos, Map map)
        {
            foreach((string id, LocalTargetInfo item) in targetPartsWithId)
            {
                Thing thing = ChildNodes[id];
                ChildNodes[id] = item.Thing;
                if(ChildNodes[id] == item.Thing)
                if (thing != null && map != null)
                {
                    GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near);
                }
            }

            ResetTargetPart();

            foreach (Thing item in ChildNodes.InnerListForReading)
            {
                CompModularizationWeapon comp = item;
                if(comp != null)
                {
                    comp.ApplyTargetPart(pos, map);
                }
            }
        }


        public IEnumerator<(string,Thing)> GetEnumerator()
        {
            foreach(string id in NodeProccesser.RegiestedNodeId)
            {
                yield return (id,ChildNodes[id]);
            }
            yield break;
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion


        #region Offset
        public float ArmorPenetrationOffset
        {
            get
            {
                float result = Props.armorPenetrationOffset;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result += comp.ArmorPenetrationOffset * properties.armorPenetrationOffsetAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float MeleeCooldownTimeOffset
        {
            get
            {
                float result = Props.meleeCooldownTimeOffset;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result += comp.MeleeCooldownTimeOffset * properties.meleeCooldownTimeOffsetAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float MeleeDamageOffset
        {
            get
            {
                float result = Props.meleeDamageOffset;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result += comp.MeleeDamageOffset * properties.meleeDamageOffsetAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float BurstShotCountOffset
        {
            get
            {
                float result = Props.burstShotCountOffset;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result += comp.BurstShotCountOffset * properties.burstShotCountOffsetAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float TicksBetweenBurstShotsOffset
        {
            get
            {
                float result = Props.ticksBetweenBurstShotsOffset;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result += comp.TicksBetweenBurstShotsOffset * properties.ticksBetweenBurstShotsOffsetAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float MuzzleFlashScaleOffset
        {
            get
            {
                float result = Props.muzzleFlashScaleOffset;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result += comp.MuzzleFlashScaleOffset * properties.muzzleFlashScaleOffsetAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float RangeOffset
        {
            get
            {
                float result = Props.rangeOffset;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result += comp.RangeOffset * properties.rangeOffsetAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float WarmupTimeOffset
        {
            get
            {
                float result = Props.warmupTimeOffset;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result += comp.WarmupTimeOffset * properties.warmupTimeOffsetAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float GetStatOffset(StatDef stateDef)
        {
            float result = Props.statOffset.GetStatOffsetFromList(stateDef);
            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing thing = container[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (thing != null)
                {
                    CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                    if (comp != null && comp.Validity)
                    {
                        result += comp.GetStatOffset(stateDef) * properties.statOffsetAffectHorizon.GetStatFactorFromList(stateDef);
                    }
                }
            }
            return result;
        }
        #endregion


        #region Multiplier
        public float ArmorPenetrationMultiplier
        {
            get
            {
                float result = Props.armorPenetrationMultiplier;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result *= 1f + (comp.ArmorPenetrationMultiplier - 1f) * properties.armorPenetrationMultiplierAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float MeleeCooldownTimeMultiplier
        {
            get
            {
                float result = Props.meleeCooldownTimeMultiplier;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result *= 1f + (comp.MeleeCooldownTimeMultiplier - 1f) * properties.meleeCooldownTimeMultiplierAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float MeleeDamageMultiplier
        {
            get
            {
                float result = Props.meleeDamageMultiplier;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result *= 1f + (comp.MeleeDamageMultiplier - 1f) * properties.meleeDamageMultiplierAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float BurstShotCountMultiplier
        {
            get
            {
                float result = Props.burstShotCountMultiplier;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result *= 1f + (comp.BurstShotCountMultiplier - 1f) * properties.burstShotCountMultiplierAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float TicksBetweenBurstShotsMultiplier
        {
            get
            {
                float result = Props.ticksBetweenBurstShotsMultiplier;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result *= 1f + (comp.TicksBetweenBurstShotsMultiplier - 1f) * properties.ticksBetweenBurstShotsMultiplierAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float MuzzleFlashScaleMultiplier
        {
            get
            {
                float result = Props.muzzleFlashScaleMultiplier;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result *= 1f + (comp.MuzzleFlashScaleMultiplier - 1f) * properties.muzzleFlashScaleMultiplierAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float RangeMultiplier
        {
            get
            {
                float result = Props.rangeMultiplier;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result *= 1f + (comp.RangeMultiplier - 1f) * properties.rangeMultiplierAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float WarmupTimeMultiplier
        {
            get
            {
                float result = Props.warmupTimeMultiplier;
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if(thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity)
                        {
                            result *= 1f + (comp.WarmupTimeMultiplier - 1f) * properties.warmupTimeMultiplierAffectHorizon;
                        }
                    }
                }
                return result;
            }
        }
        
        public float GetStatMultiplier(StatDef stateDef)
        {
            float result = Props.statMultiplier.GetStatFactorFromList(stateDef);
            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing thing = container[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (thing != null)
                {
                    CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                    if (comp != null && comp.Validity)
                    {
                        result *= 1f + (comp.GetStatMultiplier(stateDef) - 1f) * properties.statMultiplierAffectHorizon.GetStatFactorFromList(stateDef);
                    }
                }
            }
            return result;
        }
        #endregion


        #region Def
        public ThingDef ForceProjectile
        {
            get
            {
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    if (thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity && comp.Props.forceProjectile != null && typeof(Projectile).IsAssignableFrom(comp.Props.forceProjectile.thingClass))
                        {
                            return comp.Props.forceProjectile;
                        }
                    }
                }
                return null;
            }
        }
        

        public SoundDef ForceSound
        {
            get
            {
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    if (thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity && comp.Props.forceSound != null)
                        {
                            return comp.Props.forceSound;
                        }
                    }
                }
                return null;
            }
        }
        

        public SoundDef ForceSoundCastTail
        {
            get
            {
                NodeContainer container = ChildNodes;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing thing = container[i];
                    if (thing != null)
                    {
                        CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                        if (comp != null && comp.Validity && comp.Props.forceSoundCastTail != null)
                        {
                            return comp.Props.forceSoundCastTail;
                        }
                    }
                }
                return null;
            }
        }
        #endregion


        #region Verb
        internal VerbProperties VerbPropertiesAfterAffect(VerbProperties properties, bool affectDef)
        {
            properties = (VerbProperties)properties.SimpleCopy();
            properties.burstShotCount = (int)(BurstShotCountMultiplier * properties.burstShotCount / Props.burstShotCountMultiplier + BurstShotCountOffset - Props.burstShotCountOffset);
            properties.ticksBetweenBurstShots = (int)(TicksBetweenBurstShotsMultiplier * properties.ticksBetweenBurstShots / Props.ticksBetweenBurstShotsMultiplier + TicksBetweenBurstShotsOffset - Props.ticksBetweenBurstShotsOffset);
            properties.muzzleFlashScale = (int)(MuzzleFlashScaleMultiplier * properties.muzzleFlashScale / Props.muzzleFlashScaleMultiplier + MuzzleFlashScaleOffset - Props.muzzleFlashScaleOffset);
            properties.range = (int)(RangeMultiplier * properties.range / Props.rangeOffset + RangeOffset - Props.rangeOffset);
            properties.warmupTime = (int)(WarmupTimeMultiplier * properties.warmupTime / Props.warmupTimeMultiplier + WarmupTimeOffset - Props.warmupTimeOffset);
            if(affectDef)
            {
                properties.defaultProjectile = ForceProjectile ?? properties.defaultProjectile;
                properties.soundCast = ForceSound ?? properties.soundCast;
                properties.soundCastTail = ForceSoundCastTail ?? properties.soundCastTail;
            }
            return properties;
        }


        internal Tool ToolAfterAffect(Tool tool)
        {
            tool = (Tool)tool.SimpleCopy();
            tool.armorPenetration = (int)(ArmorPenetrationMultiplier * tool.armorPenetration / Props.armorPenetrationMultiplier + ArmorPenetrationOffset - Props.armorPenetrationOffset);
            tool.cooldownTime = (int)(MeleeCooldownTimeMultiplier * tool.cooldownTime / Props.meleeCooldownTimeMultiplier + MeleeCooldownTimeOffset - Props.meleeCooldownTimeOffset);
            tool.power = (int)(MeleeDamageMultiplier * tool.power / Props.meleeDamageMultiplier + MeleeDamageOffset - Props.meleeDamageOffset);
            return tool;
        }


        protected override List<VerbToolRegiestInfo> PostIVerbOwner_GetTools(Type ownerType, List<VerbToolRegiestInfo> result, Dictionary<string, object> forPostRead)
        {
            if (Props.verbPropertiesAffectByChildPart)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    VerbToolRegiestInfo prop = result[i];
                    Tool newProp = ToolAfterAffect(prop.berforConvertTool);
                    prop.afterCobvertTool = newProp;
                    result[i] = prop;
                }
            }

            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                WeaponAttachmentProperties attachmentProperties = Props.WeaponAttachmentPropertiesById(id);
                if (!internal_NotUseTools(container[i], attachmentProperties))
                {
                    List<Tool> tools = CompChildNodeProccesser.GetSameTypeVerbOwner(ownerType, container[i])?.Tools;
                    if (tools != null)
                    {
                        result.Capacity += tools.Count;
                        if (internal_ToolsAffectByOtherPart(container[i], attachmentProperties))
                        {
                            for (int j = 0; j < tools.Count; j++)
                            {
                                Tool cache = tools[j];
                                Tool prop = ((CompChildNodeProccesser)container[i])?.GetBeforeConvertVerbCorrespondingThing(ownerType, cache, null).Item3;
                                Tool newProp = ToolAfterAffect(prop ?? cache);
                                result.Add(new VerbToolRegiestInfo(id, cache, newProp));
                            }
                        }
                        else
                        {
                            for (int j = 0; j < tools.Count; j++)
                            {
                                Tool cache = tools[j];
                                result.Add(new VerbToolRegiestInfo(id, cache, cache));
                            }
                        }
                    }
                }
            }
            return result;
        }


        protected override List<VerbPropertiesRegiestInfo> PostIVerbOwner_GetVerbProperties(Type ownerType, List<VerbPropertiesRegiestInfo> result, Dictionary<string, object> forPostRead)
        {
            if (Props.verbPropertiesAffectByChildPart)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    VerbPropertiesRegiestInfo prop = result[i];
                    VerbProperties newProp = VerbPropertiesAfterAffect(prop.berforConvertProperties, true);
                    prop.afterConvertProperties = newProp;
                    result[i] = prop;
                }
            }

            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                WeaponAttachmentProperties attachmentProperties = Props.WeaponAttachmentPropertiesById(id);
                if (!internal_NotUseVerbProperties(container[i], attachmentProperties))
                {
                    List<VerbProperties> verbProperties = CompChildNodeProccesser.GetSameTypeVerbOwner(ownerType, container[i])?.VerbProperties;
                    if (verbProperties != null)
                    {
                        result.Capacity += verbProperties.Count;
                        if (internal_VerbPropertiesAffectByOtherPart(container[i], attachmentProperties))
                        {
                            for (int j = 0; j < verbProperties.Count; j++)
                            {
                                VerbProperties cache = verbProperties[j];
                                VerbProperties prop = ((CompChildNodeProccesser)container[i])?.GetBeforeConvertVerbCorrespondingThing(ownerType, null, cache).Item4;
                                VerbProperties newProp = VerbPropertiesAfterAffect(prop ?? cache, false);
                                result.Add(new VerbPropertiesRegiestInfo(id, cache, newProp));
                            }
                        }
                        else
                        {
                            for (int j = 0; j < verbProperties.Count; j++)
                            {
                                VerbProperties cache = verbProperties[j];
                                result.Add(new VerbPropertiesRegiestInfo(id, cache, cache));
                            }
                        }
                    }
                }
            }
            return result;
        }
        #endregion


        #region Stat
        protected override void PreStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, Dictionary<string, object> forPostRead)
        {
            if (statWorker is StatWorker_MeleeAverageArmorPenetration || statWorker is StatWorker_MeleeAverageDPS)
            {
                CompEquippable eq = parent.GetComp<CompEquippable>();
                if (eq != null)
                {
                    forPostRead.Add("CompModularizationWeapon_verbs", new List<VerbProperties>(parent.def.Verbs));
                    forPostRead.Add("CompModularizationWeapon_tools", new List<Tool>(parent.def.tools));
                    //if (Prefs.DevMode) Log.Message(" prefix before clear: parent.def.Verbs0=" + parent.def.Verbs.Count + "; parent.def.tools0=" + parent.def.tools.Count + ";\n");
                    List<Verb> verbs = eq.AllVerbs;
                    parent.def.Verbs.Clear();
                    parent.def.tools.Clear();
                    //if (Prefs.DevMode) Log.Message(" prefix before change: parent.def.Verbs.Count=" + parent.def.Verbs.Count + "; parent.def.tools.Count=" + parent.def.tools.Count + ";\n");
                    foreach (Verb verb in verbs)
                    {
                        if (verb.tool != null)
                        {
                            parent.def.tools.Add(verb.tool);
                        }
                        else
                        {
                            parent.def.Verbs.Add(verb.verbProps);
                        }
                    }
                    //if (Prefs.DevMode) Log.Message(" prefix after change: parent.def.Verbs.Count=" + parent.def.Verbs.Count + "; parent.def.tools.Count=" + parent.def.tools.Count + ";\n");
                }
            }
        }


        protected override float PostStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            if (statWorker is StatWorker_MeleeAverageArmorPenetration || statWorker is StatWorker_MeleeAverageDPS)
            {
                CompEquippable eq = parent.GetComp<CompEquippable>();
                if (eq != null)
                {
                    //if (Prefs.DevMode) Log.Message(" postfix before clear: parent.def.Verbs.Count=" + parent.def.Verbs.Count + "; parent.def.tools.Count=" + parent.def.tools.Count + ";\n");
                    parent.def.Verbs.Clear();
                    parent.def.tools.Clear();
                    //if (Prefs.DevMode) Log.Message(" postfix before change: parent.def.Verbs.Count=" + parent.def.Verbs.Count + "; parent.def.tools.Count=" + parent.def.tools.Count + ";\n");
                    parent.def.Verbs.AddRange((List<VerbProperties>)forPostRead["CompModularizationWeapon_verbs"]);
                    parent.def.tools.AddRange((List<Tool>)forPostRead["CompModularizationWeapon_tools"]);
                    //if (Prefs.DevMode) Log.Message(" postfix after change: parent.def.Verbs0=" + parent.def.Verbs.Count + "; parent.def.tools0=" + parent.def.tools.Count + ";\n");
                }
            }
            return result;
        }


        protected override float PostStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            if(statWorker is StatWorker_MarketValue || statWorker == StatDefOf.Mass.Worker)
            {
                foreach (Thing thing in ChildNodes.InnerListForReading)
                {
                    result += statWorker.GetValue(thing);
                }
            }
            return result;
        }


        protected override string PostStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, string result, Dictionary<string, object> forPostRead)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (statWorker is StatWorker_MeleeAverageDPS || 
                statWorker is StatWorker_MeleeAverageArmorPenetration || 
                statWorker is StatWorker_MarketValue ||
                statWorker == StatDefOf.Mass.Worker
            )
            {
                foreach (Thing thing in ChildNodes.InnerListForReading)
                {
                    stringBuilder.AppendLine("  " + thing.Label + ":");
                    string exp = "\n" + statWorker.GetExplanationUnfinalized(StatRequest.For(thing), numberSense);
                    exp = Regex.Replace(exp, "\n", "\n  ");
                    stringBuilder.AppendLine(exp);
                }
            }
            return result + "\n" + stringBuilder.ToString();
        }


        protected override IEnumerable<Dialog_InfoCard.Hyperlink> PostStatWorker_GetInfoCardHyperlinks(StatWorker statWorker, StatRequest reqstatRequest, IEnumerable<Dialog_InfoCard.Hyperlink> result)
        {
            foreach(Dialog_InfoCard.Hyperlink link in result)
            {
                yield return link;
            }
            if (statWorker is StatWorker_MeleeAverageDPS ||
                statWorker is StatWorker_MeleeAverageArmorPenetration ||
                statWorker is StatWorker_MarketValue ||
                statWorker == StatDefOf.Mass.Worker
            )
            {
                foreach (Thing thing in ChildNodes.InnerListForReading)
                {
                    yield return new Dialog_InfoCard.Hyperlink(thing);
                }
            }
        }
        #endregion


        #region UI
        #region TreeView
        public bool GetChildTreeViewOpend(string id)
        {
            bool result;
            if(!childTreeViewOpend.TryGetValue(id,out result))
            {
                result = false;
                childTreeViewOpend.Add(id,result);
            }
            return result;
        }


        public void SetChildTreeViewOpend(string id, bool value) => childTreeViewOpend.SetOrAdd(id,value);


        public Vector2 TreeViewDrawSize(Vector2 BlockSize)
        {
            Vector2 result = BlockSize;
            foreach((string id, Thing thing) in this)
            {
                if(id != null && GetChildTreeViewOpend(id))
                {
                    CompModularizationWeapon comp = thing;
                    if (comp != null)
                    {
                        Vector2 childSize = comp.TreeViewDrawSize(BlockSize);
                        result.y += childSize.y;
                        result.x = Math.Max(childSize.x + BlockSize.y, result.x);
                    }
                    else
                    {
                        result.y += BlockSize.y;
                    }
                }
            }
            return result;
        }

        
        public float DrawChildTreeView(
            Vector2 DrawPos,
            float BlockHeight,
            float ContainerWidth,
            Action<string,Thing,CompModularizationWeapon> openEvent,
            Action<string,Thing,CompModularizationWeapon> closeEvent,
            Action<string,Thing,CompModularizationWeapon> iconEvent,
            HashSet<(string, CompModularizationWeapon)> Selected
        )
        {
            Vector2 currentPos = DrawPos;
            bool cacheWordWrap = Text.WordWrap;
            GameFont cacheFont = Text.Font;
            TextAnchor cacheAnchor = Text.Anchor;
            Text.WordWrap = false;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            foreach ((string id, Thing thing) in this)
            {
                if(id != null)
                {
                    if (Selected?.Contains((id,this)) ?? false) Widgets.DrawBoxSolidWithOutline(new Rect(currentPos.x, currentPos.y,ContainerWidth,BlockHeight), new Color32(51, 153, 255, 64), new Color32(51, 153, 255, 96));
                    else if(GetChildTreeViewOpend(id)) Widgets.DrawHighlightSelected(new Rect(currentPos.x, currentPos.y,ContainerWidth,BlockHeight));
                    Widgets.DrawHighlightIfMouseover(new Rect(currentPos.x, currentPos.y,ContainerWidth,BlockHeight));//hover

                    if(thing != null)
                    {
                        Widgets.ThingIcon(new Rect(currentPos.x+1, currentPos.y+1,BlockHeight-1,BlockHeight-2),thing);
                        Widgets.Label(new Rect(currentPos.x+BlockHeight, currentPos.y+1,ContainerWidth-BlockHeight-1,BlockHeight-2),$"{id} : {thing.Label}");

                        if (Widgets.ButtonInvisible(new Rect(currentPos.x, currentPos.y, BlockHeight, BlockHeight)))
                        {
                            iconEvent?.Invoke(id, thing, this);
                        }
                        if ((CompModularizationWeapon)thing != null)
                        {
                            bool opend = GetChildTreeViewOpend(id);
                            if (Widgets.ButtonInvisible(new Rect(currentPos.x + BlockHeight, currentPos.y, ContainerWidth - BlockHeight, BlockHeight)))
                            {
                                opend = !opend;
                                if (opend) openEvent?.Invoke(id, thing, this);
                                else closeEvent?.Invoke(id, thing, this);
                                SetChildTreeViewOpend(id, opend);
                            }
                            if (opend)
                            {
                                currentPos.y += ((CompModularizationWeapon)thing).DrawChildTreeView(
                                    currentPos + Vector2.one * BlockHeight,
                                    BlockHeight,
                                    ContainerWidth - BlockHeight,
                                    openEvent,
                                    closeEvent,
                                    iconEvent,
                                    Selected
                                );
                            }
                        }
                        else if (Widgets.ButtonInvisible(new Rect(currentPos.x + BlockHeight, currentPos.y, ContainerWidth - BlockHeight, BlockHeight)))
                        {
                            openEvent?.Invoke(id, thing, this);
                        }
                    }
                    else
                    {
                        Widgets.DrawTextureFitted(new Rect(currentPos.x, currentPos.y,BlockHeight,BlockHeight),Props.WeaponAttachmentPropertiesById(id).UITexture,1);
                        Widgets.Label(new Rect(currentPos.x+BlockHeight, currentPos.y,ContainerWidth-BlockHeight,BlockHeight),id);
                        if(Widgets.ButtonInvisible(new Rect(currentPos.x, currentPos.y,ContainerWidth,BlockHeight))) iconEvent?.Invoke(id,thing,this);
                    }
                    currentPos.y += BlockHeight;
                }
            }
            Text.WordWrap = cacheWordWrap;
            Text.Font = GameFont.Small;
            Text.Anchor = cacheAnchor;
            return currentPos.y - DrawPos.y;
        }
        #endregion





        #endregion


        protected override List<(Thing, string, List<RenderInfo>)> OverrideDrawSteep(List<(Thing, string, List<RenderInfo>)> nodeRenderingInfos, Rot4 rot, Graphic graphic)
        {
            for (int i = 0; i < nodeRenderingInfos.Count; i++)
            {
                (Thing part, string id, List<RenderInfo> renderInfos) = nodeRenderingInfos[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (id.NullOrEmpty() && part == parent)
                {
                    List<RenderInfo> cacheInfo = renderInfos;
                    if (ParentProccesser != null)
                    {
                        for(int j = 0; j < cacheInfo.Count; j++)
                        {
                            RenderInfo info = cacheInfo[j];
                            if(info.material == graphic?.MatAt(rot, this.parent))
                            {
                                info.material = Props.PartTexMaterial ?? info.material;
                                cacheInfo[j] = info;
                            }
                        }
                    }
                }
                else if (!internal_NotDraw(part, properties))
                {
                    List<RenderInfo> cacheInfo = renderInfos;
                    if (properties != null)
                    {
                        for (int j = 0; j < cacheInfo.Count; j++)
                        {
                            RenderInfo info = cacheInfo[j];
                            Matrix4x4[] matrix = info.matrices;
                            for (int k = 0; k < matrix.Length; k++)
                            {
                                matrix[k] = properties.Transfrom * matrix[k];
                            }
                            cacheInfo[j] = info;
                        }
                    }
                }
            }
            return nodeRenderingInfos;
        }


        protected override bool AllowNode(Thing node, string id = null)
        {
            WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
            //if (Prefs.DevMode) Log.Message($"properties : {properties}");
            if (properties != null)
            {
                if(node == null) return properties.allowEmpty;
                //if (Prefs.DevMode) Log.Message($"properties.filter.AllowedDefCount : {properties.filter.AllowedDefCount}");
                return
                    ((CompModularizationWeapon)node)?.targetModeParent == null &&
                    properties.filter.Allows(node) &&
                    !internal_Unchangeable(node, properties);
            }
            return false;
        }


        protected override void PostPreApplyDamageWithRef(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            int count = ChildNodes.Count + 1;
            dinfo.SetAmount(dinfo.Amount / count);
            foreach (Thing thing in ChildNodes.InnerListForReading)
            {
                thing.TakeDamage(dinfo);
            }
        }


        protected override HashSet<string> RegiestedNodeId(HashSet<string> regiestedNodeId)
        {
            foreach(WeaponAttachmentProperties properties in Props.attachmentProperties) regiestedNodeId.Add(properties.id);
            return regiestedNodeId;
        }


        protected override void Added(NodeContainer container, string id)
        {
            targetModeParent = container.Comp;
            UsingTargetPart = ShowTargetPart;
        }


        protected override void Removed(NodeContainer container, string id)
        {
            targetModeParent = null;
            UsingTargetPart = ShowTargetPart;
        }


        protected override CompChildNodeProccesser OverrideParentProccesser(CompChildNodeProccesser orginal)
        {
            return UsingTargetPart ? (targetModeParent ?? orginal) : orginal;
        }


        #region operator
        public static implicit operator Thing(CompModularizationWeapon node)
        {
            return node?.parent;
        }

        public static implicit operator CompModularizationWeapon(Thing thing)
        {
            return thing?.TryGetComp<CompModularizationWeapon>();
        }
        #endregion


        private readonly Dictionary<string, bool> childTreeViewOpend = new Dictionary<string, bool>();
        private Dictionary<string, LocalTargetInfo> targetPartsWithId = new Dictionary<string, LocalTargetInfo>();
        private CompChildNodeProccesser targetModeParent;
        private bool showTargetPart = false;
        private bool usingTargetPart = false;
    }


    public class CompProperties_ModularizationWeapon : CompProperties
    {
        public Material PartTexMaterial
        {
            get
            {
                if (materialCache == null)
                {
                    Texture2D texture = (!PartTexPath.NullOrEmpty()) ? ContentFinder<Texture2D>.Get(PartTexPath) : null;
                    if(texture != null)
                    {
                        materialCache = new Material(ShaderDatabase.Cutout);
                        materialCache.mainTexture = texture;
                    }
                }
                return materialCache;
            }
        }


        public Texture2D PartTexture
        {
            get
            {
                return PartTexMaterial?.mainTexture as Texture2D;
            }
        }


        public CompProperties_ModularizationWeapon()
        {
            compClass = typeof(CompModularizationWeapon);
        }


        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach(string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }
            if(forceProjectile != null && typeof(Projectile).IsAssignableFrom(forceProjectile.thingClass))
            {
                forceProjectile = null;
                yield return "forceProjectile is not vaildity";
            }
            for (int i = attachmentProperties.Count - 1; i >= 0; i--)
            {
                WeaponAttachmentProperties properties = attachmentProperties[i];
                if(properties == null)
                {
                    attachmentProperties.RemoveAt(i);
                    yield return $"attachmentProperties[{i}] is null";
                    continue;
                }
                else if(properties.id.NullOrEmpty())
                {
                    attachmentProperties.RemoveAt(i);
                    yield return $"attachmentProperties[{i}].id is null or empty";
                    continue;
                }
                for (int j = 0; j < i; j++)
                {
                    WeaponAttachmentProperties propertiesForCompare = attachmentProperties[j];
                    if(!(propertiesForCompare?.id).NullOrEmpty() && propertiesForCompare.id == properties.id)
                    {
                        attachmentProperties.RemoveAt(i);
                        yield return $"attachmentProperties[{i}].id should be unique, but now repeat with attachmentProperties[{j}].id";
                        break;
                    }
                }
            }
        }


        public WeaponAttachmentProperties WeaponAttachmentPropertiesById(string id)
        {
            if(!id.NullOrEmpty())
            {
                foreach(WeaponAttachmentProperties properties in attachmentProperties)
                {
                    if(properties.id == id) return properties;
                }
            }
            return null;
        }


        public override void ResolveReferences(ThingDef parentDef)
        {
            foreach (WeaponAttachmentProperties properties in attachmentProperties)
            {
                properties.filter.ResolveReferences();
            }
        }


        #region Condation
        public bool unchangeable = false;


        public bool notDrawInParent = false;


        public bool notAllowParentUseTools = false;


        public bool notAllowParentUseVerbProperties = false;


        public bool useOriginalCraftMethod = false;


        public bool verbPropertiesAffectByOtherPart = false;


        public bool toolsAffectByOtherPart = false;


        public bool verbPropertiesAffectByChildPart = false;


        public bool toolsAffectByChildPart = false;
        #endregion


        #region Offset
        public float armorPenetrationOffset = 0;


        public float meleeCooldownTimeOffset = 0;


        public float meleeDamageOffset = 0;


        public float burstShotCountOffset = 0;


        public float ticksBetweenBurstShotsOffset = 0;


        public float muzzleFlashScaleOffset = 0;


        public float rangeOffset = 0;


        public float warmupTimeOffset = 0;


        public List<StatModifier> statOffset = new List<StatModifier>();
        #endregion


        #region Multiplier
        public float armorPenetrationMultiplier = 1f;


        public float meleeCooldownTimeMultiplier = 1f;


        public float meleeDamageMultiplier = 1f;


        public float burstShotCountMultiplier = 1f;


        public float ticksBetweenBurstShotsMultiplier = 1f;


        public float muzzleFlashScaleMultiplier = 1f;


        public float rangeMultiplier = 1f;


        public float warmupTimeMultiplier = 1f;


        public List<StatModifier> statMultiplier = new List<StatModifier>();
        #endregion


        #region Def
        public ThingDef forceProjectile = null;


        public SoundDef forceSound = null;


        public SoundDef forceSoundCastTail = null;
        #endregion


        public List<WeaponAttachmentProperties> attachmentProperties = new List<WeaponAttachmentProperties>();


        public string PartTexPath = null;


        private Material materialCache;
    }
}
