using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class ModularizationWeapon
    {
        public static CompProperties CompPropertiesAfterAffect(CompProperties compProperties, string? childNodeIdForCompProperties, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            //tool = (Tool)tool.SimpleCopy();
            compProperties = (compProperties * CompPropertiesMultiplier(childNodeIdForCompProperties, container, attachmentProperties)) ?? compProperties;
            compProperties = (compProperties + CompPropertiesOffseter(childNodeIdForCompProperties, container, attachmentProperties)) ?? compProperties;
            compProperties = (compProperties & CompPropertiesBoolAndPatch(childNodeIdForCompProperties, container, attachmentProperties)) ?? compProperties;
            compProperties = (compProperties | CompPropertiesBoolOrPatch(childNodeIdForCompProperties, container, attachmentProperties)) ?? compProperties;
            CompPropertiesObjectPatch(childNodeIdForCompProperties, container, attachmentProperties)
            .ForEach(x =>
            {
                //Log.Message(x.ToString());
                compProperties = (compProperties & x) ?? compProperties;
                compProperties = (compProperties | x) ?? compProperties;
            });
            return compProperties;
        }

        internal static List<CompProperties> CompPropertiesFromThing(Thing thing, out ReaderWriterLockSlim? lockSlim)
        {
            lockSlim = null;
            ModularizationWeapon? comp = thing as ModularizationWeapon;
            if (comp != null)
            {
                ReadOnlyCollection<(string? id, int index, CompProperties afterConvert)> regiestInfos = comp.CompPropertiesRegiestInfo;
                List<CompProperties> result = new List<CompProperties>(regiestInfos.Count);
                foreach (var regiestInfo in regiestInfos)
                {
                    result.Add(regiestInfo.afterConvert);
                }
                lockSlim = comp.readerWriterLockSlim;
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
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (compPropertiesCache == null)
                    {

                        NodeContainer? container = ChildNodes;
                        if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                        ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties = GetOrGenCurrentPartAttachmentProperties();
                        List<(string? id, int index, Task<CompProperties> afterConvert)> tasks = new List<(string? id, int index, Task<CompProperties> afterConvert)>();
                        if (!def.comps.NullOrEmpty())
                        {
                            tasks.Capacity += def.comps.Count;
                            for (int i = 0; i < def.comps.Count; i++)
                            {
                                CompProperties comp = def.comps[i];
                                tasks.Add((null, i, Task.Run(() => CompPropertiesAfterAffect(comp, null, container, attachmentProperties))));
                                //VerbToolRegiestInfo prop = ;
                                //result.Add(prop);
                            }
                        }
                        for (int i = 0; i < container.Count; i++)
                        {
                            string id = ((IReadOnlyList<string>)container)[i];
                            Thing child = container[i];
                            attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                            if (!NotUseCompProperties(child, properties))
                            {
                                List<CompProperties> comps = CompPropertiesFromThing(child, out _);
                                tasks.Capacity += comps.Count;
                                for (int j = 0; j < comps.Count; j++)
                                {
                                    CompProperties comp = comps[j];
                                    tasks.Add((id, j, Task.Run(() => CompPropertiesAfterAffect(comp, id, container, attachmentProperties))));
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
                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            compPropertiesCache = new ReadOnlyCollection<(string? id, int index, CompProperties afterConvert)>(result);
                        }
                        finally
                        {
                            if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                        }
                    }
                    return compPropertiesCache;
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
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

        
    }
}
