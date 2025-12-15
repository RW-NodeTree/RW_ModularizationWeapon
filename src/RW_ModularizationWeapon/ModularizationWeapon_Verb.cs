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
        public static VerbProperties VerbPropertiesAfterAffect(VerbProperties properties, string? childNodeIdForVerbProperties, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            //properties = (VerbProperties)properties.SimpleCopy();
            properties = (properties * VerbPropertiesMultiplier(childNodeIdForVerbProperties, container, attachmentProperties)) ?? properties;
            properties = (properties + VerbPropertiesOffseter(childNodeIdForVerbProperties, container, attachmentProperties)) ?? properties;
            properties = (properties & VerbPropertiesBoolAndPatch(childNodeIdForVerbProperties, container, attachmentProperties)) ?? properties;
            properties = (properties | VerbPropertiesBoolOrPatch(childNodeIdForVerbProperties, container, attachmentProperties)) ?? properties;
            VerbPropertiesObjectPatch(childNodeIdForVerbProperties, container, attachmentProperties)
            .ForEach(x =>
            {
                //Log.Message(x.ToString());
                properties = (properties & x) ?? properties;
                properties = (properties | x) ?? properties;
            });
            return properties;
        }


        public static Tool ToolAfterAffect(Tool tool, string? childNodeIdForTool, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            //tool = (Tool)tool.SimpleCopy();
            tool = (tool * ToolsMultiplier(childNodeIdForTool, container, attachmentProperties)) ?? tool;
            tool = (tool + ToolsOffseter(childNodeIdForTool, container, attachmentProperties)) ?? tool;
            tool = (tool & ToolsBoolAndPatch(childNodeIdForTool, container, attachmentProperties)) ?? tool;
            tool = (tool | ToolsBoolOrPatch(childNodeIdForTool, container, attachmentProperties)) ?? tool;
            ToolsObjectPatch(childNodeIdForTool, container, attachmentProperties)
            .ForEach(x =>
            {
                //Log.Message(x.ToString());
                tool = (tool & x) ?? tool;
                tool = (tool | x) ?? tool;
            });
            return tool;
        }

        internal static List<Tool> ToolsFromThing(Thing thing)
        {
            ModularizationWeapon? comp = thing as ModularizationWeapon;
            if (comp != null)
            {
                ReadOnlyCollection<(string? id, int index, Tool afterConvert)> regiestInfos = comp.VerbToolRegiestInfo;
                List<Tool> result = new List<Tool>(regiestInfos.Count);
                foreach (var regiestInfo in regiestInfos)
                {
                    result.Add(regiestInfo.afterConvert);
                }
                return result;
            }
            else
            {
                return thing.def.tools ?? new List<Tool>();
            }
        }

        internal static List<VerbProperties> VerbPropertiesFromThing(Thing thing)
        {
            ModularizationWeapon? comp = thing as ModularizationWeapon;
            if (comp != null)
            {
                ReadOnlyCollection<(string? id, int index, VerbProperties afterConvert)> regiestInfos = comp.VerbPropertiesRegiestInfo;
                List<VerbProperties> result = new List<VerbProperties>(regiestInfos.Count);
                foreach (var regiestInfo in regiestInfos)
                {
                    result.Add(regiestInfo.afterConvert);
                }
                return result;
            }
            else
            {
                return thing.def.Verbs ?? new List<VerbProperties>();
            }
        }

        public ReadOnlyCollection<(string? id, int index, Tool afterConvert)> VerbToolRegiestInfo
        {
            get
            {
                NodeContainer? container = ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (toolsCache == null)
                    {

                        ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties = GetOrGenCurrentPartAttachmentProperties();
                        List<(string? id, int index, Task<Tool> afterConvert)> tasks = new List<(string? id, int index, Task<Tool> afterConvert)>();
                        if (!def.tools.NullOrEmpty())
                        {
                            tasks.Capacity += def.tools.Count;
                            for (int i = 0; i < def.tools.Count; i++)
                            {
                                Tool tool = def.tools[i];
                                tasks.Add((null, i, Task.Run(() => ToolAfterAffect(tool, null, container, attachmentProperties))));
                                //VerbToolRegiestInfo prop = ;
                                //result.Add(prop);
                            }
                        }
                        for (int i = 0; i < container.Count; i++)
                        {
                            string id = ((IList<string>)container)[i];
                            Thing child = container[i];
                            attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                            if (!NotUseTools(child, properties))
                            {
                                List<Tool> tools = ToolsFromThing(child);
                                tasks.Capacity += tools.Count;
                                for (int j = 0; j < tools.Count; j++)
                                {
                                    Tool tool = tools[j];
                                    tasks.Add((id, j, Task.Run(() => ToolAfterAffect(tool, id, container, attachmentProperties))));
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
                        List<(string? id, int index, Tool afterConvert)> result = new List<(string? id, int index, Tool afterConvert)>(tasks.Count);
                        foreach (var info in tasks) result.Add((info.id, info.index, info.afterConvert.Result));
                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            toolsCache = new ReadOnlyCollection<(string? id, int index, Tool afterConvert)>(result);
                        }
                        finally
                        {
                            if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                        }
                    }
                    return toolsCache;
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }

        public ReadOnlyCollection<(string? id, int index, VerbProperties afterConvert)> VerbPropertiesRegiestInfo
        {
            get
            {
                NodeContainer? container = ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (verbPropertiesCache == null)
                    {

                        ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties = GetOrGenCurrentPartAttachmentProperties();
                        List<(string? id, int index, Task<VerbProperties> afterConvert)> tasks = new List<(string? id, int index, Task<VerbProperties> afterConvert)>();
                        if (!def.Verbs.NullOrEmpty())
                        {

                            //result.Capacity += parent.def.Verbs.Count;
                            tasks.Capacity += def.Verbs.Count;
                            for (int i = 0; i < def.Verbs.Count; i++)
                            {
                                VerbProperties properties = def.Verbs[i];
                                tasks.Add((null, i, Task.Run(() => VerbPropertiesAfterAffect(properties, null, container, attachmentProperties))));
                                //VerbPropertiesRegiestInfo prop = ;
                                //result.Add(prop);
                            }
                        }

                        for (int i = 0; i < container.Count; i++)
                        {
                            string id = ((IList<string>)container)[i];
                            Thing child = container[i];
                            attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                            if (!NotUseVerbProperties(child, properties))
                            {
                                List<VerbProperties> verbProperties = VerbPropertiesFromThing(child);
                                tasks.Capacity += verbProperties.Count;
                                for (int j = 0; j < verbProperties.Count; j++)
                                {
                                    VerbProperties cache = verbProperties[j];
                                    tasks.Add((id, j, Task.Run(() => VerbPropertiesAfterAffect(cache, id, container, attachmentProperties))));
                                    //result.Add();
                                }
                            }
                        }
                        //StringBuilder stringBuildchildNodeIdForVerbProperties: er = new StringBuilder();
                        //for (int i = 0; i < result.Count; i++)
                        //{
                        //    stringBuilder.AppendLine($"{i} : {result[i]}");
                        //}
                        //Log.Message(stringBuilder.ToString());
                        List<(string? id, int index, VerbProperties afterConvert)> result = new List<(string? id, int index, VerbProperties afterConvert)>(tasks.Count);
                        foreach (var info in tasks) result.Add((info.id, info.index, info.afterConvert.Result));
                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            verbPropertiesCache = new ReadOnlyCollection<(string? id, int index, VerbProperties afterConvert)>(result);
                        }
                        finally
                        {
                            if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                        }
                    }
                    return verbPropertiesCache;
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }

        public ReadOnlyCollection<(CompEquippable, Verb)> ChildVariantVerbsOfTool(int toolIndex, VerbProperties? verbProperties = null)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                childVariantVerbsOfTool ??= new Dictionary<(int, VerbProperties?), ReadOnlyCollection<(CompEquippable, Verb)>>(VerbToolRegiestInfo.Count);
                if (!childVariantVerbsOfTool.TryGetValue((toolIndex, verbProperties), out var cached))
                {

                    var info = VerbToolRegiestInfo[toolIndex];

                    List<(CompEquippable, Verb)> variants = new List<(CompEquippable, Verb)>();
                    if (!info.id.NullOrEmpty())
                    {
                        ModularizationWeapon? child = ChildNodes[info.id!] as ModularizationWeapon;
                        if (child != null) variants.AddRange(child.ChildVariantVerbsOfTool(info.index, verbProperties));
                    }
                    CompEquippable? compEquippable = GetComp<CompEquippable>();
                    if (compEquippable != null)
                    {
                        Verb? verb = compEquippable.AllVerbs.FirstOrDefault(x => x.tool == info.afterConvert && (verbProperties == null || x.verbProps == verbProperties));
                        if (verb != null)
                        {
                            variants.Add((compEquippable, verb));
                        }
                    }
                    cached = new ReadOnlyCollection<(CompEquippable, Verb)>(variants);
                    bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                    if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                    try
                    {
                        childVariantVerbsOfTool[(toolIndex, verbProperties)] = cached;
                    }
                    finally
                    {
                        if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                    }
                }

                return cached;
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }

        }

        public ReadOnlyCollection<(CompEquippable, Verb)> ChildVariantVerbsOfVerbProp(int propIndex)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                childVariantVerbsOfVerbProp ??= new Dictionary<int, ReadOnlyCollection<(CompEquippable, Verb)>>(VerbPropertiesRegiestInfo.Count);
                if (!childVariantVerbsOfVerbProp.TryGetValue(propIndex, out var cached))
                {

                    var info = VerbPropertiesRegiestInfo[propIndex];

                    List<(CompEquippable, Verb)> variants = new List<(CompEquippable, Verb)>();
                    if (!info.id.NullOrEmpty())
                    {
                        ModularizationWeapon? child = ChildNodes[info.id!] as ModularizationWeapon;
                        if (child != null) variants.AddRange(child.ChildVariantVerbsOfVerbProp(info.index));
                    }
                    CompEquippable? compEquippable = GetComp<CompEquippable>();
                    if (compEquippable != null)
                    {
                        Verb? verb = compEquippable.AllVerbs.FirstOrDefault(x => x.verbProps == info.afterConvert);
                        if (verb != null)
                        {
                            variants.Add((compEquippable, verb));
                        }
                    }
                    cached = new ReadOnlyCollection<(CompEquippable, Verb)>(variants);
                    bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                    if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                    try
                    {
                        childVariantVerbsOfVerbProp[propIndex] = cached;
                    }
                    finally
                    {
                        if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                    }
                }

                return cached;
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }

        }

        public ReadOnlyCollection<(CompEquippable, Verb)> ChildVariantVerbsOfVerb(int verbIndex)
        {
            CompEquippable? compEquippable = GetComp<CompEquippable>();
            if(compEquippable == null) throw new InvalidOperationException("CompEquippable not found");

            Verb verb = compEquippable.AllVerbs[verbIndex];
            if (verb.tool != null)
            {
                int toolIndex = VerbToolRegiestInfo.FirstIndexOf(x => x.afterConvert == verb.tool);
                return ChildVariantVerbsOfTool(toolIndex, verb.verbProps);
            }
            else
            {
                int propIndex = VerbPropertiesRegiestInfo.FirstIndexOf(x => x.afterConvert == verb.verbProps);
                return ChildVariantVerbsOfVerbProp(propIndex);
            }
        }

        public ReadOnlyCollection<(CompEquippable, Verb)> ChildVariantVerbsOfPrimaryVerb
        {
            get
            {
                CompEquippable? compEquippable = GetComp<CompEquippable>();
                if(compEquippable == null) throw new InvalidOperationException("CompEquippable not found");

                Verb verb = compEquippable.PrimaryVerb;
                if (verb.tool != null)
                {
                    int toolIndex = VerbToolRegiestInfo.FirstIndexOf(x => x.afterConvert == verb.tool);
                    return ChildVariantVerbsOfTool(toolIndex, verb.verbProps);
                }
                else
                {
                    int propIndex = VerbPropertiesRegiestInfo.FirstIndexOf(x => x.afterConvert == verb.verbProps);
                    return ChildVariantVerbsOfVerbProp(propIndex);
                }
            }
        }


        private ReadOnlyCollection<(string? id, int index, Tool afterConvert)>? toolsCache = null;
        private ReadOnlyCollection<(string? id, int index, Tool afterConvert)>? toolsCache_TargetPart = null;
        private ReadOnlyCollection<(string? id, int index, VerbProperties afterConvert)>? verbPropertiesCache = null;
        private ReadOnlyCollection<(string? id, int index, VerbProperties afterConvert)>? verbPropertiesCache_TargetPart = null;
        private Dictionary<int, ReadOnlyCollection<(CompEquippable, Verb)>>? childVariantVerbsOfVerbProp = null;
        private Dictionary<int, ReadOnlyCollection<(CompEquippable, Verb)>>? childVariantVerbsOfVerbProp_TargetPart = null;
        private Dictionary<(int, VerbProperties?), ReadOnlyCollection<(CompEquippable, Verb)>>? childVariantVerbsOfTool = null;
        private Dictionary<(int, VerbProperties?), ReadOnlyCollection<(CompEquippable, Verb)>>? childVariantVerbsOfTool_TargetPart = null;
    }
}
