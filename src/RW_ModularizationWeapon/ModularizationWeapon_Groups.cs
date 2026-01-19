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
    public partial class ModularizationWeapon : IWeaponPropertiesHolder
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
                    if(currentWeaponMode != value && value < InheritableProperties.Count)
                    {
                        Pawn_EquipmentTracker? equipmentTracker = ParentHolder as Pawn_EquipmentTracker;
                        equipmentTracker?.Remove(this);
                        currentWeaponMode = value;
                        InitializeComps();
                        WeaponPropertiesPostMake();
                        equipmentTracker?.AddEquipment(this);
                    }
                }
                finally
                {
                    if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                }
            }
        }

        public WeaponProperties PrivateProperties
        {
            get
            {
                
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (privatePropertiesCache == null)
                    {
                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            privatePropertiesCache = new WeaponProperties(this);
                        }
                        finally
                        {
                            if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                        }
                    }
                    return privatePropertiesCache;
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }

        public ReadOnlyCollection<CompProperties_ModularizationWeaponEquippable> InheritableProperties
        {
            get
            {
                NodeContainer? container = ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (inheritablePropertiesCache == null)
                    {
                        List<CompProperties_ModularizationWeaponEquippable> equippableProperties = new List<CompProperties_ModularizationWeaponEquippable>(Math.Max(Props.weaponPropertiesInfos.Count, 1));
                        for(uint i = 0; i < equippableProperties.Capacity; i++)
                        {
                            equippableProperties.Add(new CompProperties_ModularizationWeaponEquippable(this, null, i));
                        }
                        ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties = GetOrGenCurrentPartAttachmentProperties();
                        foreach (var kv in container)
                        {
                            if(kv.Item2 is ModularizationWeapon child)
                            {
                                WeaponAttachmentProperties attachment = attachmentProperties[kv.Item1];
                                bool uasTools = !child.Props.notAllowParentUseTools && !attachment.notUseTools;
                                bool uasVerbProps = !child.Props.notAllowParentUseVerbProperties && !attachment.notUseVerbProperties;
                                bool uasCompProps = !child.Props.notAllowParentUseCompProperties && !attachment.notUseCompProperties;
                                equippableProperties.Capacity += child.InheritableProperties.Count;
                                for(uint i = 0; i < child.InheritableProperties.Count; i++)
                                {
                                    CompProperties_ModularizationWeaponEquippable childMode = child.InheritableProperties[(int)i];
                                    if (
                                        uasTools && childMode.Tools.Count > 0 ||
                                        uasVerbProps && childMode.VerbProperties.Count > 0 ||
                                        uasCompProps && childMode.CompProperties.Count > 0
                                    )
                                    {
                                        equippableProperties.Add(new CompProperties_ModularizationWeaponEquippable(this, kv.Item1, i));
                                    }
                                }
                            }
                        }

                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            inheritablePropertiesCache = new ReadOnlyCollection<CompProperties_ModularizationWeaponEquippable>(equippableProperties);
                        }
                        finally
                        {
                            if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                        }
                    }
                    return inheritablePropertiesCache;
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
                int mode = (int)CurrentMode;
                var props = InheritableProperties;
                if(mode < props.Count)
                {
                    return props[mode].Name;
                }
                return Label;
            }
        }

        public Color CurrentModeColor
        {
            get
            {
                int mode = (int)CurrentMode;
                var props = InheritableProperties;
                if(mode < props.Count)
                {
                    return props[mode].Color;
                }
                return base.DrawColor;
            }
        }

        public Texture2D CurrentModeIcon
        {
            get
            {
                int mode = (int)CurrentMode;
                var props = InheritableProperties;
                if(mode < props.Count)
                {
                    return props[mode].Icon;
                }
                return (Graphic?.MatSingle?.mainTexture as Texture2D) ?? def.uiIcon;
            }
        }

        public string? SourceChildID => null;

        public ModularizationWeapon Weapon => this;

        public ReadOnlyCollection<Tool> Tools => new ReadOnlyCollection<Tool>(def.tools ?? new List<Tool>());

        public ReadOnlyCollection<VerbProperties> VerbProperties => new ReadOnlyCollection<VerbProperties>(def.Verbs ?? new List<VerbProperties>());

        public ReadOnlyCollection<CompProperties> CompProperties => new ReadOnlyCollection<CompProperties>(def.comps ?? new List<CompProperties>());



        private void WeaponPropertiesPostMake()
        {
            try
            {
                PrivateProperties.PostMake();
            }
            catch(Exception ex)
            {
                const int key = ('M' << 24) | ('W' << 16) | ('P' << 8) | 'M';
                Log.ErrorOnce(ex.ToString(), key);
            }
            var props = InheritableProperties;
            try
            {
                if(currentWeaponMode < props.Count)
                {
                    props[(int)currentWeaponMode].PostMake();
                }
            }
            catch(Exception ex)
            {
                const int key = ('M' << 24) | ('W' << 16) | ('P' << 8) | 'M';
                Log.ErrorOnce(ex.ToString(), key);
            }
        }


        public List<Tool> ToolsFromThing(uint index)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                List<Tool> result = [.. PrivateProperties.VerbToolRegiestInfo];
                var props = InheritableProperties;
                if (props.Count > index)
                {
                    WeaponProperties inheritableProperties = props[(int)index].weaponProperties;
                    result.Capacity += inheritableProperties.VerbToolRegiestInfo.Count;
                    result.AddRange(inheritableProperties.VerbToolRegiestInfo);
                }
                return result;
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }


        public List<VerbProperties> VerbPropertiesFromThing(uint index)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                List<VerbProperties> result = [.. PrivateProperties.VerbPropertiesRegiestInfo];
                var props = InheritableProperties;
                if (props.Count > index)
                {
                    WeaponProperties inheritableProperties = props[(int)index].weaponProperties;
                    result.Capacity += inheritableProperties.VerbPropertiesRegiestInfo.Count;
                    result.AddRange(inheritableProperties.VerbPropertiesRegiestInfo);
                }
                return result;
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }
        

        public List<CompProperties> CompPropertiesFromThing(uint index)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                List<CompProperties> result = [.. PrivateProperties.CompPropertiesRegiestInfo];
                var props = InheritableProperties;
                if (props.Count > index)
                {
                    CompProperties_ModularizationWeaponEquippable inheritableProperties = props[(int)index];
                    result.Add(inheritableProperties);
                    result.Capacity += inheritableProperties.weaponProperties.CompPropertiesRegiestInfo.Count;
                    result.AddRange(inheritableProperties.weaponProperties.CompPropertiesRegiestInfo);
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
