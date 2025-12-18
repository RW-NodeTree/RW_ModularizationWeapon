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


        public float GetStatMultiplier(StatDef statDef, string? childNodeIdForState)
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
                statMultiplierCache ??= new Dictionary<(StatDef, string?), float>();
                if (!statMultiplierCache.TryGetValue((statDef, childNodeIdForState), out float result))
                {
                    result = 1;
                    for (int i = 0; i < container.Count; i++)
                    {
                        string id = ((IReadOnlyList<string>)container)[i];
                        ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                        attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                        if (weapon != null && properties != null && id != childNodeIdForState)
                        {
                            float cache = properties.statMultiplierAffectHorizon.GetStatValueFromList(statDef, properties.statMultiplierAffectHorizonDefaultValue);

                            if (current != null)
                            {
                                cache *= current.statOtherPartMultiplierAffectHorizon
                                .GetOrNewWhenNull(
                                    id,
                                    () => new List<StatModifier>()
                                ).GetStatValueFromList(statDef, current.statOtherPartMultiplierAffectHorizonDefaultValue);
                                if (currentWeapon != null && weapon != currentWeapon)
                                {
                                    cache *= currentWeapon.Props.statOtherPartMultiplierAffectHorizon.GetStatValueFromList(statDef, currentWeapon.Props.statOtherPartMultiplierAffectHorizonDefaultValue);
                                }
                            }
                            result *= (weapon.Props.statMultiplier.GetStatValueFromList(statDef, weapon.Props.statMultiplierDefaultValue) - 1) * cache + 1;
                            result *= (weapon.GetStatMultiplier(statDef, null) - 1) * cache + 1;
                        }
                    }
                    bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                    if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                    try
                    {
                        statMultiplierCache.Add((statDef, childNodeIdForState), result);
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
