﻿using RW_NodeTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
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
                        if (comp != null && verbOwner == null)
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
    }
}