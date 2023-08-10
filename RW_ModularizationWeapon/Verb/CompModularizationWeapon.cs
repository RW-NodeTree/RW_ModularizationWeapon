using RW_NodeTree;
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
        internal VerbProperties VerbPropertiesAfterAffect(VerbProperties properties, string childNodeIdForVerbProperties)
        {
            //properties = (VerbProperties)properties.SimpleCopy();
            properties *= VerbPropertiesMultiplier(childNodeIdForVerbProperties);
            properties += VerbPropertiesOffseter(childNodeIdForVerbProperties);
            properties &= VerbPropertiesBoolAndPatch(childNodeIdForVerbProperties);
            properties |= VerbPropertiesBoolOrPatch(childNodeIdForVerbProperties);
            VerbPropertiesObjectPatch(childNodeIdForVerbProperties)
            .ForEach(x =>
            {
                //Log.Message(x.ToString());
                properties &= x;
                properties |= x;
            });
            return properties;
        }


        internal Tool ToolAfterAffect(Tool tool, string childNodeIdForTool)
        {
            //tool = (Tool)tool.SimpleCopy();
            tool *= ToolsMultiplier(childNodeIdForTool);
            tool += ToolsOffseter(childNodeIdForTool);
            tool &= ToolsBoolAndPatch(childNodeIdForTool);
            tool |= ToolsBoolOrPatch(childNodeIdForTool);
            ToolsObjectPatch(childNodeIdForTool)
            .ForEach(x =>
            {
                //Log.Message(x.ToString());
                tool &= x;
                tool |= x;
            });
            return tool;
        }


        protected override List<VerbPropertiesRegiestInfo> VerbPropertiesRegiestInfoUpadte(Type ownerType, List<VerbPropertiesRegiestInfo> result)
        {
            if ((this.UsingTargetPart ? regiestedNodeVerbPropertiesInfos_TargetPart : regiestedNodeVerbPropertiesInfos).TryGetValue(ownerType, out List<VerbPropertiesRegiestInfo> data)) return data;

            if (!parent.def.Verbs.NullOrEmpty() && CompChildNodeProccesser.GetSameTypeVerbOwner(ownerType, parent) == null)
            {

                result.Capacity += parent.def.Verbs.Count;
                foreach (VerbProperties properties in parent.def.Verbs)
                {
                    VerbPropertiesRegiestInfo prop = new VerbPropertiesRegiestInfo
                    (
                        null,
                        properties,
                        VerbPropertiesAfterAffect(
                            properties,
                            null
                        )
                    );
                    result.Add(prop);
                }
            }
            else
            {
                for (int i = 0; i < result.Count; i++)
                {
                    VerbPropertiesRegiestInfo prop = result[i];
                    VerbProperties newProp = VerbPropertiesAfterAffect(
                        prop.berforConvertProperties,
                        null
                        );
                    prop.afterConvertProperties = newProp;
                    result[i] = prop;
                }
            }

            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing child = container[i];
                WeaponAttachmentProperties attachmentProperties = Props.WeaponAttachmentPropertiesById(id);
                if (!internal_NotUseVerbProperties(child, attachmentProperties))
                {
                    CompModularizationWeapon comp = child;
                    List<VerbPropertiesRegiestInfo> childInfos = comp?.NodeProccesser.GetRegiestedNodeVerbPropertiesInfos(ownerType);
                    List<VerbProperties> verbProperties;
                    if (childInfos != null)
                    {
                        verbProperties = new List<VerbProperties>(childInfos.Count);
                        foreach (VerbPropertiesRegiestInfo info in childInfos) verbProperties.Add(info.afterConvertProperties);
                    }
                    else
                    {
                        verbProperties = child.def.Verbs ?? new List<VerbProperties>();
                    }
                    if (verbProperties != null)
                    {
                        result.Capacity += verbProperties.Count;
                        for (int j = 0; j < verbProperties.Count; j++)
                        {
                            VerbProperties cache = verbProperties[j];
                            VerbProperties newProp
                                = VerbPropertiesAfterAffect(
                                    comp?.VerbPropertiesAfterAffect(
                                        cache,
                                        null
                                        ) ?? cache,
                                    id
                                    );
                            result.Add(new VerbPropertiesRegiestInfo(id, cache, newProp));
                        }
                    }
                }
            }
            //StringBuilder stringBuildchildNodeIdForVerbProperties: er = new StringBuilder();
            //for (int i = 0; i < result.Count; i++)
            //{
            //    stringBuilder.AppendLine($"{i} : {result[i]}");
            //}
            //Log.Message(stringBuilder.ToString());
            (this.UsingTargetPart ? regiestedNodeVerbPropertiesInfos_TargetPart : regiestedNodeVerbPropertiesInfos).Add(ownerType, result);
            return result;
        }

        protected override List<VerbToolRegiestInfo> VerbToolRegiestInfoUpdate(Type ownerType, List<VerbToolRegiestInfo> result)
        {
            if ((this.UsingTargetPart ? regiestedNodeVerbToolInfos_TargetPart : regiestedNodeVerbToolInfos).TryGetValue(ownerType, out List<VerbToolRegiestInfo> data)) return data;


            if (!parent.def.tools.NullOrEmpty() && CompChildNodeProccesser.GetSameTypeVerbOwner(ownerType,parent) == null)
            {
                result.Capacity += parent.def.tools.Count;
                foreach(Tool tool in parent.def.tools)
                {
                    VerbToolRegiestInfo prop = new VerbToolRegiestInfo
                    (
                        null,
                        tool,
                        ToolAfterAffect(
                            tool,
                            null
                        )
                    );
                    result.Add(prop);
                }
            }
            else
            {
                for (int i = 0; i < result.Count; i++)
                {
                    VerbToolRegiestInfo prop = result[i];
                    Tool newProp = ToolAfterAffect(
                        prop.berforConvertTool,
                        null
                        );
                    prop.afterCobvertTool = newProp;
                    result[i] = prop;
                }
            }

            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing child = container[i];
                WeaponAttachmentProperties attachmentProperties = Props.WeaponAttachmentPropertiesById(id);
                if (!internal_NotUseTools(child, attachmentProperties))
                {
                    CompModularizationWeapon comp = child;
                    List<VerbToolRegiestInfo> childInfos = comp?.NodeProccesser.GetRegiestedNodeVerbToolInfos(ownerType);
                    List<Tool> tools;
                    if (childInfos != null)
                    {
                        tools = new List<Tool>(childInfos.Count);
                        foreach (VerbToolRegiestInfo info in childInfos) tools.Add(info.afterCobvertTool);
                    }
                    else
                    {
                        tools = child.def.tools ?? new List<Tool>();
                    }

                    if (tools != null)
                    {
                        result.Capacity += tools.Count;
                        for (int j = 0; j < tools.Count; j++)
                        {
                            Tool cache = tools[j];
                            Tool newProp
                                = ToolAfterAffect(
                                    comp?.ToolAfterAffect(
                                        cache,
                                        null
                                        ) ?? cache,
                                    id
                                    );
                            result.Add(new VerbToolRegiestInfo(id, cache, newProp));
                        }
                    }
                }
            }
            //StringBuilder stringBuilder = new StringBuilder();
            //for (int i = 0; i < result.Count; i++)
            //{
            //    stringBuilder.AppendLine($"{i} : {result[i]}");
            //}
            //Log.Message(stringBuilder.ToString());
            (this.UsingTargetPart ? regiestedNodeVerbToolInfos_TargetPart : regiestedNodeVerbToolInfos).Add(ownerType, result);
            return result;
        }
    }
}
