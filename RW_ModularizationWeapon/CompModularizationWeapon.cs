﻿using System;
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
using RW_NodeTree;
using RW_NodeTree.Rendering;
using RW_NodeTree.Tools;
using UnityEngine;
using Verse;
using Verse.AI;

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
        internal bool internal_Unchangeable(Thing thing, WeaponAttachmentProperties properties)
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
        internal bool internal_NotDraw(Thing thing, WeaponAttachmentProperties properties)
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
        internal bool internal_NotUseTools(Thing thing, WeaponAttachmentProperties properties)
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
        internal bool internal_NotUseVerbProperties(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                CompModularizationWeapon comp = thing;
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
                CompModularizationWeapon comp = thing;
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
        public FieldReaderDgit<VerbProperties> VerbPropertiesOffseter(string childNodeIdForVerbProperties)
        {
            FieldReaderDgit<VerbProperties> result = new FieldReaderDgit<VerbProperties>();
            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing thing = container[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (thing != null && id != childNodeIdForVerbProperties)
                {
                    CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                    if (comp != null && comp.Validity)
                    {
                        result += comp.Props.verbPropertiesOffseter * properties.verbPropertiesOffseterAffectHorizon;
                    }
                }
            }
            return result;
        }


        public FieldReaderDgit<Tool> ToolsOffseter(string childNodeIdForTool)
        {
            FieldReaderDgit<Tool> result = new FieldReaderDgit<Tool>();
            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing thing = container[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (thing != null && id != childNodeIdForTool)
                {
                    CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                    if (comp != null && comp.Validity)
                    {
                        result += comp.Props.toolsOffseter * properties.toolsOffseterAffectHorizon;
                    }
                }
            }
            return result;
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
        public FieldReaderDgit<VerbProperties> VerbPropertiesMultiplier(string childNodeIdForVerbProperties)
        {
            FieldReaderDgit<VerbProperties> result = new FieldReaderDgit<VerbProperties>();
            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing thing = container[i];
                if (thing != null && id != childNodeIdForVerbProperties)
                {
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                    if (comp != null && comp.Validity)
                    {
                        result *= (comp.Props.verbPropertiesMultiplier - 1f) * properties.verbPropertiesMultiplierAffectHorizon + 1f;
                    }
                }
            }
            return result;
        }


        public FieldReaderDgit<Tool> ToolsMultiplier(string childNodeIdForTool)
        {
            FieldReaderDgit<Tool> result = new FieldReaderDgit<Tool>();
            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing thing = container[i];
                if (thing != null && id != childNodeIdForTool)
                {
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                    CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                    if (comp != null && comp.Validity)
                    {
                        result *= (comp.Props.toolsMultiplier - 1f) * properties.toolsMultiplierAffectHorizon + 1f;
                    }
                }
            }
            return result;
        }

        public float GetStatMultiplier(StatDef statDef)
        {
            float result = Props.statMultiplier.GetStatFactorFromList(statDef);
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
                        result *= 1f + (comp.GetStatMultiplier(statDef) - 1f) * properties.statMultiplierAffectHorizon.GetStatFactorFromList(statDef);
                    }
                }
            }
            return result;
        }
        #endregion


        #region Patch
        public FieldReaderInst<VerbProperties> VerbPropertiesObjectPatch(string childNodeIdForVerbProperties)
        {
            NodeContainer container = ChildNodes;
            FieldReaderInst<VerbProperties> result = new FieldReaderInst<VerbProperties>();
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing thing = container[i];
                if (thing != null && id != childNodeIdForVerbProperties)
                {
                    CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                    result |= comp.Props.verbPropertiesObjectPatch;
                }
            }
            return null;
        }
        

        public FieldReaderInst<Tool> ToolsObjectPatch(string childNodeIdForTool)
        {
            NodeContainer container = ChildNodes;
            FieldReaderInst<Tool> result = new FieldReaderInst<Tool>();
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing thing = container[i];
                if (thing != null && id != childNodeIdForTool)
                {
                    CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                    result |= comp.Props.toolsObjectPatch;
                }
            }
            return null;
        }


        public FieldReaderBool<VerbProperties> VerbPropertiesBoolAndPatch(string childNodeIdForVerbProperties)
        {
            NodeContainer container = ChildNodes;
            FieldReaderBool<VerbProperties> result = new FieldReaderBool<VerbProperties>();
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing thing = container[i];
                if (thing != null && id != childNodeIdForVerbProperties)
                {
                    CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                    result &= comp.Props.verbPropertiesBoolAndPatch;
                }
            }
            return null;
        }


        public FieldReaderBool<Tool> ToolsBoolAndPatch(string childNodeIdForTool)
        {
            NodeContainer container = ChildNodes;
            FieldReaderBool<Tool> result = new FieldReaderBool<Tool>();
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing thing = container[i];
                if (thing != null && id != childNodeIdForTool)
                {
                    CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                    result &= comp.Props.toolsBoolAndPatch;
                }
            }
            return null;
        }


        public FieldReaderBool<VerbProperties> VerbPropertiesBoolOrPatch(string childNodeIdForVerbProperties)
        {
            NodeContainer container = ChildNodes;
            FieldReaderBool<VerbProperties> result = new FieldReaderBool<VerbProperties>();
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing thing = container[i];
                if (thing != null && id != childNodeIdForVerbProperties)
                {
                    CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                    result |= comp.Props.verbPropertiesBoolOrPatch;
                }
            }
            return null;
        }


        public FieldReaderBool<Tool> ToolsBoolOrPatch(string childNodeIdForTool)
        {
            NodeContainer container = ChildNodes;
            FieldReaderBool<Tool> result = new FieldReaderBool<Tool>();
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing thing = container[i];
                if (thing != null && id != childNodeIdForTool)
                {
                    CompModularizationWeapon comp = thing.TryGetComp<CompModularizationWeapon>();
                    result |= comp.Props.toolsBoolOrPatch;
                }
            }
            return null;
        }
        #endregion


        #region Verb
        internal VerbProperties VerbPropertiesAfterAffect(VerbProperties properties, string childNodeIdForVerbProperties, bool affectDef)
        {
            //properties = (VerbProperties)properties.SimpleCopy();
            properties *= VerbPropertiesMultiplier(childNodeIdForVerbProperties);
            properties += VerbPropertiesOffseter(childNodeIdForVerbProperties);
            if (affectDef)
            {
                FieldReaderInst<VerbProperties> patchInst = VerbPropertiesObjectPatch(childNodeIdForVerbProperties);
                properties &= patchInst;
                properties |= patchInst;
                properties &= VerbPropertiesBoolAndPatch(childNodeIdForVerbProperties);
                properties |= VerbPropertiesBoolOrPatch(childNodeIdForVerbProperties);
            }
            return properties;
        }


        internal Tool ToolAfterAffect(Tool tool, string childNodeIdForTool, bool affectDef)
        {
            //tool = (Tool)tool.SimpleCopy();
            tool *= ToolsMultiplier(childNodeIdForTool);
            tool += ToolsOffseter(childNodeIdForTool);
            if (affectDef)
            {
                FieldReaderInst<Tool> patchInst = ToolsObjectPatch(childNodeIdForTool);
                tool &= patchInst;
                tool |= patchInst;
                tool &= ToolsBoolAndPatch(childNodeIdForTool);
                tool |= ToolsBoolOrPatch(childNodeIdForTool);
            }
            return tool;
        }


        protected override List<VerbToolRegiestInfo> PostIVerbOwner_GetTools(Type ownerType, List<VerbToolRegiestInfo> result, Dictionary<string, object> forPostRead)
        {
            if (Props.verbPropertiesAffectByChildPart)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    VerbToolRegiestInfo prop = result[i];
                    Tool newProp = ToolAfterAffect(prop.berforConvertTool, null, true);
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
                                Tool newProp = ToolAfterAffect(cache,id, false);
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
                    VerbProperties newProp = VerbPropertiesAfterAffect(prop.berforConvertProperties, null, true);
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
                                VerbProperties newProp = VerbPropertiesAfterAffect(cache, id, false);
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
                    ThingDef_verbs(parent.def) = ThingDef_verbs(parent.def) ?? new List<VerbProperties>();
                    forPostRead.Add("CompModularizationWeapon_verbs", new List<VerbProperties>(parent.def.Verbs));
                    forPostRead.Add("CompModularizationWeapon_tools", new List<Tool>(parent.def.tools));
                    //if (Prefs.DevMode) Log.Message(" prefix before clear: parent.def.Verbs0=" + parent.def.Verbs.Count + "; parent.def.tools0=" + parent.def.tools.Count + ";\n");
                    List<Verb> verbs = eq.AllVerbs;
                    parent.def.Verbs.Clear();
                    parent.def.tools.Clear();
                    //if (Prefs.DevMode) Log.Message(" prefix before change: parent.def.Verbs.Count=" + parent.def.Verbs.Count + "; parent.def.tools.Count=" + parent.def.tools.Count + ";\n");
                    foreach (Verb verb in verbs)
                    {
                        if (verb.tool != null) parent.def.tools.Add(verb.tool);
                        else parent.def.Verbs.Add(verb.verbProps);
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
                foreach (Thing thing in ChildNodes.Values)
                {
                    result += statWorker.GetValue(thing);
                }
            }
            else if(!(statWorker is StatWorker_MeleeAverageArmorPenetration || statWorker is StatWorker_MeleeAverageDPS))
            {
                StatDef statDef = StatWorker_stat(statWorker);
                result *= GetStatMultiplier(statDef) / Props.statMultiplier.GetStatFactorFromList(statDef);
                result += GetStatOffset(statDef) - Props.statOffset.GetStatOffsetFromList(statDef);
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
                foreach (Thing thing in ChildNodes.Values)
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
                foreach (Thing thing in ChildNodes.Values)
                {
                    yield return new Dialog_InfoCard.Hyperlink(thing);
                }
            }
        }


        protected override IEnumerable<StatDrawEntry> PostThingDef_SpecialDisplayStats(ThingDef def, StatRequest req, IEnumerable<StatDrawEntry> result)
        {
            //Log.Message($"PostThingDef_SpecialDisplayStats({def},{req},{result})");
            List<VerbProperties> verbProperties = null;
            List<Tool> tools = null;
            CompEquippable eq = parent.GetComp<CompEquippable>();
            if (eq != null)
            {
                ThingDef_verbs(parent.def) = ThingDef_verbs(parent.def) ?? new List<VerbProperties>();
                verbProperties = new List<VerbProperties>(parent.def.Verbs);
                tools = new List<Tool>(parent.def.tools);
                List<Verb> verbs = eq.AllVerbs;
                parent.def.Verbs.Clear();
                parent.def.tools.Clear();
                foreach (Verb verb in verbs)
                {
                    if (verb.tool != null) parent.def.tools.Add(verb.tool);
                    else parent.def.Verbs.Add(verb.verbProps);
                }
            }
            foreach (StatDrawEntry entry in result)
            {
                yield return entry;
            }
            if(verbProperties != null && tools != null)
            {
                parent.def.Verbs.Clear();
                parent.def.tools.Clear();
                parent.def.Verbs.AddRange(verbProperties);
                parent.def.tools.AddRange(tools);
            }
        }


        protected override IEnumerable<StatDrawEntry> PostStatsReportUtility_StatsToDraw(Thing thing, IEnumerable<StatDrawEntry> result)
        {
            //Log.Message($"PostStatsReportUtility_StatsToDraw({thing},{result})");
            List<VerbProperties> verbProperties = null;
            List<Tool> tools = null;
            CompEquippable eq = parent.GetComp<CompEquippable>();
            if (eq != null)
            {
                ThingDef_verbs(parent.def) = ThingDef_verbs(parent.def) ?? new List<VerbProperties>();
                verbProperties = new List<VerbProperties>(parent.def.Verbs);
                tools = new List<Tool>(parent.def.tools);
                List<Verb> verbs = eq.AllVerbs;
                parent.def.Verbs.Clear();
                parent.def.tools.Clear();
                foreach (Verb verb in verbs)
                {
                    if (verb.tool != null) parent.def.tools.Add(verb.tool);
                    else parent.def.Verbs.Add(verb.verbProps);
                }
            }
            foreach (StatDrawEntry entry in result)
            {
                yield return entry;
            }
            if (verbProperties != null && tools != null)
            {
                parent.def.Verbs.Clear();
                parent.def.tools.Clear();
                parent.def.Verbs.AddRange(verbProperties);
                parent.def.tools.AddRange(tools);
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
            foreach ((string id, Thing thing, WeaponAttachmentProperties properties) in this)
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
                                //Vector4 cache = matrix[k].GetRow(0);
                                //matrix[k].SetRow(0, new Vector4(new Vector3(cache.x, cache.y, cache.z).magnitude, 0, 0, cache.w));

                                //cache = matrix[k].GetRow(1);
                                //matrix[k].SetRow(1, new Vector4(0, new Vector3(cache.x, cache.y, cache.z).magnitude, 0, cache.w));

                                //cache = matrix[k].GetRow(2);
                                //matrix[k].SetRow(2, new Vector4(0, 0, new Vector3(cache.x, cache.y, cache.z).magnitude, cache.w));

                                //matrix[k] = properties.Transfrom * matrix[k];
                                matrix[k] = properties.Transfrom;
                            }
                            cacheInfo[j] = info;
                        }
                    }
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


        protected override void PostPreApplyDamageWithRef(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            int count = ChildNodes.Count + 1;
            dinfo.SetAmount(dinfo.Amount / count);
            foreach (Thing thing in ChildNodes.Values)
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


        protected override CompChildNodeProccesser OverrideParentProccesser(CompChildNodeProccesser orginal) => UsingTargetPart ? (targetModeParent ?? orginal) : orginal;


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
                properties.filter.ResolveReferences();
            }
            if (attachmentProperties.Count > 0) parentDef.stackLimit = 1;
        }


        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {

            CompModularizationWeapon comp = req.Thing;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("verbPropertiesOffseter".Translate() + " :");
            foreach ((FieldInfo field, double value) in verbPropertiesOffseter)
            {
                stringBuilder.AppendLine($"  {field.Name.Translate()} : +{value}");
            }
            stringBuilder.AppendLine("toolsOffseter".Translate() + " :");
            foreach ((FieldInfo field, double value) in toolsOffseter)
            {
                stringBuilder.AppendLine($"  {field.Name.Translate()} : +{value}");
            }
            stringBuilder.AppendLine("statOffset".Translate() + " :");
            foreach (StatModifier stat in statOffset)
            {
                stringBuilder.AppendLine($"  {stat.stat.LabelCap} : +{comp?.GetStatOffset(stat.stat) ?? stat.value}");
            }
            int count = verbPropertiesOffseter.Count + toolsOffseter.Count + statOffset.Count;
            yield return new StatDrawEntry(
                StatCategoryDefOf.Weapon,
                "Offset".Translate(),
                count + " " + "Offseter".Translate(),
                stringBuilder.ToString(),
                1000
                );

            stringBuilder.Clear();
            stringBuilder.AppendLine("verbPropertiesMultiplier".Translate() + " :");
            foreach ((FieldInfo field, double value) in verbPropertiesMultiplier)
            {
                stringBuilder.AppendLine($"  {field.Name.Translate()} : x{value}");
            }
            stringBuilder.AppendLine("toolsMultiplier".Translate() + " :");
            foreach ((FieldInfo field, double value) in toolsMultiplier)
            {
                stringBuilder.AppendLine($"  {field.Name.Translate()} : x{value}");
            }
            stringBuilder.AppendLine("statMultiplier".Translate() + " :");
            foreach (StatModifier stat in statMultiplier)
            {
                stringBuilder.AppendLine($"  {stat.stat.LabelCap} : x{comp?.GetStatMultiplier(stat.stat) ?? stat.value}");
            }
            count = verbPropertiesMultiplier.Count + toolsMultiplier.Count + statMultiplier.Count;
            yield return new StatDrawEntry(
                category: StatCategoryDefOf.Weapon, 
                "Multiplier".Translate(),
                count + " " + "Multiplier".Translate(), 
                stringBuilder.ToString(), 
                950
                );

            foreach (WeaponAttachmentProperties properties in attachmentProperties)
            {

                CompModularizationWeapon childComp = comp?.ChildNodes[properties.id];
                stringBuilder.Clear();
                if(childComp != null)
                {
                    stringBuilder.AppendLine("Offseter".Translate() + " :");
                    stringBuilder.AppendLine("  " + "verbPropertiesOffseter".Translate() + " :");
                    foreach ((FieldInfo field, double value) in properties.verbPropertiesOffseterAffectHorizon * childComp.Props.verbPropertiesOffseter)
                    {
                        stringBuilder.AppendLine($"    {field.Name.Translate()} : +{value}");
                    }
                    stringBuilder.AppendLine("  " + "toolsOffseter".Translate() + " :");
                    foreach ((FieldInfo field, double value) in properties.toolsOffseterAffectHorizon * childComp.Props.toolsOffseter)
                    {
                        stringBuilder.AppendLine($"    {field.Name.Translate()} : +{value}");
                    }
                    stringBuilder.AppendLine("  " + "statOffseter".Translate() + " :");
                    foreach (StatModifier stat in properties.statOffsetAffectHorizon)
                    {
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : +{stat.value * childComp.GetStatOffset(stat.stat)}");
                    }

                    stringBuilder.AppendLine("Multiplier".Translate() + " :");
                    stringBuilder.AppendLine("  " + "verbPropertiesMultiplier".Translate() + " :");
                    foreach ((FieldInfo field, double value) in (properties.verbPropertiesMultiplierAffectHorizon - 1) * childComp.Props.verbPropertiesMultiplier + 1)
                    {
                        stringBuilder.AppendLine($"    {field.Name.Translate()} : x{value}");
                    }
                    stringBuilder.AppendLine("  " + "toolsMultiplier".Translate() + " :");
                    foreach ((FieldInfo field, double value) in (properties.toolsMultiplierAffectHorizon - 1) * childComp.Props.toolsMultiplier + 1)
                    {
                        stringBuilder.AppendLine($"    {field.Name.Translate()} : x{value}");
                    }
                    stringBuilder.AppendLine("  " + "statMultiplier".Translate() + " :");
                    foreach (StatModifier stat in properties.statMultiplierAffectHorizon)
                    {
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : x{stat.value * (childComp.GetStatOffset(stat.stat) - 1) + 1}");
                    }
                }
                else
                {
                    stringBuilder.AppendLine("OffseterAffectHorizon".Translate() + " :");
                    stringBuilder.AppendLine("  " + "verbPropertiesOffseterAffectHorizon".Translate() + " :");
                    foreach ((FieldInfo field, double value) in properties.verbPropertiesOffseterAffectHorizon)
                    {
                        stringBuilder.AppendLine($"    {field.Name.Translate()} : x{value}");
                    }
                    stringBuilder.AppendLine("  " + "toolsOffseterAffectHorizon".Translate() + " :");
                    foreach ((FieldInfo field, double value) in properties.toolsOffseterAffectHorizon)
                    {
                        stringBuilder.AppendLine($"    {field.Name.Translate()} : x{value}");
                    }
                    stringBuilder.AppendLine("  " + "statOffsetAffectHorizon".Translate() + " :");
                    foreach (StatModifier stat in properties.statOffsetAffectHorizon)
                    {
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : x{stat.value}");
                    }

                    stringBuilder.AppendLine("MultiplierAffectHorizon".Translate() + " :");
                    stringBuilder.AppendLine("  " + "verbPropertiesMultiplierAffectHorizon".Translate() + " :");
                    foreach ((FieldInfo field, double value) in properties.verbPropertiesMultiplierAffectHorizon)
                    {
                        stringBuilder.AppendLine($"    {field.Name.Translate()} : (k-1)x{value}+1");
                    }
                    stringBuilder.AppendLine("  " + "toolsMultiplierAffectHorizon".Translate() + " :");
                    foreach ((FieldInfo field, double value) in properties.toolsMultiplierAffectHorizon)
                    {
                        stringBuilder.AppendLine($"    {field.Name.Translate()} : (k-1)x{value}+1");
                    }
                    stringBuilder.AppendLine("  " + "statMultiplierAffectHorizon".Translate() + " :");
                    foreach (StatModifier stat in properties.statMultiplierAffectHorizon)
                    {
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : (k-1)x{stat.value}+1");
                    }
                }

                List<Dialog_InfoCard.Hyperlink> hyperlinks = new List<Dialog_InfoCard.Hyperlink>(properties.filter.AllowedDefCount);
                hyperlinks.AddRange(from x in properties.filter.AllowedThingDefs select new Dialog_InfoCard.Hyperlink(x));
                if (childComp != null) hyperlinks.Insert(0, new Dialog_InfoCard.Hyperlink(childComp));
                yield return new StatDrawEntry(
                    StatCategoryDefOf.Weapon,
                    "AttachmentPoint".Translate() + " : " + properties.Name,
                    childComp?.parent.Label ?? ((properties.verbPropertiesOffseterAffectHorizon.Count + properties.toolsOffseterAffectHorizon.Count + properties.statOffsetAffectHorizon.Count)
                    + " " + "Offseter".Translate() + "; " +
                    (properties.verbPropertiesMultiplierAffectHorizon.Count + properties.toolsMultiplierAffectHorizon.Count + properties.statMultiplierAffectHorizon.Count)
                    + " " + "Multiplier".Translate() + ";"),
                    stringBuilder.ToString(),
                    900,null,
                    hyperlinks
                    );
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
        public FieldReaderDgit<VerbProperties> verbPropertiesOffseter = new FieldReaderDgit<VerbProperties>();


        public FieldReaderDgit<Tool> toolsOffseter = new FieldReaderDgit<Tool>();


        public List<StatModifier> statOffset = new List<StatModifier>();
        #endregion


        #region Multiplier
        public FieldReaderDgit<VerbProperties> verbPropertiesMultiplier = new FieldReaderDgit<VerbProperties>();


        public FieldReaderDgit<Tool> toolsMultiplier = new FieldReaderDgit<Tool>();


        public List<StatModifier> statMultiplier = new List<StatModifier>();
        #endregion


        #region Patchs
        public FieldReaderInst<VerbProperties> verbPropertiesObjectPatch = new FieldReaderInst<VerbProperties>();


        public FieldReaderInst<Tool> toolsObjectPatch = new FieldReaderInst<Tool>();


        public FieldReaderBool<VerbProperties> verbPropertiesBoolAndPatch = new FieldReaderBool<VerbProperties>();


        public FieldReaderBool<Tool> toolsBoolAndPatch = new FieldReaderBool<Tool>();


        public FieldReaderBool<VerbProperties> verbPropertiesBoolOrPatch = new FieldReaderBool<VerbProperties>();


        public FieldReaderBool<Tool> toolsBoolOrPatch = new FieldReaderBool<Tool>();
        #endregion


        public List<WeaponAttachmentProperties> attachmentProperties = new List<WeaponAttachmentProperties>();


        public string PartTexPath = null;


        private Material materialCache;
    }
}
