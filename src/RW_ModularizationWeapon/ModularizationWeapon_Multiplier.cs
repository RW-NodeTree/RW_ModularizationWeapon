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

        public FieldReaderDgitList<VerbProperties> VerbPropertiesMultiplier(string? childNodeIdForVerbProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return WeaponProperties.VerbPropertiesMultiplier(childNodeIdForVerbProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }
        
        public FieldReaderDgitList<Tool> ToolsMultiplier(string? childNodeIdForVerbProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return WeaponProperties.ToolsMultiplier(childNodeIdForVerbProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public FieldReaderDgitList<CompProperties> CompPropertiesMultiplier(string? childNodeIdForVerbProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return WeaponProperties.CompPropertiesMultiplier(childNodeIdForVerbProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }


        public float GetStatMultiplier(StatDef statDef)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties = GetOrGenCurrentPartAttachmentProperties();
                if (statMultiplierCache == null || !statMultiplierCache.TryGetValue(statDef, out float result))
                {
                    result = 1;
                    for (int i = 0; i < container.Count; i++)
                    {
                        string id = ((IReadOnlyList<string>)container)[i];
                        ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                        attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                        if (weapon != null && properties != null)
                        {
                            float cache = properties.statMultiplierAffectHorizon.GetStatValueFromList(statDef, properties.statMultiplierAffectHorizonDefaultValue);
                            result *= (weapon.Props.statMultiplier.GetStatValueFromList(statDef, weapon.Props.statMultiplierDefaultValue) - 1) * cache + 1;
                            result *= (weapon.GetStatMultiplier(statDef) - 1) * cache + 1;
                        }
                    }
                    bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                    if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                    try
                    {
                        statMultiplierCache ??= new Dictionary<StatDef, float>();
                        statMultiplierCache.Add(statDef, result);
                    }
                    finally
                    {
                        if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                    }
                    //Log.Message($"{this}.GetStatMultiplier({statDef},{part})=>{result}\ncurrent.statOtherPartMultiplierAffectHorizonDefaultValue : {current?.statOtherPartMultiplierAffectHorizonDefaultValue}");
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
