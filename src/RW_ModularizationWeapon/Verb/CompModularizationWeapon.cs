using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        internal VerbProperties VerbPropertiesAfterAffect(VerbProperties properties, string? childNodeIdForVerbProperties)
        {
            //properties = (VerbProperties)properties.SimpleCopy();
            properties = (properties * VerbPropertiesMultiplier(childNodeIdForVerbProperties)) ?? properties;
            properties = (properties + VerbPropertiesOffseter(childNodeIdForVerbProperties)) ?? properties;
            properties = (properties & VerbPropertiesBoolAndPatch(childNodeIdForVerbProperties)) ?? properties;
            properties = (properties | VerbPropertiesBoolOrPatch(childNodeIdForVerbProperties)) ?? properties;
            VerbPropertiesObjectPatch(childNodeIdForVerbProperties)
            .ForEach(x =>
            {
                //Log.Message(x.ToString());
                properties = (properties & x) ?? properties;
                properties = (properties | x) ?? properties;
            });
            return properties;
        }


        internal Tool ToolAfterAffect(Tool tool, string? childNodeIdForTool)
        {
            //tool = (Tool)tool.SimpleCopy();
            tool = (tool * ToolsMultiplier(childNodeIdForTool)) ?? tool;
            tool = (tool + ToolsOffseter(childNodeIdForTool)) ?? tool;
            tool = (tool & ToolsBoolAndPatch(childNodeIdForTool)) ?? tool;
            tool = (tool | ToolsBoolOrPatch(childNodeIdForTool)) ?? tool;
            ToolsObjectPatch(childNodeIdForTool)
            .ForEach(x =>
            {
                //Log.Message(x.ToString());
                tool = (tool & x) ?? tool;
                tool = (tool | x) ?? tool;
            });
            return tool;
        }


        protected override List<VerbPropertiesRegiestInfo> VerbPropertiesRegiestInfoUpadte(Type ownerType, List<VerbPropertiesRegiestInfo> result)
        {
            lock (verbPropertiesCache)
            {
                GetOrGenCurrentPartAttachmentProperties();
                GetOrGenTargetPartAttachmentProperties();
                int index = 0;
                NodeContainer? container = ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                List<Task<VerbPropertiesRegiestInfo>> tasks = new List<Task<VerbPropertiesRegiestInfo>>();
                List<VerbProperties> verbPropertiesCache = this.verbPropertiesCache.GetOrNewWhenNull(ownerType, () => new List<VerbProperties>());
                if (!parent.def.Verbs.NullOrEmpty() && CompChildNodeProccesser.GetSameTypeVerbOwner(ownerType, parent) == null)
                {

                    //result.Capacity += parent.def.Verbs.Count;
                    tasks.Capacity += parent.def.Verbs.Count;
                    foreach (VerbProperties properties in parent.def.Verbs)
                    {
                        int i = index;
                        VerbProperties boxed = properties;
                        tasks.Add(Task.Run(() => new VerbPropertiesRegiestInfo
                        (
                            null,
                            boxed,
                            (i < verbPropertiesCache.Count) ? verbPropertiesCache[i] : VerbPropertiesAfterAffect(
                                boxed,
                                null
                            )
                        )));
                        index++;
                        //VerbPropertiesRegiestInfo prop = ;
                        //result.Add(prop);
                    }
                }
                else
                {
                    tasks.Capacity += result.Count;
                    for (int i = 0; i < result.Count; i++)
                    {
                        int j = index;
                        VerbPropertiesRegiestInfo prop = result[i];
                        tasks.Add(Task.Run(() =>
                        {
                            prop.afterConvertProperties = (j < verbPropertiesCache.Count) ? verbPropertiesCache[j] : VerbPropertiesAfterAffect(
                                prop.berforConvertProperties,
                                null
                            );
                            return prop;
                        }));
                        index++;
                        //VerbProperties newProp = 
                        //newProp;
                        //result[i] = prop;
                    }
                    result.Clear();
                }

                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing child = container[i];
                    WeaponAttachmentProperties? attachmentProperties = CurrentPartWeaponAttachmentPropertiesById(id);
                    if (!internal_NotUseVerbProperties(child, attachmentProperties))
                    {
                        CompModularizationWeapon? comp = child;
                        List<VerbPropertiesRegiestInfo>? childInfos = comp?.NodeProccesser?.GetRegiestedNodeVerbPropertiesInfos(ownerType);
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
                            //result.Capacity += verbProperties.Count;
                            tasks.Capacity += verbProperties.Count;
                            for (int j = 0; j < verbProperties.Count; j++)
                            {

                                int k = index;
                                VerbProperties cache = verbProperties[j];
                                tasks.Add(Task.Run(() => new VerbPropertiesRegiestInfo
                                (
                                    id,
                                    cache,
                                    (k < verbPropertiesCache.Count) ? verbPropertiesCache[k] : VerbPropertiesAfterAffect(
                                    comp?.VerbPropertiesAfterAffect(
                                        cache,
                                        null
                                        ) ?? cache,
                                    id
                                    )
                                )));
                                index++;
                                //result.Add();
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
                result.Capacity += tasks.Count;
                foreach (Task<VerbPropertiesRegiestInfo> info in tasks) result.Add(info.Result);
                verbPropertiesCache.Clear();
                verbPropertiesCache.Capacity = result.Capacity;
                foreach (VerbPropertiesRegiestInfo info in result) verbPropertiesCache.Add(info.afterConvertProperties);
                return result;
            }
        }

        protected override List<VerbToolRegiestInfo> VerbToolRegiestInfoUpdate(Type ownerType, List<VerbToolRegiestInfo> result)
        {
            lock (toolsCache)
            {
                GetOrGenCurrentPartAttachmentProperties();
                GetOrGenTargetPartAttachmentProperties();
                int index = 0;
                NodeContainer? container = ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                List<Task<VerbToolRegiestInfo>> tasks = new List<Task<VerbToolRegiestInfo>>();
                List<Tool> toolsCache = this.toolsCache.GetOrNewWhenNull(ownerType, () => new List<Tool>());
                if (!parent.def.tools.NullOrEmpty() && CompChildNodeProccesser.GetSameTypeVerbOwner(ownerType, parent) == null)
                {
                    tasks.Capacity += parent.def.tools.Count;
                    foreach (Tool tool in parent.def.tools)
                    {
                        int i = index;
                        tasks.Add(Task.Run(() => new VerbToolRegiestInfo
                        (
                            null,
                            tool,
                            (i < toolsCache.Count) ? toolsCache[i] : ToolAfterAffect(
                                tool,
                                null
                            )
                        )));
                        index++;
                        //VerbToolRegiestInfo prop = ;
                        //result.Add(prop);
                    }
                }
                else
                {
                    tasks.Capacity += result.Count;
                    for (int i = 0; i < result.Count; i++)
                    {
                        int j = index;
                        VerbToolRegiestInfo prop = result[i];
                        tasks.Add(Task.Run(() =>
                        {
                            prop.afterConvertTool = (j < toolsCache.Count) ? toolsCache[j] : ToolAfterAffect(
                                prop.berforConvertTool,
                                null
                                );
                            return prop;
                        }));
                        index++;
                        //result[i] = prop;
                    }
                    result.Clear();
                }

                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    Thing child = container[i];
                    WeaponAttachmentProperties? attachmentProperties = CurrentPartWeaponAttachmentPropertiesById(id);
                    if (!internal_NotUseTools(child, attachmentProperties))
                    {
                        CompModularizationWeapon? comp = child;
                        List<VerbToolRegiestInfo>? childInfos = comp?.NodeProccesser?.GetRegiestedNodeVerbToolInfos(ownerType);
                        List<Tool> tools;
                        if (childInfos != null)
                        {
                            tools = new List<Tool>(childInfos.Count);
                            foreach (VerbToolRegiestInfo info in childInfos) tools.Add(info.afterConvertTool);
                        }
                        else
                        {
                            tools = child.def.tools ?? new List<Tool>();
                        }

                        if (tools != null)
                        {
                            tasks.Capacity += tools.Count;
                            for (int j = 0; j < tools.Count; j++)
                            {
                                int k = index;
                                Tool cache = tools[j];
                                tasks.Add(Task.Run(() => new VerbToolRegiestInfo
                                (
                                    id,
                                    cache,
                                    (k < toolsCache.Count) ? toolsCache[k] : ToolAfterAffect(
                                        comp?.ToolAfterAffect(
                                            cache,
                                            null
                                            ) ?? cache,
                                        id
                                    )
                                )));
                                index++;
                                //Tool newProp
                                //    = ;
                                //result.Add();
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

                result.Capacity += tasks.Count;
                foreach (Task<VerbToolRegiestInfo> info in tasks) result.Add(info.Result);
                toolsCache.Clear();
                toolsCache.Capacity = result.Capacity;
                foreach (VerbToolRegiestInfo info in result) toolsCache.Add(info.afterConvertTool);
                return result;
            }
        }
    }
}
