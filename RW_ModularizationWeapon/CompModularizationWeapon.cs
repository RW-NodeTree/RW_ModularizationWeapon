using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using RW_NodeTree.Rendering;
using RW_NodeTree.Tools;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace RW_ModularizationWeapon
{
    public class CompModularizationWeapon : CompBasicNodeComp, IEnumerable<(string,Thing,WeaponAttachmentProperties)>
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
            if (Props.setRandomPartWhenCreate)
            {
                System.Random random = new System.Random();
                foreach (WeaponAttachmentProperties properties in Props.attachmentProperties)
                {
                    int i = random.Next(properties.allowEmpty ? (properties.filter.AllowedDefCount + 1) : properties.filter.AllowedDefCount);
                    ThingDef def = i < properties.filter.AllowedDefCount ? properties.filter.AllowedThingDefs.ToList()[i] : null;
                    if (def != null)
                    {
                        Thing thing = ThingMaker.MakeThing(def, GenStuff.RandomStuffFor(def));
                        thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
                        ChildNodes[properties.id] = thing;
                    }
                }
            }
            else SetThingToDefault();
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref showTargetPart, "showTargetPart");
            Scribe_Values.Look(ref usingTargetPart, "usingTargetPart");
            Scribe_Collections.Look(ref targetPartsWithId, "targetPartsWithId", LookMode.Value, LookMode.LocalTargetInfo);
            if(Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                foreach(Thing thing in ChildNodes.Values)
                {
                    CompModularizationWeapon comp = thing;
                    if(comp != null)
                    {
                        comp.targetModeParent = NodeProccesser;
                        comp.UsingTargetPart = comp.ShowTargetPart;
                    }
                }
            }
        }


        public override bool AllowStackWith(Thing other)
        {
            return Props.attachmentProperties.Count == 0;
        }


        #region Condition
        public bool Unchangeable(string id) => internal_Unchangeable(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_Unchangeable(Thing thing, WeaponAttachmentProperties properties)
        {
            if(thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon comp = thing;
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
        internal static bool internal_NotDraw(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                CompModularizationWeapon comp = thing;
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
        internal static bool internal_NotUseTools(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                CompModularizationWeapon comp = thing;
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
        internal static bool internal_NotUseVerbProperties(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.notAllowParentUseVerbProperties || properties.notUseVerbProperties;
                }
                else
                {
                    return properties.notUseVerbProperties;
                }
            }
            return false;
        }

        public bool VerbPropertiesObjectPatchByOtherPart(string id) => internal_VerbPropertiesObjectPatchByOtherPart(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_VerbPropertiesObjectPatchByOtherPart(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.verbPropertiesObjectPatchByOtherPart && properties.verbPropertiesObjectPatchByOtherPart;
                }
                else
                {
                    return properties.verbPropertiesObjectPatchByOtherPart;
                }
            }
            return false;
        }

        public bool ToolsObjectPatchByOtherPart(string id) => internal_ToolsObjectPatchByOtherPart(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_ToolsObjectPatchByOtherPart(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.toolsObjectPatchByOtherPart && properties.toolsObjectPatchByOtherPart;
                }
                else
                {
                    return properties.toolsObjectPatchByOtherPart;
                }
            }
            return false;
        }

        public bool VerbPropertiesBoolAndPatchByOtherPart(string id) => internal_VerbPropertiesBoolAndPatchByOtherPart(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_VerbPropertiesBoolAndPatchByOtherPart(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.verbPropertiesBoolAndPatchByOtherPart && properties.verbPropertiesBoolAndPatchByOtherPart;
                }
                else
                {
                    return properties.verbPropertiesBoolAndPatchByOtherPart;
                }
            }
            return false;
        }

        public bool ToolsBoolAndPatchByOtherPart(string id) => internal_ToolsBoolAndPatchByOtherPart(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_ToolsBoolAndPatchByOtherPart(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.toolsBoolAndPatchByOtherPart && properties.toolsBoolAndPatchByOtherPart;
                }
                else
                {
                    return properties.toolsBoolAndPatchByOtherPart;
                }
            }
            return false;
        }

        public bool VerbPropertiesBoolOrPatchByOtherPart(string id) => internal_VerbPropertiesBoolOrPatchByOtherPart(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_VerbPropertiesBoolOrPatchByOtherPart(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.verbPropertiesBoolOrPatchByOtherPart && properties.verbPropertiesBoolOrPatchByOtherPart;
                }
                else
                {
                    return properties.verbPropertiesBoolOrPatchByOtherPart;
                }
            }
            return false;
        }

        public bool ToolsBoolOrPatchByOtherPart(string id) => internal_ToolsBoolOrPatchByOtherPart(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_ToolsBoolOrPatchByOtherPart(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.toolsBoolOrPatchByOtherPart && properties.toolsBoolOrPatchByOtherPart;
                }
                else
                {
                    return properties.toolsBoolOrPatchByOtherPart;
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
                    current = current.targetModeParent?.parent;
                }
                return result;
            }
            set
            {
                //Log.Message($"ShowTargetPart {parent} : {value}; org : {ShowTargetPart}");

                showTargetPart = value;
                UsingTargetPart = ShowTargetPart;
                NodeProccesser?.UpdateNode();
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
                    NodeProccesser?.UpdateNode();
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
            UsingTargetPart = false;
            foreach ((string id, LocalTargetInfo item) in targetPartsWithId)
            {
                Thing thing = ChildNodes[id];
                if (item.HasThing && item.Thing.Spawned) item.Thing.DeSpawn();
                ChildNodes[id] = item.Thing;
                if (thing != null && map != null && ChildNodes[id] != thing)
                {
                    GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near);
                }
            }

            ResetTargetPart();

            foreach (Thing item in ChildNodes.Values)
            {
                CompModularizationWeapon comp = item;
                if(comp != null)
                {
                    comp.ApplyTargetPart(pos, map);
                }
            }
        }


        public IEnumerator<(string,Thing,WeaponAttachmentProperties)> GetEnumerator()
        {
            foreach(string id in NodeProccesser.RegiestedNodeId)
            {
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if(properties != null) yield return (id,ChildNodes[id], properties);
            }
            yield break;
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion


        #region Offset
        public FieldReaderDgitList<VerbProperties> VerbPropertiesOffseter(string childNodeIdForVerbProperties)
        {
            FieldReaderDgitList<VerbProperties> results = new FieldReaderDgitList<VerbProperties>();
            WeaponAttachmentProperties current = Props.WeaponAttachmentPropertiesById(childNodeIdForVerbProperties);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForVerbProperties];
            NodeContainer container = ChildNodes;
            results.DefaultValue = 0;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForVerbProperties)
                {
                    FieldReaderDgitList<VerbProperties> cache = properties.verbPropertiesOffseterAffectHorizon;

                    if (current != null)
                    {
                        cache *=
                            current.verbPropertiesOtherPartOffseterAffectHorizon
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderDgitList<VerbProperties> result = new FieldReaderDgitList<VerbProperties>();
                                    result.DefaultValue = current.verbPropertiesOtherPartOffseterAffectHorizonDefaultValue;
                                    return result;
                                }
                            );
                        cache.DefaultValue = properties.verbPropertiesOffseterAffectHorizon.DefaultValue * current.verbPropertiesOtherPartOffseterAffectHorizonDefaultValue;
                        if (currentComp != null && comp != currentComp)
                        {
                            cache *= currentComp.Props.verbPropertiesOtherPartOffseterAffectHorizon;
                            cache.DefaultValue *= currentComp.Props.verbPropertiesOtherPartOffseterAffectHorizon;
                        }
                    }

                    cache = comp.Props.verbPropertiesOffseter * cache;
                    cache.DefaultValue = 0;
                    results += cache;
                    results.DefaultValue = 0;
                }
            }
            //Log.Message($"{this}.VerbPropertiesOffseter({childNodeIdForVerbProperties}) :\nresults : {results}");
            return results;
        }


        public FieldReaderDgitList<Tool> ToolsOffseter(string childNodeIdForTool)
        {
            FieldReaderDgitList<Tool> results = new FieldReaderDgitList<Tool>();
            WeaponAttachmentProperties current = Props.WeaponAttachmentPropertiesById(childNodeIdForTool);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForTool];
            NodeContainer container = ChildNodes;
            results.DefaultValue = 0;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForTool)
                {
                    FieldReaderDgitList<Tool> cache = properties.toolsOffseterAffectHorizon;

                    if (current != null)
                    {
                        cache *=
                            current.toolsOtherPartOffseterAffectHorizon
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderDgitList<Tool> result = new FieldReaderDgitList<Tool>();
                                    result.DefaultValue = current.toolsOtherPartOffseterAffectHorizonDefaultValue;
                                    return result;
                                }
                            );
                        cache.DefaultValue = properties.toolsOffseterAffectHorizon.DefaultValue * current.toolsOtherPartOffseterAffectHorizonDefaultValue;
                        if (currentComp != null && comp != currentComp)
                        {
                            cache *= currentComp.Props.toolsOtherPartOffseterAffectHorizon;
                            cache.DefaultValue *= currentComp.Props.toolsOtherPartOffseterAffectHorizon;
                        }
                    }

                    cache = comp.Props.toolsOffseter * cache;
                    cache.DefaultValue = 0;
                    results += cache;
                    results.DefaultValue = 0;
                }
            }
            //Log.Message($"{this}.ToolsOffseter({childNodeIdForTool}) :\nresults : {results}");
            return results;
        }


        public FieldReaderDgitList<CompProperties> CompPropertiesOffseter()
        {
            FieldReaderDgitList<CompProperties> results = new FieldReaderDgitList<CompProperties>();
            NodeContainer container = ChildNodes;
            results.DefaultValue = 0;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity)
                {
                    FieldReaderDgitList<CompProperties> cache = properties.compPropertiesOffseterAffectHorizon * comp.Props.compPropertiesOffseter;
                    cache.DefaultValue = 0;
                    results += cache;
                    results.DefaultValue = 0;
                }
            }
            //Log.Message($"{this}.ToolsOffseter({childNodeIdForTool}) :\nresults : {results}");
            return results;
        }

        public float GetStatOffset(StatDef statDef, Thing part)
        {
            NodeContainer container = ChildNodes;
            float result = 0;
            if (!statOffsetCache.TryGetValue((statDef, part), out result))
            {
                result = (container.IsChild(part) || part == parent) ? 0 : Props.statOffset.GetStatOffsetFromList(statDef);
                WeaponAttachmentProperties current = null;
                CompModularizationWeapon currentComp = null;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    CompModularizationWeapon comp = container[i];
                    if (comp != null && comp.Validity && (comp.ChildNodes.IsChild(part) || part == comp.parent))
                    {
                        current = Props.WeaponAttachmentPropertiesById(id);
                        currentComp = comp;
                        break;
                    }
                }

                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    CompModularizationWeapon comp = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if (comp != null && comp.Validity)
                    {
                        result += comp.GetStatOffset(statDef, part)
                            * (comp.ChildNodes.IsChild(part) ? 1f : (properties.statOffsetAffectHorizon.GetStatValueFromList(
                                    statDef,
                                    properties.statOffsetAffectHorizonDefaultValue
                                )
                                * (current?.statOtherPartOffseterAffectHorizon
                                .GetOrNewWhenNull(id, () => new List<StatModifier>())
                                .GetStatValueFromList(
                                    statDef,
                                    current.statOtherPartOffseterAffectHorizonDefaultValue
                                ) ?? 1)
                                * (currentComp != comp ? (currentComp?.Props.statOtherPartOffseterAffectHorizon ?? 1) : 1))
                            );
                    }
                }
                statOffsetCache.Add((statDef, part), result);
                //Log.Message($"{this}.GetStatOffset({statDef},{part})=>{result}\ncurrent.statOtherPartOffseterAffectHorizonDefaultValue : {current?.statOtherPartOffseterAffectHorizonDefaultValue}");
            }
            return result;
        }
        #endregion


        #region Multiplier
        public FieldReaderDgitList<VerbProperties> VerbPropertiesMultiplier(string childNodeIdForVerbProperties)
        {
            FieldReaderDgitList<VerbProperties> results = new FieldReaderDgitList<VerbProperties>();
            WeaponAttachmentProperties current = Props.WeaponAttachmentPropertiesById(childNodeIdForVerbProperties);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForVerbProperties];
            NodeContainer container = ChildNodes;
            results.DefaultValue = 1;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForVerbProperties)
                {
                    FieldReaderDgitList<VerbProperties> cache = properties.verbPropertiesMultiplierAffectHorizon;

                    if (current != null)
                    {
                        cache *=
                            current.verbPropertiesOtherPartMultiplierAffectHorizon
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderDgitList<VerbProperties> result = new FieldReaderDgitList<VerbProperties>();
                                    result.DefaultValue = current.verbPropertiesOtherPartMultiplierAffectHorizonDefaultValue;
                                    return result;
                                }
                            );
                        cache.DefaultValue = properties.verbPropertiesMultiplierAffectHorizon.DefaultValue * current.verbPropertiesOtherPartMultiplierAffectHorizonDefaultValue;
                        if (currentComp != null && comp != currentComp)
                        {
                            cache *= currentComp.Props.verbPropertiesOtherPartMultiplierAffectHorizon;
                            cache.DefaultValue *= currentComp.Props.verbPropertiesOtherPartMultiplierAffectHorizon;
                        }
                    }

                    FieldReaderDgitList<VerbProperties> mul = comp.Props.verbPropertiesMultiplier - 1f;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    cache = mul * cache + 1;
                    cache.DefaultValue = 1;
                    results *= cache;
                    results.DefaultValue = 1;
                    //result *= (comp.Props.verbPropertiesMultiplier - 1f) * properties.verbPropertiesMultiplierAffectHorizon + 1f;
                }
            }
            //Log.Message($" Final {this}.VerbPropertiesMultiplier({childNodeIdForVerbProperties}) :\nresults : {results}");
            return results;
        }


        public FieldReaderDgitList<Tool> ToolsMultiplier(string childNodeIdForTool)
        {
            FieldReaderDgitList<Tool> results = new FieldReaderDgitList<Tool>();
            WeaponAttachmentProperties current = Props.WeaponAttachmentPropertiesById(childNodeIdForTool);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForTool];
            NodeContainer container = ChildNodes;
            results.DefaultValue = 1;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForTool)
                {
                    FieldReaderDgitList<Tool> cache = properties.toolsMultiplierAffectHorizon;

                    if (current != null)
                    {
                        cache *=
                            current.toolsOtherPartMultiplierAffectHorizon
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderDgitList<Tool> result = new FieldReaderDgitList<Tool>();
                                    result.DefaultValue = current.toolsOtherPartMultiplierAffectHorizonDefaultValue;
                                    return result;
                                }
                            );
                        cache.DefaultValue = properties.toolsMultiplierAffectHorizon.DefaultValue * current.toolsOtherPartMultiplierAffectHorizonDefaultValue;
                        if (currentComp != null && comp != currentComp)
                        {
                            cache *= currentComp.Props.toolsOtherPartMultiplierAffectHorizon;
                            cache.DefaultValue *= currentComp.Props.toolsOtherPartMultiplierAffectHorizon;
                        }
                    }

                    FieldReaderDgitList<Tool> mul = comp.Props.toolsMultiplier - 1f;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    cache = mul * cache + 1;
                    cache.DefaultValue = 1;
                    results *= cache;
                    results.DefaultValue = 1;
                }
            }
            //Log.Message($"{this}.ToolsMultiplier({childNodeIdForTool}) :\nresults : {results}");
            return results;
        }


        public FieldReaderDgitList<CompProperties> CompPropertiesMultiplier()
        {
            FieldReaderDgitList<CompProperties> results = new FieldReaderDgitList<CompProperties>();
            NodeContainer container = ChildNodes;
            results.DefaultValue = 1;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity)
                {
                    FieldReaderDgitList<CompProperties> cache = comp.Props.compPropertiesMultiplier - 1;
                    if (cache.HasDefaultValue) cache.DefaultValue--;
                    cache = cache * properties.compPropertiesMultiplierAffectHorizon + 1;
                    cache.DefaultValue = 1;
                    results *= cache;
                    results.DefaultValue = 1;
                }
            }
            //Log.Message($"{this}.ToolsOffseter({childNodeIdForTool}) :\nresults : {results}");
            return results;
        }

        public float GetStatMultiplier(StatDef statDef, Thing part)
        {
            NodeContainer container = ChildNodes;
            float result = 1;
            if (!statMultiplierCache.TryGetValue((statDef, part), out result))
            {
                result = (container.IsChild(part) || part == parent) ? 1 : Props.statMultiplier.GetStatFactorFromList(statDef);
                WeaponAttachmentProperties current = null;
                CompModularizationWeapon currentComp = null;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    CompModularizationWeapon comp = container[i];
                    if (comp != null && comp.Validity && (comp.ChildNodes.IsChild(part) || part == comp.parent))
                    {
                        current = Props.WeaponAttachmentPropertiesById(id);
                        currentComp = comp;
                        break;
                    }
                }
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    CompModularizationWeapon comp = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    if (comp != null && comp.Validity)
                    {
                        result *= 1f + (comp.GetStatMultiplier(statDef, part) - 1f)
                            * (comp.ChildNodes.IsChild(part) ? 1f : (properties.statMultiplierAffectHorizon
                                .GetStatValueFromList(
                                    statDef,
                                    properties.statMultiplierAffectHorizonDefaultValue
                                )
                                * (current?.statOtherPartMultiplierAffectHorizon
                                .GetOrNewWhenNull(id, () => new List<StatModifier>())
                                .GetStatValueFromList(
                                    statDef,
                                    current.statOtherPartMultiplierAffectHorizonDefaultValue
                                ) ?? 1f)
                                * (currentComp != comp ? (currentComp?.Props.statOtherPartMultiplierAffectHorizon ?? 1f) : 1f))
                            );
                    }
                }
                statMultiplierCache.Add((statDef, part), result);
                //Log.Message($"{this}.GetStatMultiplier({statDef},{part})=>{result} \ncurrent.statOtherPartMultiplierAffectHorizonDefaultValue : {current?.statOtherPartMultiplierAffectHorizonDefaultValue}");
            }
            return result;
        }
        #endregion


        #region Patch
        public List<FieldReaderInst<VerbProperties>> VerbPropertiesObjectPatch(string childNodeIdForVerbProperties)
        {
            NodeContainer container = ChildNodes;
            List<FieldReaderInst<VerbProperties>> results = new List<FieldReaderInst<VerbProperties>>();
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && id != childNodeIdForVerbProperties && Props.WeaponAttachmentPropertiesById(id).verbPropertiesBoolAndPatchByChildPart)
                {
                    foreach (FieldReaderInst<VerbProperties> child in comp.Props.verbPropertiesObjectPatch)
                    {
                        int index = results.FindIndex(x => x.UsedType == child.UsedType);
                        if (index < 0) results.Add(child);
                        else results[index] |= child;
                    }
                }
            }
            return results;
        }
        

        public List<FieldReaderInst<Tool>> ToolsObjectPatch(string childNodeIdForTool)
        {
            NodeContainer container = ChildNodes;
            List<FieldReaderInst<Tool>> results = new List<FieldReaderInst<Tool>>();
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && id != childNodeIdForTool && Props.WeaponAttachmentPropertiesById(id).toolsBoolAndPatchByChildPart)
                {
                    foreach (FieldReaderInst<Tool> child in comp.Props.toolsObjectPatch)
                    {
                        int index = results.FindIndex(x => x.UsedType == child.UsedType);
                        if (index < 0) results.Add(child);
                        else results[index] |= child;
                    }
                }
            }
            return results;
        }



        public List<FieldReaderInst<CompProperties>> CompPropertiesObjectPatch()
        {
            NodeContainer container = ChildNodes;
            List<FieldReaderInst<CompProperties>> results = new List<FieldReaderInst<CompProperties>>();
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && Props.WeaponAttachmentPropertiesById(id).compPropertiesObjectPatchByChildPart)
                {
                    foreach (FieldReaderInst<CompProperties> child in comp.Props.compPropertiesObjectPatch)
                    {
                        int index = results.FindIndex(x => x.UsedType == child.UsedType);
                        if (index < 0) results.Add(child);
                        else results[index] |= child;
                    }
                }
            }
            return results;
        }


        public FieldReaderBoolList<VerbProperties> VerbPropertiesBoolAndPatch(string childNodeIdForVerbProperties)
        {
            NodeContainer container = ChildNodes;
            FieldReaderBoolList<VerbProperties> results = new FieldReaderBoolList<VerbProperties>();
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && id != childNodeIdForVerbProperties && Props.WeaponAttachmentPropertiesById(id).verbPropertiesBoolAndPatchByChildPart)
                {
                    results &= comp.Props.verbPropertiesBoolAndPatch;
                    results.DefaultValue = true;
                }
            }
            return results;
        }


        public FieldReaderBoolList<Tool> ToolsBoolAndPatch(string childNodeIdForTool)
        {
            NodeContainer container = ChildNodes;
            FieldReaderBoolList<Tool> results = new FieldReaderBoolList<Tool>();
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && id != childNodeIdForTool && Props.WeaponAttachmentPropertiesById(id).toolsBoolAndPatchByChildPart)
                {
                    results &= comp.Props.toolsBoolAndPatch;
                    results.DefaultValue = true;
                }
            }
            return results;
        }


        public FieldReaderBoolList<CompProperties> CompPropertiesBoolAndPatch()
        {
            NodeContainer container = ChildNodes;
            FieldReaderBoolList<CompProperties> results = new FieldReaderBoolList<CompProperties>();
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && Props.WeaponAttachmentPropertiesById(id).compPropertiesBoolAndPatchByChildPart)
                {
                    results &= comp.Props.compPropertiesBoolAndPatch;
                    results.DefaultValue = true;
                }
            }
            return results;
        }


        public FieldReaderBoolList<VerbProperties> VerbPropertiesBoolOrPatch(string childNodeIdForVerbProperties)
        {
            NodeContainer container = ChildNodes;
            FieldReaderBoolList<VerbProperties> results = new FieldReaderBoolList<VerbProperties>();
            results.DefaultValue = false;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && id != childNodeIdForVerbProperties && Props.WeaponAttachmentPropertiesById(id).verbPropertiesBoolOrPatchByChildPart)
                {
                    results |= comp.Props.verbPropertiesBoolOrPatch;
                    results.DefaultValue = false;
                }
            }
            return results;
        }


        public FieldReaderBoolList<Tool> ToolsBoolOrPatch(string childNodeIdForTool)
        {
            NodeContainer container = ChildNodes;
            FieldReaderBoolList<Tool> results = new FieldReaderBoolList<Tool>();
            results.DefaultValue = false;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && id != childNodeIdForTool && Props.WeaponAttachmentPropertiesById(id).toolsBoolOrPatchByChildPart)
                {
                    results |= comp.Props.toolsBoolOrPatch;
                    results.DefaultValue = false;
                }
            }
            return results;
        }


        public FieldReaderBoolList<CompProperties> CompPropertiesBoolOrPatch()
        {
            NodeContainer container = ChildNodes;
            FieldReaderBoolList<CompProperties> results = new FieldReaderBoolList<CompProperties>();
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && Props.WeaponAttachmentPropertiesById(id).compPropertiesBoolOrPatchByChildPart)
                {
                    results |= comp.Props.compPropertiesBoolOrPatch;
                    results.DefaultValue = true;
                }
            }
            return results;
        }
        #endregion


        #region Verb
        internal VerbProperties VerbPropertiesAfterAffect(VerbProperties properties, string childNodeIdForVerbProperties, bool verbPropertiesBoolAndPatch, bool verbPropertiesBoolOrPatch, bool verbPropertiesObjectPatch)
        {
            //properties = (VerbProperties)properties.SimpleCopy();
            properties *= VerbPropertiesMultiplier(childNodeIdForVerbProperties);
            properties += VerbPropertiesOffseter(childNodeIdForVerbProperties);
            if (verbPropertiesBoolAndPatch) properties &= VerbPropertiesBoolAndPatch(childNodeIdForVerbProperties);
            if (verbPropertiesBoolOrPatch) properties |= VerbPropertiesBoolOrPatch(childNodeIdForVerbProperties);
            if (verbPropertiesObjectPatch)
                VerbPropertiesObjectPatch(childNodeIdForVerbProperties)
                .ForEach(x =>
                {
                    //Log.Message(x.ToString());
                    properties &= x;
                    properties |= x;
                });
            return properties;
        }


        internal Tool ToolAfterAffect(Tool tool, string childNodeIdForTool, bool toolsBoolAndPatch, bool toolsBoolOrPatch, bool toolsObjectPatch)
        {
            //tool = (Tool)tool.SimpleCopy();
            tool *= ToolsMultiplier(childNodeIdForTool);
            tool += ToolsOffseter(childNodeIdForTool);
            if (toolsBoolAndPatch) tool &= ToolsBoolAndPatch(childNodeIdForTool);
            if (toolsBoolOrPatch) tool |= ToolsBoolOrPatch(childNodeIdForTool);
            if (toolsObjectPatch)
                ToolsObjectPatch(childNodeIdForTool)
                .ForEach(x =>
                {
                    //Log.Message(x.ToString());
                    tool &= x;
                    tool |= x;
                });
            return tool;
        }


        protected override List<VerbPropertiesRegiestInfo> PostIVerbOwner_GetVerbProperties(Type ownerType, List<VerbPropertiesRegiestInfo> result, Dictionary<string, object> forPostRead)
        {
            for (int i = 0; i < result.Count; i++)
            {
                VerbPropertiesRegiestInfo prop = result[i];
                VerbProperties newProp = VerbPropertiesAfterAffect(
                    prop.berforConvertProperties, 
                    null, 
                    Props.verbPropertiesBoolAndPatchByChildPart,
                    Props.verbPropertiesBoolOrPatchByChildPart,
                    Props.verbPropertiesObjectPatchByChildPart);
                prop.afterConvertProperties = newProp;
                result[i] = prop;
            }

            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                WeaponAttachmentProperties attachmentProperties = Props.WeaponAttachmentPropertiesById(id);
                if (!internal_NotUseVerbProperties(container[i], attachmentProperties))
                {
                    IVerbOwner verbOwner = CompChildNodeProccesser.GetSameTypeVerbOwner(ownerType, container[i]);
                    List<VerbProperties> verbProperties = verbOwner?.VerbProperties ?? container[i]?.def.Verbs;
                    if (verbProperties != null)
                    {
                        result.Capacity += verbProperties.Count;
                        CompModularizationWeapon comp = container[i];
                        if(comp != null && verbOwner == null)
                        {
                            for (int j = 0; j < verbProperties.Count; j++)
                            {
                                VerbProperties cache = verbProperties[j];
                                VerbProperties newProp 
                                    = VerbPropertiesAfterAffect(
                                        comp.VerbPropertiesAfterAffect(
                                            cache, 
                                            null,
                                            comp.Props.verbPropertiesBoolAndPatchByChildPart,
                                            comp.Props.verbPropertiesBoolOrPatchByChildPart,
                                            comp.Props.verbPropertiesObjectPatchByChildPart
                                            ), 
                                        id, 
                                        internal_VerbPropertiesBoolAndPatchByOtherPart(
                                            container[i],
                                            attachmentProperties),
                                        internal_VerbPropertiesBoolOrPatchByOtherPart(
                                            container[i],
                                            attachmentProperties),
                                        internal_VerbPropertiesObjectPatchByOtherPart(
                                            container[i],
                                            attachmentProperties)
                                        );
                                result.Add(new VerbPropertiesRegiestInfo(id, cache, newProp));
                            }
                        }
                        else
                        {
                            for (int j = 0; j < verbProperties.Count; j++)
                            {
                                VerbProperties cache = verbProperties[j];
                                VerbProperties newProp
                                    = VerbPropertiesAfterAffect(
                                        cache,
                                        id,
                                        internal_VerbPropertiesBoolAndPatchByOtherPart(
                                            container[i],
                                            attachmentProperties),
                                        internal_VerbPropertiesBoolOrPatchByOtherPart(
                                            container[i],
                                            attachmentProperties),
                                        internal_VerbPropertiesObjectPatchByOtherPart(
                                            container[i],
                                            attachmentProperties)
                                        );
                                result.Add(new VerbPropertiesRegiestInfo(id, cache, newProp));
                            }
                        }
                    }
                }
            }
            return result;
        }


        protected override List<VerbToolRegiestInfo> PostIVerbOwner_GetTools(Type ownerType, List<VerbToolRegiestInfo> result, Dictionary<string, object> forPostRead)
        {
            for (int i = 0; i < result.Count; i++)
            {
                VerbToolRegiestInfo prop = result[i];
                Tool newProp = ToolAfterAffect(
                    prop.berforConvertTool,
                    null,
                    Props.toolsBoolAndPatchByChildPart,
                    Props.toolsBoolOrPatchByChildPart,
                    Props.toolsObjectPatchByChildPart);
                prop.afterCobvertTool = newProp;
                result[i] = prop;
            }

            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                WeaponAttachmentProperties attachmentProperties = Props.WeaponAttachmentPropertiesById(id);
                if (!internal_NotUseTools(container[i], attachmentProperties))
                {
                    IVerbOwner verbOwner = CompChildNodeProccesser.GetSameTypeVerbOwner(ownerType, container[i]);
                    List<Tool> tools = verbOwner?.Tools ?? container[i]?.def.tools;
                    if (tools != null)
                    {
                        result.Capacity += tools.Count;
                        CompModularizationWeapon comp = container[i];
                        if (comp != null && verbOwner == null)
                        {
                            for (int j = 0; j < tools.Count; j++)
                            {
                                Tool cache = tools[j];
                                Tool newProp
                                    = ToolAfterAffect(
                                        comp.ToolAfterAffect(
                                            cache,
                                            null,
                                            comp.Props.toolsBoolAndPatchByChildPart,
                                            comp.Props.toolsBoolOrPatchByChildPart,
                                            comp.Props.toolsObjectPatchByChildPart
                                            ),
                                        id,
                                        internal_ToolsBoolAndPatchByOtherPart(
                                            container[i],
                                            attachmentProperties),
                                        internal_ToolsBoolOrPatchByOtherPart(
                                            container[i],
                                            attachmentProperties),
                                        internal_ToolsObjectPatchByOtherPart(
                                            container[i],
                                            attachmentProperties)
                                        );
                                result.Add(new VerbToolRegiestInfo(id, cache, newProp));
                            }
                        }
                        else
                        {
                            for (int j = 0; j < tools.Count; j++)
                            {
                                Tool cache = tools[j];
                                Tool newProp
                                    = ToolAfterAffect(
                                        cache,
                                        id,
                                        internal_ToolsBoolAndPatchByOtherPart(
                                            container[i],
                                            attachmentProperties),
                                        internal_ToolsBoolOrPatchByOtherPart(
                                            container[i],
                                            attachmentProperties),
                                        internal_ToolsObjectPatchByOtherPart(
                                            container[i],
                                            attachmentProperties)
                                        );
                                result.Add(new VerbToolRegiestInfo(id, cache, newProp));
                            }
                        }
                    }
                }
            }
            return result;
        }
        #endregion


        #region Stat
        private StatRequest RedirectoryReq(StatWorker statWorker, StatRequest req)
        {
            StatDef statDef = StatWorker_stat(statWorker);
            if (statDef.category == StatCategoryDefOf.Weapon || statDef.category == StatCategoryDefOf.Weapon_Ranged)
            {
                CompEquippable eq = req.Thing.TryGetComp<CompEquippable>();
                CompChildNodeProccesser proccesser = req.Thing;
                if (eq != null && proccesser != null)
                {
                    req = StatRequest.For(proccesser.GetBeforeConvertVerbCorrespondingThing(typeof(CompEquippable), eq.PrimaryVerb).Item1);
                }
            }
            return req;
        }


        protected override void PreStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, Dictionary<string, object> forPostRead)
        {
            StatRequest before = req;
            req = RedirectoryReq(statWorker, req);
            if(req.Thing == before.Thing)
            {
                if (req.Thing == parent)
                {
                    CompEquippable eq = parent.GetComp<CompEquippable>();
                    if (eq != null)
                    {
                        //if (Prefs.DevMode) Log.Message(" prefix before clear: parent.def.Verbs0=" + parent.def.Verbs.Count + "; parent.def.tools0=" + parent.def.tools.Count + ";\n");
                        List<Verb> verbs = eq.AllVerbs;
                        List<VerbProperties> cachedVerbs = new List<VerbProperties>();
                        List<Tool> cachedTools = new List<Tool>();
                        foreach (Verb verb in verbs)
                        {
                            if (verb.tool != null && !parent.def.tools.Contains(verb.tool)) cachedTools.Add(verb.tool);
                            else if (!parent.def.Verbs.Contains(verb.verbProps)) cachedVerbs.Add(verb.verbProps);
                        }
                        if(!cachedVerbs.NullOrEmpty())
                        {
                            ThingDef_verbs(parent.def) = ThingDef_verbs(parent.def) ?? new List<VerbProperties>();
                            forPostRead.Add("CompModularizationWeapon_verbs", ThingDef_verbs(parent.def));
                            ThingDef_verbs(parent.def) = cachedVerbs;
                        }
                        if(!cachedTools.NullOrEmpty())
                        {
                            forPostRead.Add("CompModularizationWeapon_tools", parent.def.tools);
                            parent.def.tools = cachedTools;
                        }
                        //if (Prefs.DevMode) Log.Message(" prefix after change: parent.def.Verbs.Count=" + parent.def.Verbs.Count + "; parent.def.tools.Count=" + parent.def.tools.Count + ";\n");
                    }
                }
                else ((CompChildNodeProccesser)req.Thing)?.PreStatWorker_GetValueUnfinalized(statWorker, req, applyPostProcess, forPostRead);
            }
        }


        protected override float PostStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            StatRequest before = req;
            req = RedirectoryReq(statWorker, req);
            if (req.Thing != before.Thing)
            {
                return statWorker.GetValueUnfinalized(req, applyPostProcess);
            }

            if (req.Thing == parent)
            {
                CompEquippable eq = parent.GetComp<CompEquippable>();
                if (eq != null)
                {
                    object
                    obj = forPostRead.TryGetValue("CompModularizationWeapon_verbs");
                    if(obj != null)
                    {
                        ThingDef_verbs(parent.def) = (List<VerbProperties>)obj;
                    }
                    obj = forPostRead.TryGetValue("CompModularizationWeapon_tools");
                    if (obj != null)
                    {
                        parent.def.tools = (List<Tool>)obj;
                    }
                }
            }
            else result = ((CompChildNodeProccesser)req.Thing)?.PostStatWorker_GetValueUnfinalized(statWorker, req, applyPostProcess, result, forPostRead) ?? result;
            return result;
        }


        protected override float PreStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            StatRequest before = req;
            req = RedirectoryReq(statWorker, req);
            if (before.Thing != req.Thing)
            {
                statWorker.FinalizeValue(req, ref result, applyPostProcess);
                forPostRead.Add("afterRedirectoryReq", result);
                //Log.Message($"{StatWorker_stat(statWorker)}.FinalizeValue({req})  afterRedirectoryReq : {result}");
            }
            return result;
        }


        protected override float PostStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {

            if (forPostRead.TryGetValue("afterRedirectoryReq", out object cache))
            {
                //Log.Message($"{StatWorker_stat(statWorker)}.FinalizeValue({req})  afterRedirectoryReq : {result}");
                return (float)cache;
            }
            if (req.Thing == parent)
            {
                if (statWorker is StatWorker_MarketValue || statWorker == StatDefOf.Mass.Worker)
                {
                    foreach (Thing thing in ChildNodes.Values)
                    {
                        result += statWorker.GetValue(thing);
                    }
                }
                else if (!(statWorker is StatWorker_MeleeAverageArmorPenetration || statWorker is StatWorker_MeleeAverageDPS))
                {
                    StatDef statDef = StatWorker_stat(statWorker);
                    result *= GetStatMultiplier(statDef, req.Thing);
                    result += GetStatOffset(statDef, req.Thing);
                }
            }
            else if (statWorker is StatWorker_MeleeAverageDPS ||
                    statWorker is StatWorker_MeleeAverageArmorPenetration ||
                    statWorker is StatWorker_MarketValue ||
                    statWorker == StatDefOf.Mass.Worker
                )
            {
                result = ((CompChildNodeProccesser)req.Thing)?.PostStatWorker_FinalizeValue(statWorker, req, applyPostProcess, result, forPostRead) ?? result;
            }
            else
            {
                StatDef statDef = StatWorker_stat(statWorker);
                result *= GetStatMultiplier(statDef, req.Thing);
                result += GetStatOffset(statDef, req.Thing);
            }
            return result;
        }


        protected override string PostStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, string result, Dictionary<string, object> forPostRead)
        {
            StatRequest before = req;
            req = RedirectoryReq(statWorker, req);
            if (before.Thing != req.Thing)
            {
                return statWorker.GetExplanationUnfinalized(req, numberSense);
            }
            if (req.Thing == parent)
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (statWorker is StatWorker_MeleeAverageDPS || 
                    statWorker is StatWorker_MeleeAverageArmorPenetration || 
                    statWorker is StatWorker_MarketValue ||
                    statWorker == StatDefOf.Mass.Worker
                )
                {
                    foreach ((string id, Thing thing) in ChildNodes)
                    {
                        if(!NotUseTools(id))
                        {
                            stringBuilder.AppendLine("  " + thing.Label + ":");
                            string exp = "\n" + statWorker.GetExplanationUnfinalized(StatRequest.For(thing), numberSense);
                            exp = Regex.Replace(exp, "\n", "\n  ");
                            stringBuilder.AppendLine(exp);
                        }
                    }
                    result += "\n" + stringBuilder.ToString();
                }
            }
            else
            {
                result = ((CompChildNodeProccesser)req.Thing)?.PostStatWorker_GetExplanationUnfinalized(statWorker, req, numberSense, result, forPostRead) ?? result;
            }
            return result;
        }


        protected override IEnumerable<Dialog_InfoCard.Hyperlink> PostStatWorker_GetInfoCardHyperlinks(StatWorker statWorker, StatRequest statRequest, IEnumerable<Dialog_InfoCard.Hyperlink> result)
        {
            statRequest = RedirectoryReq(statWorker, statRequest);
            foreach (Dialog_InfoCard.Hyperlink link in result)
            {
                yield return link;
            }
            if (statRequest.Thing == parent)
            {
                if (statWorker is StatWorker_MeleeAverageDPS ||
                    statWorker is StatWorker_MeleeAverageArmorPenetration ||
                    statWorker is StatWorker_MarketValue ||
                    statWorker == StatDefOf.Mass.Worker
                )
                {
                    foreach (Thing thing in ChildNodes.Values)
                    {
                        yield return new Dialog_InfoCard.Hyperlink(thing);
                    }
                }
            }
        }


        protected override IEnumerable<StatDrawEntry> PostThingDef_SpecialDisplayStats(ThingDef def, StatRequest req, IEnumerable<StatDrawEntry> result)
        {
            //Log.Message($"PostThingDef_SpecialDisplayStats({def},{req},{result})");
            if (req.Thing == parent)
            {
                List<VerbProperties> verbProperties = null;
                List<Tool> tools = null;
                CompEquippable eq = parent.GetComp<CompEquippable>();
                if (eq != null)
                {
                    //if (Prefs.DevMode) Log.Message(" prefix before clear: parent.def.Verbs0=" + parent.def.Verbs.Count + "; parent.def.tools0=" + parent.def.tools.Count + ";\n");
                    List<Verb> verbs = eq.AllVerbs;
                    List<VerbProperties> cachedVerbs = new List<VerbProperties>();
                    List<Tool> cachedTools = new List<Tool>();
                    foreach (Verb verb in verbs)
                    {
                        if (verb.tool != null && !parent.def.tools.Contains(verb.tool)) cachedTools.Add(verb.tool);
                        else if (!parent.def.Verbs.Contains(verb.verbProps)) cachedVerbs.Add(verb.verbProps);
                    }
                    if (!cachedVerbs.NullOrEmpty())
                    {
                        ThingDef_verbs(parent.def) = ThingDef_verbs(parent.def) ?? new List<VerbProperties>();
                        verbProperties = ThingDef_verbs(parent.def);
                        ThingDef_verbs(parent.def) = cachedVerbs;
                    }
                    if (!cachedTools.NullOrEmpty())
                    {
                        tools = parent.def.tools;
                        parent.def.tools = cachedTools;
                    }
                }
                List<CompProperties> compProperties = def.comps;
                def.comps = (from x in parent.AllComps select x.props).ToList();

                foreach (StatDrawEntry entry in result)
                {
                    yield return entry;
                }
                if (verbProperties != null)
                {
                    ThingDef_verbs(parent.def) = verbProperties;
                }
                if (tools != null)
                {
                    parent.def.tools = tools;
                }

                def.comps = compProperties;

            }
            else
            {
                result = ((CompChildNodeProccesser)req.Thing)?.PostThingDef_SpecialDisplayStats(def, req, result) ?? result;
                foreach (StatDrawEntry entry in result)
                {
                    yield return entry;
                }
            }
        }


        protected override IEnumerable<StatDrawEntry> PostStatsReportUtility_StatsToDraw(Thing thing, IEnumerable<StatDrawEntry> result)
        {
            //Log.Message($"PostStatsReportUtility_StatsToDraw({thing},{result})");
            if (thing == parent)
            {
                List<VerbProperties> verbProperties = null;
                List<Tool> tools = null;
                CompEquippable eq = parent.GetComp<CompEquippable>();
                if (eq != null)
                {
                    //if (Prefs.DevMode) Log.Message(" prefix before clear: parent.def.Verbs0=" + parent.def.Verbs.Count + "; parent.def.tools0=" + parent.def.tools.Count + ";\n");
                    List<Verb> verbs = eq.AllVerbs;
                    List<VerbProperties> cachedVerbs = new List<VerbProperties>();
                    List<Tool> cachedTools = new List<Tool>();
                    foreach (Verb verb in verbs)
                    {
                        if (verb.tool != null && !parent.def.tools.Contains(verb.tool)) cachedTools.Add(verb.tool);
                        else if (!parent.def.Verbs.Contains(verb.verbProps)) cachedVerbs.Add(verb.verbProps);
                    }
                    if (!cachedVerbs.NullOrEmpty())
                    {
                        ThingDef_verbs(parent.def) = ThingDef_verbs(parent.def) ?? new List<VerbProperties>();
                        verbProperties = ThingDef_verbs(parent.def);
                        ThingDef_verbs(parent.def) = cachedVerbs;
                    }
                    if (!cachedTools.NullOrEmpty())
                    {
                        tools = parent.def.tools;
                        parent.def.tools = cachedTools;
                    }
                }

                List<CompProperties> compProperties = parent.def.comps;
                parent.def.comps = (from x in parent.AllComps select x.props).ToList();

                foreach (StatDrawEntry entry in result)
                {
                    yield return entry;
                }
                if (verbProperties != null)
                {
                    ThingDef_verbs(parent.def) = verbProperties;
                }
                if (tools != null)
                {
                    parent.def.tools = tools;
                }

                parent.def.comps = compProperties;
            }
            else
            {
                result = ((CompChildNodeProccesser)thing)?.PostStatsReportUtility_StatsToDraw(thing, result) ?? result;
                foreach (StatDrawEntry entry in result)
                {
                    yield return entry;
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
            Vector2 result = new Vector2(BlockSize.x,0);
            foreach((string id, Thing thing, WeaponAttachmentProperties properties) in this)
            {
                result.y += BlockSize.y;
                if(id != null && GetChildTreeViewOpend(id))
                {
                    CompModularizationWeapon comp = thing;
                    if (comp != null)
                    {
                        Vector2 childSize = comp.TreeViewDrawSize(BlockSize);
                        result.y += childSize.y;
                        result.x = Math.Max(childSize.x + BlockSize.y, result.x);
                    }
                }
            }
            return result;
        }

        
        public float DrawChildTreeView(
            Vector2 DrawPos,
            float ScrollPos,
            float BlockHeight,
            float ContainerWidth,
            float ContainerHeight,
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
            foreach ((string id, Thing thing, WeaponAttachmentProperties properties) in this)
            {
                if(id != null)
                {
                    if (currentPos.y + BlockHeight > ScrollPos && currentPos.y < ScrollPos + ContainerHeight)
                    {
                        if (Selected?.Contains((id, this)) ?? false) Widgets.DrawBoxSolidWithOutline(new Rect(currentPos.x, currentPos.y, ContainerWidth, BlockHeight), new Color32(51, 153, 255, 64), new Color32(51, 153, 255, 96));
                        else if (GetChildTreeViewOpend(id)) Widgets.DrawHighlightSelected(new Rect(currentPos.x, currentPos.y, ContainerWidth, BlockHeight));
                        Widgets.DrawHighlightIfMouseover(new Rect(currentPos.x, currentPos.y, ContainerWidth, BlockHeight));//hover
                    }

                    bool opend = GetChildTreeViewOpend(id);

                    if (currentPos.y + BlockHeight > ScrollPos && currentPos.y < ScrollPos + ContainerHeight)
                    {
                        if (Widgets.ButtonInvisible(new Rect(currentPos.x + BlockHeight, currentPos.y, ContainerWidth - BlockHeight, BlockHeight)))
                        {
                            opend = !opend;
                            if (opend) openEvent?.Invoke(id, thing, this);
                            else closeEvent?.Invoke(id, thing, this);
                            SetChildTreeViewOpend(id, opend);
                        }
                    }
                    if (thing != null)
                    {
                        CompModularizationWeapon comp = thing;
                        if (currentPos.y + BlockHeight > ScrollPos && currentPos.y < ScrollPos + ContainerHeight)
                        {
                            ThingStyleDef styleDef = thing.StyleDef;
                            CompChildNodeProccesser comp_targetModeParent = (thing.def.graphicData != null && (styleDef == null || styleDef.UIIcon == null) && thing.def.uiIconPath.NullOrEmpty() && !(thing is Pawn || thing is Corpse)) ? comp?.targetModeParent : null;
                            if (comp_targetModeParent != null)
                            {
                                comp.NodeProccesser.ResetRenderedTexture();
                                comp.targetModeParent = null;
                            }
                            Widgets.ThingIcon(new Rect(currentPos.x + 1, currentPos.y + 1, BlockHeight - 1, BlockHeight - 2), thing);
                            if (comp_targetModeParent != null)
                            {
                                comp.NodeProccesser.ResetRenderedTexture();
                                comp.targetModeParent = comp_targetModeParent;
                            }
                            Widgets.Label(new Rect(currentPos.x + BlockHeight, currentPos.y + 1, ContainerWidth - BlockHeight - 1, BlockHeight - 2), $"{properties.Name} : {thing.Label}");

                            if (Widgets.ButtonInvisible(new Rect(currentPos.x, currentPos.y, BlockHeight, BlockHeight)))
                            {
                                iconEvent?.Invoke(id, thing, this);
                            }
                        }
                        if (comp != null)
                        {
                            if (opend)
                            {
                                currentPos.y += comp.DrawChildTreeView(
                                    currentPos + Vector2.one * BlockHeight,
                                    ScrollPos,
                                    BlockHeight,
                                    ContainerWidth - BlockHeight,
                                    ContainerHeight,
                                    openEvent,
                                    closeEvent,
                                    iconEvent,
                                    Selected
                                );
                            }
                        }
                        else if (currentPos.y + BlockHeight > ScrollPos && currentPos.y < ScrollPos + ContainerHeight && Widgets.ButtonInvisible(new Rect(currentPos.x + BlockHeight, currentPos.y, ContainerWidth - BlockHeight, BlockHeight)))
                        {
                            openEvent?.Invoke(id, thing, this);
                        }
                    }
                    else if (currentPos.y + BlockHeight > ScrollPos && currentPos.y < ScrollPos + ContainerHeight)
                    {
                        Widgets.DrawTextureFitted(new Rect(currentPos.x, currentPos.y,BlockHeight,BlockHeight), properties.UITexture,1);
                        Widgets.Label(new Rect(currentPos.x+BlockHeight, currentPos.y,ContainerWidth-BlockHeight,BlockHeight), properties.Name);
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


        #region AI
        internal IEnumerable<Toil> CarryTarget(TargetIndex craftingTable, TargetIndex hauledThingIndex)
        {
            foreach(string id in NodeProccesser.RegiestedNodeId)
            {
                LocalTargetInfo target = ChildNodes[id];
                if (targetPartsWithId.ContainsKey(id))
                {
                    target = targetPartsWithId[id];
                    if (target.HasThing && target.Thing.Spawned)
                    {
                        Toil toil = new Toil();
                        toil.initAction = delegate ()
                        {
                            Pawn actor = toil.actor;
                            Job job = actor.CurJob;
                            job.SetTarget(hauledThingIndex, target);
                            job.count = 1;
                        };
                        yield return toil;

                        yield return
                            Toils_Goto.GotoThing(hauledThingIndex, PathEndMode.ClosestTouch)
                            .FailOnDestroyedNullOrForbidden(hauledThingIndex)
                            .FailOnBurningImmobile(hauledThingIndex);
                        yield return
                            Toils_Haul.StartCarryThing(hauledThingIndex)
                            .FailOnCannotTouch(hauledThingIndex, PathEndMode.ClosestTouch);
                        yield return
                            Toils_Haul.CarryHauledThingToCell(craftingTable, PathEndMode.ClosestTouch)
                            .FailOnDestroyedNullOrForbidden(craftingTable)
                            .FailOnBurningImmobile(craftingTable);
                        yield return
                            Toils_Haul.PlaceCarriedThingInCellFacing(craftingTable)
                            .FailOnCannotTouch(craftingTable, PathEndMode.ClosestTouch);

                        toil = new Toil();
                        toil.initAction = delegate ()
                        {
                            Pawn actor = toil.actor;
                            Job job = actor.CurJob;
                            targetPartsWithId[id] = job.GetTarget(hauledThingIndex);
                            targetPartsWithId[id].Thing.Position = job.GetTarget(craftingTable).Cell;
                            actor.Reserve(targetPartsWithId[id], job, 1, 1);
                        };
                        yield return toil;
                    }

                }

                CompModularizationWeapon comp = target.Thing;
                if (comp != null)
                {
                    foreach (Toil child in comp.CarryTarget(craftingTable, hauledThingIndex))
                    {
                        yield return child;
                    }
                }

            }
        }

        internal IEnumerable<LocalTargetInfo> AllTargetPart()
        {
            foreach (string id in NodeProccesser.RegiestedNodeId)
            {
                LocalTargetInfo target = ChildNodes[id];
                if (targetPartsWithId.ContainsKey(id))
                {
                    target = targetPartsWithId[id];
                    yield return target;
                }
                CompModularizationWeapon comp = target.Thing;
                if (comp != null)
                {
                    foreach (LocalTargetInfo childTarget in comp.AllTargetPart())
                    {
                        yield return childTarget;
                    }
                }
            }
            yield break;
        }
        #endregion


        protected override List<(Thing, string, List<RenderInfo>)> OverrideDrawSteep(List<(Thing, string, List<RenderInfo>)> nodeRenderingInfos, Rot4 rot, Graphic graphic)
        {
            Matrix4x4 scale = Matrix4x4.identity;
            for (int i = 0; i < nodeRenderingInfos.Count; i++)
            {
                (Thing part, string id, List<RenderInfo> renderInfos) = nodeRenderingInfos[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (id.NullOrEmpty() && part == parent)
                {
                    //Log.Message($"ParentProccesser : {ParentProccesser}");
                    if (ParentProccesser != null)
                    {
                        Material material = graphic?.MatAt(rot, this.parent);
                        for (int j = 0; j < renderInfos.Count; j++)
                        {
                            RenderInfo info = renderInfos[j];
                            if (info.material == material)
                            {
                                info.material = Props.PartTexMaterial ?? info.material;
                                scale.m00 = Props.DrawSizeWhenAttach.x / info.mesh.bounds.size.x;
                                scale.m22 = Props.DrawSizeWhenAttach.y / info.mesh.bounds.size.z;
                                renderInfos[j] = info;
                                break;
                            }
                        }
                    }
                    break;
                }
            }
            for (int i = 0; i < nodeRenderingInfos.Count; i++)
            {
                (Thing part, string id, List<RenderInfo> renderInfos) = nodeRenderingInfos[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if(id.NullOrEmpty() && part == parent)
                {
                    if (ParentProccesser != null)
                    {
                        Material material = graphic?.MatAt(rot, this.parent);
                        for (int j = 0; j < renderInfos.Count; j++)
                        {
                            RenderInfo info = renderInfos[j];
                            Matrix4x4[] matrix = info.matrices;

                            for (int k = 0; k < matrix.Length; k++)
                            {
                                matrix[k] = scale * matrix[k];
                            }
                        }
                    }
                }
                else if (!internal_NotDraw(part, properties))
                {
                    if (properties != null)
                    {
                        for (int j = 0; j < renderInfos.Count; j++)
                        {
                            RenderInfo info = renderInfos[j];
                            Matrix4x4[] matrix = info.matrices;
                            for (int k = 0; k < matrix.Length; k++)
                            {
                                Vector4 cache = matrix[k].GetRow(0);
                                matrix[k].SetRow(0, new Vector4(new Vector3(cache.x, cache.y, cache.z).magnitude, 0, 0, cache.w));

                                cache = matrix[k].GetRow(1);
                                matrix[k].SetRow(1, new Vector4(0, new Vector3(cache.x, cache.y, cache.z).magnitude, 0, cache.w));

                                cache = matrix[k].GetRow(2);
                                matrix[k].SetRow(2, new Vector4(0, 0, new Vector3(cache.x, cache.y, cache.z).magnitude, cache.w));

                                matrix[k] = properties.Transfrom * scale * matrix[k];
                                //matrix[k] = properties.Transfrom;
                            }
                            renderInfos[j] = info;
                        }
                    }
                }
                else
                {
                    renderInfos.Clear();
                }
            }
            nodeRenderingInfos.SortBy(x =>
            {
                for (int i = 0; i < Props.attachmentProperties.Count; i++)
                {
                    WeaponAttachmentProperties properties = Props.attachmentProperties[i];
                    if (properties.id == x.Item2) return i;
                }
                return -1;
            });
            nodeRenderingInfos.SortBy(x => Props.WeaponAttachmentPropertiesById(x.Item2)?.drawWeight ?? 0);
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
                    !internal_Unchangeable(ChildNodes[id], properties);
            }
            return false;
        }


        public void SetThingToDefault()
        {
            foreach (WeaponAttachmentProperties properties in Props.attachmentProperties)
            {
                ThingDef def = properties.defultThing;
                if (def != null)
                {
                    Thing thing = ThingMaker.MakeThing(def, GenStuff.RandomStuffFor(def));
                    thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
                    ChildNodes[properties.id] = thing;
                }
            }
        }


        protected override IEnumerable<Thing> PostGenRecipe_MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient1, IBillGiver billGiver, Precept_ThingStyle precept, RecipeInvokeSource invokeSource, IEnumerable<Thing> result)
        {
            List<Thing> things = result.ToList();
            if(invokeSource == RecipeInvokeSource.products)
            {
                SetThingToDefault();
                foreach(Thing thing in ChildNodes.Values)
                {
                    ((CompModularizationWeapon)thing)?.SetThingToDefault();
                }
            }
            return things;
        }


        protected override bool PostThingWithComps_PreApplyDamage(ref DamageInfo dinfo, bool absorbed)
        {
            int count = ChildNodes.Count + 1;
            dinfo.SetAmount(dinfo.Amount / count);
            foreach (Thing thing in ChildNodes.Values)
            {
                thing.TakeDamage(dinfo);
            }
            return absorbed;
        }


        internal CompProperties CompPropertiesAfterAffect(CompProperties compProperties)
        {
            //tool = (Tool)tool.SimpleCopy();
            compProperties *= CompPropertiesMultiplier();
            compProperties += CompPropertiesOffseter();
            compProperties &= CompPropertiesBoolAndPatch();
            compProperties |= CompPropertiesBoolOrPatch();
            CompPropertiesObjectPatch()
            .ForEach(x =>
            {
                //Log.Message(x.ToString());
                compProperties &= x;
                compProperties |= x;
            });
            return compProperties;
        }


        protected override bool UpdateNode(CompChildNodeProccesser actionNode)
        {
            statOffsetCache.Clear();
            statMultiplierCache.Clear();

            foreach(ThingComp comp in parent.AllComps)
            {
                if (comp == this) continue;
                CompProperties properties = null;
                foreach(CompProperties def in parent.def.comps)
                {
                    if(def.compClass == comp.GetType())
                    {
                        properties = def;
                        break;
                    }
                }
                if(properties != null)
                {
                    comp.props = CompPropertiesAfterAffect(properties);
                }
            }

            return false;
        }


        protected override HashSet<string> RegiestedNodeId(HashSet<string> regiestedNodeId)
        {
            foreach(WeaponAttachmentProperties properties in Props.attachmentProperties) regiestedNodeId.Add(properties.id);
            return regiestedNodeId;
        }


        protected override void Added(NodeContainer container, string id)
        {
            //Log.Message($"container add {container.Comp}");
            targetModeParent = container.Comp;
            UsingTargetPart = ShowTargetPart;
            NodeProccesser.NeedUpdate = true;
        }


        protected override void Removed(NodeContainer container, string id)
        {
            //Log.Message($"container remove {container.Comp}");
            targetModeParent = null;
            UsingTargetPart = ShowTargetPart;
            NodeProccesser.NeedUpdate = true;
            NodeProccesser.UpdateNode();
        }


        protected override CompChildNodeProccesser OverrideParentProccesser(CompChildNodeProccesser orginal) => UsingTargetPart ? targetModeParent : orginal;


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


        internal CompChildNodeProccesser targetModeParent;
        private readonly Dictionary<string, bool> childTreeViewOpend = new Dictionary<string, bool>();
        private readonly Dictionary<(StatDef, Thing), float> statOffsetCache = new Dictionary<(StatDef, Thing), float>();
        private readonly Dictionary<(StatDef, Thing), float> statMultiplierCache = new Dictionary<(StatDef, Thing), float>();
        private Dictionary<string, LocalTargetInfo> targetPartsWithId = new Dictionary<string, LocalTargetInfo>();
        private bool showTargetPart = false;
        private bool usingTargetPart = false;

        private static AccessTools.FieldRef<StatWorker, StatDef> StatWorker_stat = AccessTools.FieldRefAccess<StatWorker, StatDef>("stat");
        private static AccessTools.FieldRef<ThingDef, List<VerbProperties>> ThingDef_verbs = AccessTools.FieldRefAccess<ThingDef, List<VerbProperties>>("verbs");
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
            for (int i = attachmentProperties.Count - 1; i >= 0; i--)
            {
                WeaponAttachmentProperties properties = attachmentProperties[i];
                if(properties == null)
                {
                    attachmentProperties.RemoveAt(i);
                    yield return $"attachmentProperties[{i}] is null";
                    continue;
                }
                else if(!properties.id.IsVaildityKeyFormat())
                {
                    attachmentProperties.RemoveAt(i);
                    yield return $"attachmentProperties[{i}].id is invaild key format";
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
                properties.ResolveReferences();
            }
            foreach (WeaponAttachmentProperties properties in attachmentProperties)
            {
                properties.verbPropertiesOtherPartOffseterAffectHorizon.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.toolsOtherPartOffseterAffectHorizon.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.verbPropertiesOtherPartMultiplierAffectHorizon.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.toolsOtherPartMultiplierAffectHorizon.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
            }
            if (attachmentProperties.Count > 0) parentDef.stackLimit = 1;


            void CheckAndSetListDgit<T>(ref FieldReaderDgitList<T> list, float defaultValue)
            {
                list = list ?? new FieldReaderDgitList<T>();
                list.RemoveAll(f => f == null);
                list.DefaultValue = defaultValue;
            }

            void CheckAndSetListBool<T>(ref FieldReaderBoolList<T> list, bool defaultValue)
            {
                list = list ?? new FieldReaderBoolList<T>();
                list.RemoveAll(f => f == null);
                list.DefaultValue = defaultValue;
            }

            CheckAndSetListDgit(ref verbPropertiesOffseter, 0);
            CheckAndSetListDgit(ref toolsOffseter, 0);

            CheckAndSetListDgit(ref verbPropertiesMultiplier, 1);
            CheckAndSetListDgit(ref toolsMultiplier, 1);

            verbPropertiesObjectPatch = verbPropertiesObjectPatch ?? new List<FieldReaderInst<VerbProperties>>();
            verbPropertiesObjectPatch.RemoveAll(f => f == null);
            toolsObjectPatch = toolsObjectPatch ?? new List<FieldReaderInst<Tool>>();
            toolsObjectPatch.RemoveAll(f => f == null);

            CheckAndSetListBool(ref verbPropertiesBoolAndPatch, true);
            CheckAndSetListBool(ref toolsBoolAndPatch, true);

            CheckAndSetListBool(ref verbPropertiesBoolOrPatch, false);
            CheckAndSetListBool(ref toolsBoolOrPatch, false);
        }


        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            CompModularizationWeapon comp = req.Thing;
            StringBuilder stringBuilder = new StringBuilder();

            int listAll<T>(
                FieldReaderDgitList<T> list,
                bool mul,
                bool snap = false
                )
            {
                int result = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (snap) stringBuilder.Append("  ");
                    stringBuilder.AppendLine($"  NO.{i+1} :");
                    foreach ((FieldInfo field, double value) in list[i])
                    {
                        if (snap) stringBuilder.Append("  ");
                        stringBuilder.AppendLine($"    {field.Name.Translate()} : {(mul ? "x" : "+")}{value}");
                    }
                    result += list[i].Count;
                }
                return result;
            }
            int count = statOffset.Count;
            stringBuilder.AppendLine("verbPropertiesOffseter".Translate().RawText + " :");
            count += listAll(verbPropertiesOffseter, false);

            stringBuilder.AppendLine("toolsOffseter".Translate().RawText + " :");
            count += listAll(toolsOffseter, false);

            stringBuilder.AppendLine("statOffseter".Translate().RawText + " :");
            foreach (StatModifier stat in statOffset)
            {
                stringBuilder.AppendLine($"  {stat.stat.LabelCap} : +{comp?.GetStatOffset(stat.stat, null) ?? stat.value}");
            }
            yield return new StatDrawEntry(
                StatCategoryDefOf.Weapon,
                "Offset".Translate(),
                count + " " + "Offseter".Translate(),
                stringBuilder.ToString(),
                1000
                );

            stringBuilder.Clear();
            count = statMultiplier.Count;
            stringBuilder.AppendLine("verbPropertiesMultiplier".Translate().RawText + " :");
            count += listAll(verbPropertiesMultiplier, true);

            stringBuilder.AppendLine("toolsMultiplier".Translate().RawText + " :");
            count += listAll(toolsMultiplier, true);

            stringBuilder.AppendLine("statMultiplier".Translate().RawText + " :");
            foreach (StatModifier stat in statMultiplier)
            {
                stringBuilder.AppendLine($"  {stat.stat.LabelCap} : x{comp?.GetStatMultiplier(stat.stat, null) ?? stat.value}");
            }
            yield return new StatDrawEntry(
                StatCategoryDefOf.Weapon, 
                "Multiplier".Translate(),
                count + " " + "Multiplier".Translate(), 
                stringBuilder.ToString(), 
                1000
                );

            stringBuilder.Clear();

            string CheckAndMark(bool flag, string name)
            {
                string result = "<color=" + (flag ? "#d9ead3><b>" : "grey>");
                result += name + " : " + (flag ? ("Yes".Translate().RawText + "</b>") : "No".Translate().RawText);
                return result += "</color>";
            }

            //UnityEngine.GUIUtility.systemCopyBuffer = "<color=" + (unchangeable ? "green" : "red") + ">" + "unchangeable".Translate() + " : " + (unchangeable ? "Yes".Translate() : "No".Translate()) + "</color>";
            //stringBuilder.AppendLine("<color=" + (unchangeable ? "green" : "red") + ">" + "unchangeable" + " : " + (unchangeable ? "Yes" : "No") + "</color>");
            stringBuilder.AppendLine(CheckAndMark(unchangeable, "unchangeable".Translate()));
            stringBuilder.AppendLine(CheckAndMark(allowCreateOnCraftingPort, "allowCreateOnCraftingPort".Translate()));
            stringBuilder.AppendLine(CheckAndMark(notAllowParentUseTools, "notAllowParentUseTools".Translate()));
            stringBuilder.AppendLine(CheckAndMark(notAllowParentUseVerbProperties, "notAllowParentUseVerbProperties".Translate()));
            //stringBuilder.AppendLine("<color=" + (notAllowParentUseTools ? "green" : "red") + ">" + "notAllowParentUseTools".Translate() + " : " + (notAllowParentUseTools ? "Yes".Translate() : "No".Translate()) + "</color>");
            //stringBuilder.AppendLine("<color=" + (notAllowParentUseVerbProperties ? "green" : "red") + ">" + "notAllowParentUseVerbProperties".Translate() + " : " + (notAllowParentUseVerbProperties ? "Yes".Translate() : "No".Translate()) + "</color>");
            //stringBuilder.AppendLine("useOriginalCraftMethod".Translate() + " : <color=" + (useOriginalCraftMethod ? "green" : "red") + ">" + (useOriginalCraftMethod ? "Yes".Translate() : "No".Translate()) + "</color>");
            //stringBuilder.AppendLine("<color=" + (verbPropertiesAffectByOtherPart ? "green" : "red") + ">" + "verbPropertiesAffectByOtherPart".Translate() + " : " + (verbPropertiesAffectByOtherPart ? "Yes".Translate() : "No".Translate()) + "</color>");
            //stringBuilder.AppendLine("<color=" + (toolsAffectByOtherPart ? "green" : "red") + ">" + "toolsAffectByOtherPart".Translate() + " : " + (toolsAffectByOtherPart ? "Yes".Translate() : "No".Translate()) + "</color>");
            //stringBuilder.AppendLine("<color=" + (verbPropertiesAffectByChildPart ? "green" : "red") + ">" + "verbPropertiesAffectByChildPart".Translate() + " : " + (verbPropertiesAffectByChildPart ? "Yes".Translate() : "No".Translate()) + "</color>");
            //stringBuilder.AppendLine("<color=" + (toolsAffectByChildPart ? "green" : "red") + ">" + "toolsAffectByChildPart".Translate() + " : " + (toolsAffectByChildPart ? "Yes".Translate() : "No".Translate()) + "</color>");
            yield return new StatDrawEntry(
                category: StatCategoryDefOf.Weapon,
                "Condation".Translate(),
                "",
                stringBuilder.ToString(),
                1000
                );
            //UnityEngine.GUIUtility.systemCopyBuffer = stringBuilder.ToString();
            #region child
            foreach (WeaponAttachmentProperties properties in attachmentProperties)
            {

                Thing child = comp?.ChildNodes[properties.id];
                CompModularizationWeapon childComp = child;
                stringBuilder.Clear();
                int Offseter = 0;
                int Multiplier = 0;
                if (childComp != null)
                {
                    Offseter = childComp.Props.statOffset.Count;
                    Multiplier = childComp.Props.statMultiplier.Count;

                    stringBuilder.AppendLine("Offseter".Translate() + " :");
                    stringBuilder.AppendLine("  " + "verbPropertiesOffseter".Translate() + " :");
                    Offseter += listAll(childComp.Props.verbPropertiesOffseter * properties.verbPropertiesOffseterAffectHorizon, false, true);

                    stringBuilder.AppendLine("  " + "toolsOffseter".Translate() + " :");
                    Offseter += listAll(childComp.Props.toolsOffseter * properties.toolsOffseterAffectHorizon, false, true);

                    stringBuilder.AppendLine("  " + "statOffseter".Translate() + " :");
                    foreach (StatModifier stat in childComp.Props.statOffset)
                    {
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : +{properties.statOffsetAffectHorizon.GetStatValueFromList(stat.stat, properties.statOffsetAffectHorizonDefaultValue) * childComp.GetStatOffset(stat.stat, req.Thing)}");
                    }


                    stringBuilder.AppendLine("Multiplier".Translate() + " :");
                    stringBuilder.AppendLine("  " + "verbPropertiesMultiplier".Translate() + " :");
                    Multiplier += listAll((childComp.Props.verbPropertiesMultiplier - 1) * properties.verbPropertiesMultiplierAffectHorizon + 1, true, true);

                    stringBuilder.AppendLine("  " + "toolsMultiplier".Translate() + " :");
                    Multiplier += listAll((childComp.Props.toolsMultiplier - 1) * properties.toolsMultiplierAffectHorizon + 1, true, true);

                    stringBuilder.AppendLine("  " + "statMultiplier".Translate() + " :");
                    foreach (StatModifier stat in childComp.Props.statMultiplier)
                    {
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : x{properties.statMultiplierAffectHorizon.GetStatValueFromList(stat.stat, properties.statMultiplierAffectHorizonDefaultValue) * (childComp.GetStatMultiplier(stat.stat, req.Thing) - 1f) + 1f}");
                    }
                }
                else
                {
                    Offseter = properties.statOffsetAffectHorizon.Count;
                    Multiplier = properties.statMultiplierAffectHorizon.Count;

                    stringBuilder.AppendLine("OffseterAffectHorizon".Translate() + " :");
                    stringBuilder.AppendLine("  " + "verbPropertiesOffseterAffectHorizon".Translate() + " :");
                    Offseter += listAll(properties.verbPropertiesOffseterAffectHorizon, false, true);

                    stringBuilder.AppendLine("  " + "toolsOffseterAffectHorizon".Translate() + " :");
                    Offseter += listAll(properties.toolsOffseterAffectHorizon, false, true);

                    stringBuilder.AppendLine("  " + "statOffsetAffectHorizon".Translate() + " :");
                    foreach (StatModifier stat in properties.statOffsetAffectHorizon)
                    {
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : x{stat.value}");
                    }

                    stringBuilder.AppendLine("MultiplierAffectHorizon".Translate() + " :");
                    stringBuilder.AppendLine("  " + "verbPropertiesMultiplierAffectHorizon".Translate() + " :");
                    Multiplier += listAll(properties.verbPropertiesMultiplierAffectHorizon, true, true);

                    stringBuilder.AppendLine("  " + "toolsMultiplierAffectHorizon".Translate() + " :");
                    Multiplier += listAll(properties.toolsMultiplierAffectHorizon, true, true);

                    stringBuilder.AppendLine("  " + "statMultiplierAffectHorizon".Translate() + " :");
                    foreach (StatModifier stat in properties.statMultiplierAffectHorizon)
                    {
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : (k-1)x{stat.value}+1");
                    }
                }

                List<Dialog_InfoCard.Hyperlink> hyperlinks = new List<Dialog_InfoCard.Hyperlink>(properties.filter.AllowedDefCount);
                hyperlinks.AddRange(from x in properties.filter.AllowedThingDefs select new Dialog_InfoCard.Hyperlink(x));
                if (child != null) hyperlinks.Insert(0, new Dialog_InfoCard.Hyperlink(child));
                yield return new StatDrawEntry(
                    StatCategoryDefOf.Weapon,
                    "AttachmentPoint".Translate() + " : " + properties.Name,
                    childComp?.parent.Label ?? (Offseter
                    + " " + "Offseter".Translate() + "; " +
                    Multiplier
                    + " " + "Multiplier".Translate() + ";"),
                    stringBuilder.ToString(),
                    900,null,
                    hyperlinks
                    );
            }
            #endregion
        }

        public Vector2 DrawSizeWhenAttach = Vector2.one;


        #region Condation
        public bool unchangeable = false;


        public bool notDrawInParent = false;


        public bool allowCreateOnCraftingPort = false;


        public bool setRandomPartWhenCreate = false;


        public bool notAllowParentUseVerbProperties = false;


        public bool notAllowParentUseTools = false;
        #endregion


        #region Offset
        public FieldReaderDgitList<VerbProperties> verbPropertiesOffseter = new FieldReaderDgitList<VerbProperties>();


        public FieldReaderDgitList<Tool> toolsOffseter = new FieldReaderDgitList<Tool>();


        public List<StatModifier> statOffset = new List<StatModifier>();


        public FieldReaderDgitList<CompProperties> compPropertiesOffseter = new FieldReaderDgitList<CompProperties>();


        //public FieldReaderDgitList<CompProperties>> ThingCompOffseter = new FieldReaderDgitList<CompProperties>>();

        public float verbPropertiesOtherPartOffseterAffectHorizon = 1;

        public float toolsOtherPartOffseterAffectHorizon = 1;

        public float statOtherPartOffseterAffectHorizon = 1;
        #endregion


        #region Multiplier
        public FieldReaderDgitList<VerbProperties> verbPropertiesMultiplier = new FieldReaderDgitList<VerbProperties>();


        public FieldReaderDgitList<Tool> toolsMultiplier = new FieldReaderDgitList<Tool>();


        public List<StatModifier> statMultiplier = new List<StatModifier>();


        public FieldReaderDgitList<CompProperties> compPropertiesMultiplier = new FieldReaderDgitList<CompProperties>();


        //public FieldReaderDgitList<CompProperties>> ThingCompMultiplier = new FieldReaderDgitList<CompProperties>>();

        public float verbPropertiesOtherPartMultiplierAffectHorizon = 1;

        public float toolsOtherPartMultiplierAffectHorizon = 1;

        public float statOtherPartMultiplierAffectHorizon = 1;
        #endregion


        #region Patchs
        public List<FieldReaderInst<VerbProperties>> verbPropertiesObjectPatch = new List<FieldReaderInst<VerbProperties>>();


        public List<FieldReaderInst<Tool>> toolsObjectPatch = new List<FieldReaderInst<Tool>>();


        public List<FieldReaderInst<CompProperties>> compPropertiesObjectPatch = new List<FieldReaderInst<CompProperties>>();


        public bool verbPropertiesObjectPatchByChildPart = true;


        public bool toolsObjectPatchByChildPart = true;


        public bool verbPropertiesObjectPatchByOtherPart = false;


        public bool toolsObjectPatchByOtherPart = false;




        public FieldReaderBoolList<VerbProperties> verbPropertiesBoolAndPatch = new FieldReaderBoolList<VerbProperties>();


        public FieldReaderBoolList<Tool> toolsBoolAndPatch = new FieldReaderBoolList<Tool>();


        public FieldReaderBoolList<CompProperties> compPropertiesBoolAndPatch = new FieldReaderBoolList<CompProperties>();


        public bool verbPropertiesBoolAndPatchByChildPart = true;


        public bool toolsBoolAndPatchByChildPart = true;


        public bool verbPropertiesBoolAndPatchByOtherPart = false;


        public bool toolsBoolAndPatchByOtherPart = false;




        public FieldReaderBoolList<VerbProperties> verbPropertiesBoolOrPatch = new FieldReaderBoolList<VerbProperties>();


        public FieldReaderBoolList<Tool> toolsBoolOrPatch = new FieldReaderBoolList<Tool>();


        public FieldReaderBoolList<CompProperties> compPropertiesBoolOrPatch = new FieldReaderBoolList<CompProperties>();


        public bool verbPropertiesBoolOrPatchByChildPart = true;


        public bool toolsBoolOrPatchByChildPart = true;


        public bool verbPropertiesBoolOrPatchByOtherPart = false;


        public bool toolsBoolOrPatchByOtherPart = false;
        #endregion


        public List<WeaponAttachmentProperties> attachmentProperties = new List<WeaponAttachmentProperties>();


        public string PartTexPath = null;


        private Material materialCache;
    }
}
