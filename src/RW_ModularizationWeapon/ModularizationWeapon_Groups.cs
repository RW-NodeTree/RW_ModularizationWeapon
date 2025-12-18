using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class ModularizationWeapon
    {

        public uint CurrentMode
        {
            get
            {
                bool isReadLockHeld = readerWriterLockSlim.IsReadLockHeld || readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isReadLockHeld) readerWriterLockSlim.EnterReadLock();
                try
                {
                    return currentWeaponMode;
                }
                finally
                {
                    if (!isReadLockHeld) readerWriterLockSlim.ExitReadLock();
                }
            }
            set
            {
                bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                try
                {
                    if(currentWeaponMode != value && (value < 1 || value < ProtectedProperties.Count))
                    {
                        Pawn_EquipmentTracker? equipmentTracker = ParentHolder as Pawn_EquipmentTracker;
                        equipmentTracker?.Remove(this);
                        currentWeaponMode = value;
                        InitializeComps();
                        equipmentTracker?.AddEquipment(this);
                    }
                }
                finally
                {
                    if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                }
            }
        }

        public WeaponProperties PublicProperties
        {
            get
            {
                
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (publicPropertiesCache == null)
                    {
                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            publicPropertiesCache = new WeaponProperties(this, null, -1);
                        }
                        finally
                        {
                            if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                        }
                    }
                    return publicPropertiesCache;
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }

        public ReadOnlyCollection<WeaponProperties> ProtectedProperties
        {
            get
            {
                NodeContainer? container = ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (protectedPropertiesCache == null)
                    {
                        List<WeaponProperties> weaponProperties = new List<WeaponProperties>();
                        int count = Mathf.Max(
                            Props.allPrimaryVerbProperties.Count,
                            Props.protectedVerbProperties.Count == 0 ? 0 : 1,
                            Props.protectedTools.Count == 0 ? 0 : 1,
                            Props.protectedCompProperties.Count == 0 ? 0 : 1
                        );
                        weaponProperties.Capacity += count;
                        for(int i = 0; i < count; i++)
                        {
                            weaponProperties.Add(new WeaponProperties(this, null, i));
                        }
                        foreach (var kv in container)
                        {
                            ModularizationWeapon? child = kv.Item2 as ModularizationWeapon;
                            if(child != null)
                            {
                                weaponProperties.Capacity += child.ProtectedProperties.Count;
                                for(int i = 0; i < child.ProtectedProperties.Count; i++)
                                {
                                    weaponProperties.Add(new WeaponProperties(this, kv.Item1, i));
                                }
                            }
                            else
                            {
                                count = kv.Item2.def.Verbs.Count(x => x.isPrimary);
                                weaponProperties.Capacity += count;
                                for(int i = 0; i < count; i++)
                                {
                                    weaponProperties.Add(new WeaponProperties(this, kv.Item1, i));
                                }
                                
                            }
                        }

                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            protectedPropertiesCache = new ReadOnlyCollection<WeaponProperties>(weaponProperties);
                        }
                        finally
                        {
                            if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                        }
                    }
                    return protectedPropertiesCache;
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }
        
        public string CurrentModeName
        {
            get
            {
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if(ProtectedProperties.Count > currentWeaponMode)
                    {
                        return ProtectedProperties[(int)currentWeaponMode].Name;
                    }
                    else
                    {
                        return PublicProperties.Name;
                    }
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }
        
        public Color CurrentModeColor
        {
            get
            {
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if(ProtectedProperties.Count > currentWeaponMode)
                    {
                        return ProtectedProperties[(int)currentWeaponMode].Color;
                    }
                    else
                    {
                        return PublicProperties.Color;
                    }
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }
        
        public Texture2D CurrentModeIcon
        {
            get
            {
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if(ProtectedProperties.Count > currentWeaponMode)
                    {
                        return ProtectedProperties[(int)currentWeaponMode].Icon;
                    }
                    else
                    {
                        return PublicProperties.Icon;
                    }
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }

        internal static List<VerbProperties> PublicVerbPropertiesFromThing(Thing thing)
        {
            WeaponProperties? mode = (thing as ModularizationWeapon)?.PublicProperties;
            if (mode != null)
            {
                ReadOnlyCollection<(string? id, uint index, VerbProperties afterConvert)> regiestInfos = mode.VerbPropertiesRegiestInfo;
                List<VerbProperties> result = new List<VerbProperties>(regiestInfos.Count);
                foreach (var regiestInfo in regiestInfos)
                {
                    result.Add(regiestInfo.afterConvert);
                }
                return result;
            }
            else
            {
                List<VerbProperties> result = thing.def.Verbs == null ? [] : [.. thing.def.Verbs];
                result.RemoveAll(x => x.isPrimary);
                return result;
            }
        }

        internal static List<VerbProperties> ProtectedVerbPropertiesFromThing(Thing thing, int index)
        {
            WeaponProperties? mode = (thing as ModularizationWeapon)?.ProtectedProperties[index];
            if (mode != null)
            {
                ReadOnlyCollection<(string? id, uint index, VerbProperties afterConvert)> regiestInfos = mode.VerbPropertiesRegiestInfo;
                List<VerbProperties> result = new List<VerbProperties>(regiestInfos.Count);
                foreach (var regiestInfo in regiestInfos)
                {
                    result.Add(regiestInfo.afterConvert);
                }
                return result;
            }
            else
            {
                List<VerbProperties> result = thing.def.Verbs == null ? [] : [.. thing.def.Verbs];
                result.RemoveAll(x => !x.isPrimary);
                result = [result[index]];
                return result;
            }
        }

        internal static List<VerbProperties> VerbPropertiesFromThing(Thing thing)
        {
            ModularizationWeapon? weapon = thing as ModularizationWeapon;
            if(weapon != null)
            {
                WeaponProperties publicProperties = weapon.PublicProperties;
                List<VerbProperties> result = new List<VerbProperties>(publicProperties.VerbPropertiesRegiestInfo.Count);
                foreach (var regiestInfo in publicProperties.VerbPropertiesRegiestInfo)
                {
                    result.Add(regiestInfo.afterConvert);
                }
                var props = weapon.ProtectedProperties;
                if (props.Count > weapon.currentWeaponMode)
                {
                    WeaponProperties protectedProperties = props[(int)weapon.currentWeaponMode];
                    result.Capacity += protectedProperties.VerbPropertiesRegiestInfo.Count;
                    foreach (var regiestInfo in protectedProperties.VerbPropertiesRegiestInfo)
                    {
                        result.Add(regiestInfo.afterConvert);
                    }
                }
                return result;
            }
            else
            {
                return thing.def.Verbs ?? [];
            }
        }
        

        internal static List<Tool> PublicToolsFromThing(Thing thing)
        {
            WeaponProperties? mode = (thing as ModularizationWeapon)?.PublicProperties;
            if (mode != null)
            {
                ReadOnlyCollection<(string? id, uint index, Tool afterConvert)> regiestInfos = mode.VerbToolRegiestInfo;
                List<Tool> result = new List<Tool>(regiestInfos.Count);
                foreach (var regiestInfo in regiestInfos)
                {
                    result.Add(regiestInfo.afterConvert);
                }
                return result;
            }
            else
            {
                return thing.def.tools ?? [];
            }
        }
        

        internal static List<Tool> ProtectedToolsFromThing(Thing thing, int index)
        {
            WeaponProperties? mode = (thing as ModularizationWeapon)?.ProtectedProperties[index];
            if (mode != null)
            {
                ReadOnlyCollection<(string? id, uint index, Tool afterConvert)> regiestInfos = mode.VerbToolRegiestInfo;
                List<Tool> result = new List<Tool>(regiestInfos.Count);
                foreach (var regiestInfo in regiestInfos)
                {
                    result.Add(regiestInfo.afterConvert);
                }
                return result;
            }
            return [];
        }


        internal static List<Tool> ToolsFromThing(Thing thing)
        {
            ModularizationWeapon? weapon = thing as ModularizationWeapon;
            if(weapon != null)
            {
                WeaponProperties publicProperties = weapon.PublicProperties;
                List<Tool> result = new List<Tool>(publicProperties.VerbToolRegiestInfo.Count);
                foreach (var regiestInfo in publicProperties.VerbToolRegiestInfo)
                {
                    result.Add(regiestInfo.afterConvert);
                }
                var props = weapon.ProtectedProperties;
                if (props.Count > weapon.currentWeaponMode)
                {
                    WeaponProperties protectedProperties = props[(int)weapon.currentWeaponMode];
                    result.Capacity += protectedProperties.VerbToolRegiestInfo.Count;
                    foreach (var regiestInfo in protectedProperties.VerbToolRegiestInfo)
                    {
                        result.Add(regiestInfo.afterConvert);
                    }
                }
                return result;
            }
            else
            {
                return thing.def.tools ?? [];
            }
        }


        internal static List<CompProperties> PublicCompPropertiesFromThing(Thing thing)
        {
            WeaponProperties? comp = (thing as ModularizationWeapon)?.PublicProperties;
            if (comp != null)
            {
                ReadOnlyCollection<(string? id, uint index, CompProperties afterConvert)> regiestInfos = comp.CompPropertiesRegiestInfo;
                List<CompProperties> result = new List<CompProperties>(regiestInfos.Count);
                foreach (var regiestInfo in regiestInfos)
                {
                    result.Add(regiestInfo.afterConvert);
                }
                return result;
            }
            else
            {
                List<CompProperties> result = thing.def.comps == null ? [] : [.. thing.def.comps];
                result.RemoveAll(x => typeof(CompEquippable).IsAssignableFrom(x.compClass));
                return result;
            }
        }


        internal static List<CompProperties> ProtectedCompPropertiesFromThing(Thing thing, int index)
        {
            WeaponProperties? comp = (thing as ModularizationWeapon)?.ProtectedProperties[index];
            if (comp != null)
            {
                ReadOnlyCollection<(string? id, uint index, CompProperties afterConvert)> regiestInfos = comp.CompPropertiesRegiestInfo;
                List<CompProperties> result = new List<CompProperties>(regiestInfos.Count);
                foreach (var regiestInfo in regiestInfos)
                {
                    result.Add(regiestInfo.afterConvert);
                }
                return result;
            }
            else
            {
                CompProperties? compProperties = thing.def.comps?.First(x => typeof(CompEquippable).IsAssignableFrom(x.compClass));
                List<CompProperties> result = compProperties == null ? [] : [compProperties];
                return result;
            }
        }
        

        internal static List<CompProperties> CompPropertiesFromThing(Thing thing)
        {
            ModularizationWeapon? weapon = thing as ModularizationWeapon;
            if(weapon != null)
            {
                WeaponProperties publicProperties = weapon.PublicProperties;
                List<CompProperties> result = new List<CompProperties>(publicProperties.CompPropertiesRegiestInfo.Count);
                foreach (var regiestInfo in publicProperties.CompPropertiesRegiestInfo)
                {
                    result.Add(regiestInfo.afterConvert);
                }
                var props = weapon.ProtectedProperties;
                if (props.Count > weapon.currentWeaponMode)
                {
                    WeaponProperties protectedProperties = props[(int)weapon.currentWeaponMode];
                    result.Capacity += protectedProperties.CompPropertiesRegiestInfo.Count;
                    foreach (var regiestInfo in protectedProperties.CompPropertiesRegiestInfo)
                    {
                        result.Add(regiestInfo.afterConvert);
                    }
                }
                return result;
            }
            else
            {
                return thing.def.comps ?? [];
            }
        }

        
    }
}
