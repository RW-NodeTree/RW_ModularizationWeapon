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


        protected override List<VerbPropertiesRegiestInfo> FinalIVerbOwner_GetVerbProperties(Type ownerType, List<VerbPropertiesRegiestInfo> result, Dictionary<string, object> stats, Exception exception)
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
                    CompModularizationWeapon comp = child;
                    IVerbOwner verbOwner = CompChildNodeProccesser.GetSameTypeVerbOwner(ownerType, child);
                    List<VerbProperties> verbProperties = verbOwner?.VerbProperties ?? child?.def.Verbs;
                    if(verbProperties == null && comp != null && child.def.Verbs != null)
                    {
                        Dictionary<string, object> cache = new Dictionary<string, object>();
                        comp.NodeProccesser.PreIVerbOwner_GetVerbProperties(ownerType, cache);
                        verbProperties = comp.NodeProccesser.FinalIVerbOwner_GetVerbProperties(ownerType, child.def.Verbs, cache, exception);
                    }
                    verbProperties = verbProperties ?? child?.def.Verbs;
                    if (verbProperties != null)
                    {
                        result.Capacity += verbProperties.Count;
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


        protected override List<VerbToolRegiestInfo> FinalIVerbOwner_GetTools(Type ownerType, List<VerbToolRegiestInfo> result, Dictionary<string, object> stats, Exception exception)
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
                    CompModularizationWeapon comp = child;
                    IVerbOwner verbOwner = CompChildNodeProccesser.GetSameTypeVerbOwner(ownerType, child);
                    List<Tool> tools = verbOwner?.Tools;
                    if (tools == null && comp != null && child.def.tools != null)
                    {
                        Dictionary<string, object> cache = new Dictionary<string, object>();
                        comp.NodeProccesser.PreIVerbOwner_GetTools(ownerType, cache);
                        tools = comp.NodeProccesser.FinalIVerbOwner_GetTools(ownerType, child.def.tools, cache, exception);
                    }
                    tools = tools ?? child?.def.tools;
                    if (tools != null)
                    {
                        result.Capacity += tools.Count;
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
