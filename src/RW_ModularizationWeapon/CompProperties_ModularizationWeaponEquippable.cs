
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RimWorld;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon
{
    public class CompProperties_ModularizationWeaponEquippable : CompProperties, IWeaponPropertiesHolder
    {
        //index == -1 -> public; id == null -> self
        internal CompProperties_ModularizationWeaponEquippable(ModularizationWeapon weapon, string? childId, uint index) : base(typeof(CompEquippable))
        {
            if (weapon == null) throw new NullReferenceException(nameof(weapon));
            this.Weapon = weapon;
            this.SourceChildID = childId;
            this.index = index;
            this.weaponProperties = new WeaponProperties(this);
        }
        

        public string Name
        {
            get
            {
                NodeContainer? container = Weapon.ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(Weapon.ChildNodes));
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (nameCache == null)
                    {
                        string? finalName = null;
                        if(SourceChildID == null)
                        {
                            finalName = Weapon.Label + " : Mode" + index;
                        }
                        else
                        {
                            Thing? child = container[SourceChildID];
                            ModularizationWeapon? weapon = child as ModularizationWeapon;
                            if (weapon != null)
                            {
                                finalName = weapon.InheritableProperties[(int)index].Name;
                            }
                            else if(child != null)
                            {
                                finalName = child.Label + " : Mode" + index;
                            }
                        }
                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            nameCache = finalName ?? "NULL";
                        }
                        finally
                        {
                            if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                        }
                    }
                    return nameCache;
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }
        
        

        public Color Color
        {
            get
            {
                NodeContainer? container = Weapon.ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(Weapon.ChildNodes));
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (colorCache == null)
                    {
                        Color finalColor = Color.white;
                        if(SourceChildID == null)
                        {
                            finalColor = Weapon.DrawColor;
                        }
                        else
                        {
                            Thing? child = container[SourceChildID];
                            ModularizationWeapon? weapon = child as ModularizationWeapon;
                            if (weapon != null)
                            {
                                finalColor = weapon.InheritableProperties[(int)index].Color;
                            }
                            else if(child != null)
                            {
                                finalColor = child.DrawColor;
                            }
                        }
                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            colorCache = finalColor;
                        }
                        finally
                        {
                            if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                        }
                    }
                    return colorCache.Value;
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }

        public Texture2D Icon
        {
            get
            {
                NodeContainer? container = Weapon.ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(Weapon.ChildNodes));
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (iconCache == null)
                    {
                        Texture2D? finalIcon = null;
                        if(SourceChildID == null)
                        {
                            finalIcon = (Weapon.Graphic?.MatSingle?.mainTexture as Texture2D) ?? Weapon.def.uiIcon;
                        }
                        else
                        {
                            Thing? child = container[SourceChildID];
                            ModularizationWeapon? weapon = child as ModularizationWeapon;
                            if (weapon != null)
                            {
                                finalIcon = weapon.InheritableProperties[(int)index].Icon;
                            }
                            else if(child != null)
                            {
                                finalIcon = (child.Graphic?.MatSingle?.mainTexture as Texture2D) ?? child.def.uiIcon;
                            }
                        }
                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            iconCache = finalIcon ?? BaseContent.BadTex;
                        }
                        finally
                        {
                            if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                        }
                    }
                    return iconCache;
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }

        public string? SourceChildID { get; }

        public ModularizationWeapon Weapon { get; }

        public ReadOnlyCollection<Tool> Tools
        {
            get
            {
                NodeContainer? container = Weapon.ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(Weapon.ChildNodes));
                if (SourceChildID == null)
                {
                    if (index < Weapon.Props.equippableModeInfos.Count)
                    {
                        return new ReadOnlyCollection<Tool>(Weapon.Props.equippableModeInfos[(int)index].inheritableTools);
                    }
                }
                else if(container[SourceChildID] is ModularizationWeapon weapon)
                {
                    if (index < weapon.InheritableProperties.Count)
                    {
                        return weapon.InheritableProperties[(int)index].weaponProperties.VerbToolRegiestInfo;
                    }
                }
                return new ReadOnlyCollection<Tool>(new List<Tool>());
            }
        }

        public ReadOnlyCollection<VerbProperties> VerbProperties
        {
            get
            {
                NodeContainer? container = Weapon.ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(Weapon.ChildNodes));
                if (SourceChildID == null)
                {
                    if (index < Weapon.Props.equippableModeInfos.Count)
                    {
                        return new ReadOnlyCollection<VerbProperties>(Weapon.Props.equippableModeInfos[(int)index].inheritableVerbProperties);
                    }
                }
                else if(container[SourceChildID] is ModularizationWeapon weapon)
                {
                    if (index < weapon.InheritableProperties.Count)
                    {
                        return weapon.InheritableProperties[(int)index].weaponProperties.VerbPropertiesRegiestInfo;
                    }
                }
                return new ReadOnlyCollection<VerbProperties>(new List<VerbProperties>());
            }
        }

        public ReadOnlyCollection<CompProperties> CompProperties
        {
            get
            {
                NodeContainer? container = Weapon.ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(Weapon.ChildNodes));
                if (SourceChildID == null)
                {
                    if (index < Weapon.Props.equippableModeInfos.Count)
                    {
                        return new ReadOnlyCollection<CompProperties>(Weapon.Props.equippableModeInfos[(int)index].inheritableCompProperties);
                    }
                }
                else if(container[SourceChildID] is ModularizationWeapon weapon)
                {
                    if (index < weapon.InheritableProperties.Count)
                    {
                        return weapon.InheritableProperties[(int)index].weaponProperties.CompPropertiesRegiestInfo;
                    }
                }
                return new ReadOnlyCollection<CompProperties>(new List<CompProperties>());
            }
        }
        

        internal void DestroyComps()
        {
            bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
            if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
            try
            {
                if(maked && instanceCache != null && !Weapon.AllComps.Contains(instanceCache))
                {
                    foreach(var destructor in Weapon.Props.thingCompDestructors)
                    {
                        destructor.DestroyComp(Weapon, instanceCache);
                    }
                }
                weaponProperties.DestroyComps();
                instanceCache = null;
                maked = false;
            }
            finally
            {
                if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void MarkAllMaked()
        {
            bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
            if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
            try
            {
                maked = instanceCache != null;
                if(maked)
                {
                    weaponProperties.MarkAllMaked();
                }
            }
            finally
            {
                if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void ExposeData()
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                if (maked && instanceCache != null)
                {
                    bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                    if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                    try
                    {
                        instanceCache.PostExposeData();
                        weaponProperties.ExposeData();
                    }
                    finally
                    {
                        if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                    }
                }
                else if (Scribe.mode == LoadSaveMode.Saving)
                {
                    Scribe.saver.WriteAttribute("IsNull", "True");
                }
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public void PostMake()
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                if (!maked && instanceCache != null)
                {
                    bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                    if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                    try
                    {
                        instanceCache.PostPostMake();
                        weaponProperties.PostMake();
                        maked = true;
                    }
                    finally
                    {
                        if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                    }
                }
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }
        
        internal void PreInitComps(ref bool equippablePropertiesNeedExitLock, ref bool inheritablePropertiesNeedExitLock)
        {
            equippablePropertiesNeedExitLock = !readerWriterLockSlim.IsUpgradeableReadLockHeld && !readerWriterLockSlim.IsWriteLockHeld;
            if (equippablePropertiesNeedExitLock) readerWriterLockSlim.EnterUpgradeableReadLock();
            inheritablePropertiesNeedExitLock = weaponProperties.PreInitComps();
        }

        internal void RestoreComps(List<ThingComp> next)
        {
            if (maked && instanceCache != null)
            {
                //every props in def.comps will be need create and need invoke PostMake
                int index = Weapon.def.comps.FirstIndexOf(x => x == this);
                if (index >= 0)
                {
                    Weapon.def.comps.RemoveAt(index); //def.comps = def.comps & !comps_maked
                    next.Add(instanceCache);
                }
            }
            else
            {
                maked = false;
            }
            weaponProperties.RestoreComps(next);
        }

        internal void FinalInitComps(List<ThingComp> comps, bool equippablePropertiesNeedExitLock, bool inheritablePropertiesNeedExitLock)
        {
            try
            {
                bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                try
                {
                    this.instanceCache = comps.Find(x => x.props == this) as CompEquippable; //this.comps = def.comps & compProperties
                }
                finally
                {
                    if(!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                }
                weaponProperties.FinalInitComps(comps, inheritablePropertiesNeedExitLock);
            }
            finally
            {
                if (equippablePropertiesNeedExitLock) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }
        public readonly uint index;
        public readonly WeaponProperties weaponProperties;

        private bool maked = false;
        private string? nameCache = null;
        private Color? colorCache = null;
        private Texture2D? iconCache = null;
        private CompEquippable? instanceCache = null;

        private readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();
    }


    public class EquippableModeInfo
    {

        public void ResolveReferences(ThingDef def)
        {
            inheritableTools ??= [];
            inheritableVerbProperties ??= [];
            inheritableCompProperties ??= [];

            inheritableCompProperties.RemoveAll(x => typeof(CompEquippable).IsAssignableFrom(x.compClass));
        }

        /// <summary>
        /// Tools for protected weapon properties
        /// </summary>
        public List<Tool> inheritableTools = new List<Tool>();

        /// <summary>
        /// VerbProperties for protected weapon properties
        /// </summary>
        public List<VerbProperties> inheritableVerbProperties = new List<VerbProperties>();

        /// <summary>
        /// CompProperties for protected weapon properties
        /// </summary>
        public List<CompProperties> inheritableCompProperties = new List<CompProperties>();

    }
}