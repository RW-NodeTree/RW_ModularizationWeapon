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

        public FieldReaderDgitList<VerbProperties> VerbPropertiesOffseter(string? childNodeIdForVerbProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return WeaponProperties.VerbPropertiesOffseter(childNodeIdForVerbProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }


        public FieldReaderDgitList<Tool> ToolsOffseter(string? childNodeIdForTool)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return WeaponProperties.ToolsOffseter(childNodeIdForTool, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public FieldReaderDgitList<CompProperties> CompPropertiesOffseter(string? childNodeIdForCompProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return WeaponProperties.CompPropertiesOffseter(childNodeIdForCompProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
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
