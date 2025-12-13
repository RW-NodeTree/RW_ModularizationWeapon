using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class ModularizationWeapon
    {
        internal CompProperties CompPropertiesAfterAffect(CompProperties compProperties, string? childNodeIdForCompProperties)
        {
            //tool = (Tool)tool.SimpleCopy();
            compProperties = (compProperties * CompPropertiesMultiplier(childNodeIdForCompProperties)) ?? compProperties;
            compProperties = (compProperties + CompPropertiesOffseter(childNodeIdForCompProperties)) ?? compProperties;
            compProperties = (compProperties & CompPropertiesBoolAndPatch(childNodeIdForCompProperties)) ?? compProperties;
            compProperties = (compProperties | CompPropertiesBoolOrPatch(childNodeIdForCompProperties)) ?? compProperties;
            CompPropertiesObjectPatch(childNodeIdForCompProperties)
            .ForEach(x =>
            {
                //Log.Message(x.ToString());
                compProperties = (compProperties & x) ?? compProperties;
                compProperties = (compProperties | x) ?? compProperties;
            });
            return compProperties;
        }

        internal static List<CompProperties> CompPropertiesFromThing(Thing thing)
        {
            ModularizationWeapon? comp = thing as ModularizationWeapon;
            if (comp != null)
            {
                ReadOnlyCollection<(string? id, int index, CompProperties afterConvert)> regiestInfos = comp.CompPropertiesRegiestInfo;
                List<CompProperties> result = new List<CompProperties>(regiestInfos.Count);
                foreach (var regiestInfo in regiestInfos)
                {
                    result.Add(regiestInfo.afterConvert);
                }
                return result;
            }
            else
            {
                return thing.def.comps ?? new List<CompProperties>();
            }
        }
        
        public ReadOnlyCollection<(string? id, int index, CompProperties afterConvert)> CompPropertiesRegiestInfo
        {
            get
            {
                lock (this)
                {
                    if (compPropertiesCache == null)
                    {

                        GetOrGenCurrentPartAttachmentProperties();
                        GetOrGenTargetPartAttachmentProperties();
                        NodeContainer? container = ChildNodes;
                        if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                        List<(string? id, int index, Task<CompProperties> afterConvert)> tasks = new List<(string? id, int index, Task<CompProperties> afterConvert)>();
                        if (!def.comps.NullOrEmpty())
                        {
                            tasks.Capacity += def.comps.Count;
                            for (int i = 0; i < def.comps.Count; i++)
                            {
                                CompProperties comp = def.comps[i];
                                tasks.Add((null, i, Task.Run(() => CompPropertiesAfterAffect(comp, null))));
                                //VerbToolRegiestInfo prop = ;
                                //result.Add(prop);
                            }
                        }
                        for (int i = 0; i < container.Count; i++)
                        {
                            string id = ((IList<string>)container)[i];
                            Thing child = container[i];
                            WeaponAttachmentProperties? attachmentProperties = CurrentPartWeaponAttachmentPropertiesById(id);
                            if (!internal_NotUseCompProperties(child, attachmentProperties))
                            {
                                List<CompProperties> comps = CompPropertiesFromThing(child);
                                tasks.Capacity += comps.Count;
                                for (int j = 0; j < comps.Count; j++)
                                {
                                    CompProperties comp = comps[j];
                                    tasks.Add((id, j, Task.Run(() => CompPropertiesAfterAffect(comp, id))));
                                    //Tool newProp
                                    //    = ;
                                    //result.Add();
                                }
                            }
                        }
                        //StringBuilder stringBuilder = new StringBuilder();
                        //for (int i = 0; i < result.Count; i++)
                        //{
                        //    stringBuilder.AppendLine($"{i} : {result[i]}");
                        //}
                        //Log.Message(stringBuilder.ToString());
                        List<(string? id, int index, CompProperties afterConvert)> result = new List<(string? id, int index, CompProperties afterConvert)>(tasks.Count);
                        foreach (var info in tasks) result.Add((info.id, info.index, info.afterConvert.Result));
                        compPropertiesCache = new ReadOnlyCollection<(string? id, int index, CompProperties afterConvert)>(result);
                    }
                    return compPropertiesCache;
                }
            }
        }

        internal static List<ThingComp> RestoreComps(List<ThingComp> next, List<ThingComp>? prev, ThingWithComps thing)
        {
            ModularizationWeapon? weapon = thing as ModularizationWeapon;
            if (weapon != null && weapon.swap)
            {
                
                List<ThingComp>? cache = prev;
                next = weapon.comps_TargetPart ?? next;
                weapon.comps_TargetPart = cache;
                weapon.def.comps.RemoveAll(x => next.Find(y => y.props == x) != null);
                return next;
            }
            return next;
        }

        
        private List<ThingComp>? comps_TargetPart = null;
        private ReadOnlyCollection<(string? id, int index, CompProperties afterConvert)>? compPropertiesCache = null;
        private ReadOnlyCollection<(string? id, int index, CompProperties afterConvert)>? compPropertiesCache_TargetPart = null;
    }
}
