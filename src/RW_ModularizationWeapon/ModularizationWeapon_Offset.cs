using RimWorld;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class ModularizationWeapon
    {
        public static FieldReaderDgitList<VerbProperties> VerbPropertiesOffseter(string? childNodeIdForVerbProperties, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties) //null -> self
        {
            if(container == null) throw new NullReferenceException(nameof(container));
            if(attachmentProperties == null) throw new NullReferenceException(nameof(attachmentProperties));
            FieldReaderDgitList<VerbProperties> results = new FieldReaderDgitList<VerbProperties>();
            attachmentProperties.TryGetValue(childNodeIdForVerbProperties ?? "", out WeaponAttachmentProperties? current);
            ModularizationWeapon? currentWeapon = childNodeIdForVerbProperties != null ? container[childNodeIdForVerbProperties] as ModularizationWeapon : null;
            results.DefaultValue = 0;
            foreach (var kvp in container)
            {
                string id = kvp.Key;
                ModularizationWeapon? weapon = kvp.Value as ModularizationWeapon;
                attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                if (weapon != null && properties != null && id != childNodeIdForVerbProperties)
                {
                    FieldReaderDgitList<VerbProperties> cache = properties.verbPropertiesOffseterAffectHorizon;

                    if (current != null)
                    {
                        double defaultValue = cache.DefaultValue;
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
                        defaultValue *= current.verbPropertiesOtherPartOffseterAffectHorizonDefaultValue;
                        cache.DefaultValue = defaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache *= currentWeapon.Props.verbPropertiesOtherPartOffseterAffectHorizon;
                            cache.DefaultValue = defaultValue * currentWeapon.Props.verbPropertiesOtherPartOffseterAffectHorizon.DefaultValue;
                        }
                    }


                    FieldReaderDgitList<VerbProperties>
                    offset = weapon.Props.verbPropertiesOffseter * cache;
                    offset.DefaultValue = 0;
                    results += offset;
                    results.DefaultValue = 0;

                    offset = VerbPropertiesOffseter(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) * cache;
                    offset.DefaultValue = 0;
                    results += offset;
                    results.DefaultValue = 0;
                }
            }
            //Log.Message($"{this}.VerbPropertiesOffseter({childNodeIdForVerbProperties}) :\nresults : {results}");
            return results;
        }

        public FieldReaderDgitList<VerbProperties> VerbPropertiesOffseter(string? childNodeIdForVerbProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return VerbPropertiesOffseter(childNodeIdForVerbProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        // 将 internal 实例方法改为 public static，并去掉 Internal_ 前缀。
        public static FieldReaderDgitList<Tool> ToolsOffseter(string? childNodeIdForTool, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            if(container == null) throw new NullReferenceException(nameof(container));
            if(attachmentProperties == null) throw new NullReferenceException(nameof(attachmentProperties));
            FieldReaderDgitList<Tool> results = new FieldReaderDgitList<Tool>();
            attachmentProperties.TryGetValue(childNodeIdForTool ?? "", out WeaponAttachmentProperties? current);
            ModularizationWeapon? currentWeapon = childNodeIdForTool != null ? container[childNodeIdForTool] as ModularizationWeapon : null;
            results.DefaultValue = 0;
            foreach (var kvp in container)
            {
                string id = kvp.Key;
                ModularizationWeapon? weapon = kvp.Value as ModularizationWeapon;
                attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                if (weapon != null && properties != null && id != childNodeIdForTool)
                {
                    FieldReaderDgitList<Tool> cache = properties.toolsOffseterAffectHorizon;

                    if (current != null)
                    {
                        double defaultValue = cache.DefaultValue;
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
                        defaultValue *= current.toolsOtherPartOffseterAffectHorizonDefaultValue;
                        cache.DefaultValue = defaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache *= currentWeapon.Props.toolsOtherPartOffseterAffectHorizon;
                            cache.DefaultValue = defaultValue * currentWeapon.Props.toolsOtherPartOffseterAffectHorizon.DefaultValue;
                        }
                    }

                    FieldReaderDgitList<Tool>
                    offset = weapon.Props.toolsOffseter * cache;
                    offset.DefaultValue = 0;
                    results += offset;
                    results.DefaultValue = 0;

                    // 递归改为调用静态重载，传入子武器的 container 与 attachmentProperties
                    offset = ToolsOffseter(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) * cache;
                    offset.DefaultValue = 0;
                    results += offset;
                    results.DefaultValue = 0;
                }
            }
            //Log.Message($"{container}.ToolsOffseter({childNodeIdForTool}) :\nresults : {results}");
            return results;
        }

        public FieldReaderDgitList<Tool> ToolsOffseter(string? childNodeIdForTool)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return ToolsOffseter(childNodeIdForTool, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public static FieldReaderDgitList<CompProperties> CompPropertiesOffseter(string? childNodeIdForCompProperties, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            if(container == null) throw new NullReferenceException(nameof(container));
            if(attachmentProperties == null) throw new NullReferenceException(nameof(attachmentProperties));
            FieldReaderDgitList<CompProperties> results = new FieldReaderDgitList<CompProperties>();
            attachmentProperties.TryGetValue(childNodeIdForCompProperties ?? "", out WeaponAttachmentProperties? current);
            ModularizationWeapon? currentWeapon = childNodeIdForCompProperties != null ? container[childNodeIdForCompProperties] as ModularizationWeapon : null;
            results.DefaultValue = 0;
            foreach (var kvp in container)
            {
                string id = kvp.Key;
                ModularizationWeapon? weapon = kvp.Value as ModularizationWeapon;
                attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                if (weapon != null && properties != null && id != childNodeIdForCompProperties)
                {
                    FieldReaderDgitList<CompProperties> cache = properties.compPropertiesOffseterAffectHorizon;

                    if (current != null)
                    {
                        double defaultValue = cache.DefaultValue;
                        cache *=
                            current.compPropertiesOtherPartOffseterAffectHorizon
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderDgitList<CompProperties> result = new FieldReaderDgitList<CompProperties>();
                                    result.DefaultValue = current.compPropertiesOtherPartOffseterAffectHorizonDefaultValue;
                                    return result;
                                }
                            );
                        defaultValue *= current.compPropertiesOtherPartOffseterAffectHorizonDefaultValue;
                        cache.DefaultValue = defaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache *= currentWeapon.Props.compPropertiesOtherPartOffseterAffectHorizon;
                            cache.DefaultValue = defaultValue * currentWeapon.Props.compPropertiesOtherPartOffseterAffectHorizon.DefaultValue;
                        }
                    }

                    FieldReaderDgitList<CompProperties>
                    offset = weapon.Props.compPropertiesOffseter * cache;
                    offset.DefaultValue = 0;
                    results += offset;
                    results.DefaultValue = 0;

                    // 递归改为调用静态重载，传入子武器的 container 与 attachmentProperties
                    offset = CompPropertiesOffseter(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) * cache;
                    offset.DefaultValue = 0;
                    results += offset;
                    results.DefaultValue = 0;
                }
            }
            //Log.Message($"{container}.CompPropertiesOffseter({childNodeIdForCompProperties}) :\nresults : {results}");
            return results;
        }

        public FieldReaderDgitList<CompProperties> CompPropertiesOffseter(string? childNodeIdForCompProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return CompPropertiesOffseter(childNodeIdForCompProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public float GetStatOffset(StatDef statDef, string? childNodeIdForState)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties = GetOrGenCurrentPartAttachmentProperties();
                attachmentProperties.TryGetValue(childNodeIdForState ?? "", out WeaponAttachmentProperties? current);
                ModularizationWeapon? currentWeapon = childNodeIdForState != null ? container[childNodeIdForState] as ModularizationWeapon : null;
                statOffsetCache ??= new Dictionary<(StatDef, string?), float>();
                if (!statOffsetCache.TryGetValue((statDef, childNodeIdForState), out float result))
                {
                    result = 0;
                    for (int i = 0; i < container.Count; i++)
                    {
                        string id = ((IReadOnlyList<string>)container)[i];
                        ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                        attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                        if (weapon != null && properties != null && id != childNodeIdForState)
                        {
                            float cache = properties.statOffsetAffectHorizon.GetStatValueFromList(statDef, properties.statOffsetAffectHorizonDefaultValue);

                            if (current != null)
                            {
                                cache *= current.statOtherPartOffseterAffectHorizon
                                .GetOrNewWhenNull(
                                    id,
                                    () => new List<StatModifier>()
                                ).GetStatValueFromList(statDef, current.statOtherPartOffseterAffectHorizonDefaultValue);
                                if (currentWeapon != null && weapon != currentWeapon)
                                {
                                    cache *= currentWeapon.Props.statOtherPartOffseterAffectHorizon.GetStatValueFromList(statDef, currentWeapon.Props.statOtherPartOffseterAffectHorizonDefaultValue);
                                }
                            }
                            result += weapon.Props.statOffset.GetStatValueFromList(statDef, weapon.Props.statOffsetDefaultValue) * cache;
                            result += weapon.GetStatOffset(statDef, null) * cache;
                        }
                    }
                    bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                    if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                    try
                    {
                        statOffsetCache.Add((statDef, childNodeIdForState), result);
                    }
                    finally
                    {
                        if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                    }
                    //Log.Message($"{this}.GetStatOffset({statDef},{part})=>{result}\ncurrent.statOtherPartOffseterAffectHorizonDefaultValue : {current?.statOtherPartOffseterAffectHorizonDefaultValue}");
                }
                return result;
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }
    }
}
