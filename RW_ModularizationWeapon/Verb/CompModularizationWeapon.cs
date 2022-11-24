using RW_ModularizationWeapon.Tools;
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


        protected override List<VerbPropertiesRegiestInfo> PostIVerbOwner_GetVerbProperties(Type ownerType, List<VerbPropertiesRegiestInfo> result, Dictionary<string, object> forPostRead)
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

            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing child = container[i];
                WeaponAttachmentProperties attachmentProperties = Props.WeaponAttachmentPropertiesById(id);
                if (!internal_NotUseVerbProperties(child, attachmentProperties))
                {
                    IVerbOwner verbOwner = CompChildNodeProccesser.GetSameTypeVerbOwner(ownerType, child);
                    List<VerbProperties> verbProperties = verbOwner?.VerbProperties ?? child?.def.Verbs;
                    if (verbProperties != null)
                    {
                        result.Capacity += verbProperties.Count;
                        CompModularizationWeapon comp = child;
                        if (comp != null && verbOwner == null)
                        {
                            for (int j = 0; j < verbProperties.Count; j++)
                            {
                                VerbProperties cache = verbProperties[j];
                                VerbProperties newProp
                                    = VerbPropertiesAfterAffect(
                                        comp.VerbPropertiesAfterAffect(
                                            cache,
                                            null
                                            ),
                                        id
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
                                        id
                                        );
                                result.Add(new VerbPropertiesRegiestInfo(id, cache, newProp));
                            }
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
            return result;
        }


        protected override List<VerbToolRegiestInfo> PostIVerbOwner_GetTools(Type ownerType, List<VerbToolRegiestInfo> result, Dictionary<string, object> forPostRead)
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

            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                Thing child = container[i];
                WeaponAttachmentProperties attachmentProperties = Props.WeaponAttachmentPropertiesById(id);
                if (!internal_NotUseTools(child, attachmentProperties))
                {
                    IVerbOwner verbOwner = CompChildNodeProccesser.GetSameTypeVerbOwner(ownerType, child);
                    List<Tool> tools = verbOwner?.Tools ?? child?.def.tools;
                    if (tools != null)
                    {
                        result.Capacity += tools.Count;
                        CompModularizationWeapon comp = child;
                        if (comp != null && verbOwner == null)
                        {
                            for (int j = 0; j < tools.Count; j++)
                            {
                                Tool cache = tools[j];
                                Tool newProp
                                    = ToolAfterAffect(
                                        comp.ToolAfterAffect(
                                            cache,
                                            null),
                                        id
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
                                        id
                                        );
                                result.Add(new VerbToolRegiestInfo(id, cache, newProp));
                            }
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
            return result;
        }
    }
}
