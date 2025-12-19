
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
    public class WeaponProperties
    {

        public WeaponProperties(ModularizationWeapon weapon, string? childId, int index) //index == -1 -> public; id == null -> self
        {
            this.weapon = weapon;
            this.childId = childId;
            this.index = index;
        }


        public void ExposeData()
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                if (this.comps != null)
                {
                    bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                    if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                    try
                    {
                        for (int i = 0; i < this.comps.Count; i++)
                        {
                            this.comps[i].PostExposeData();
                        }
                        compMaked = true;
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

        public void PostMake()
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                if (this.comps != null && !compMaked)
                {
                    bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                    if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                    try
                    {
                        for (int i = 0; i < this.comps.Count; i++)
                        {
                            this.comps[i].PostPostMake();
                        }
                        compMaked = true;
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

        public bool IsVaildity
        {
            get
            {
                NodeContainer? container = ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if(vaildityCache == null)
                    {
                        bool finalVaildity = index < 0;
                        if (!finalVaildity && childId != null)
                        {
                            Thing? thing = container[childId];
                            WeaponAttachmentProperties attachmentProperties = weapon.GetOrGenCurrentPartAttachmentProperties()[childId];
                            if (thing != null)
                            {
                                finalVaildity =
                                    (!ModularizationWeapon.NotUseVerbProperties(thing, attachmentProperties) && ModularizationWeapon.ProtectedVerbPropertiesFromThing(thing, index).Count > 0) ||
                                    (!ModularizationWeapon.NotUseTools(thing, attachmentProperties) && ModularizationWeapon.ProtectedToolsFromThing(thing, index).Count > 0) ||
                                    (!ModularizationWeapon.NotUseCompProperties(thing, attachmentProperties) && ModularizationWeapon.ProtectedCompPropertiesFromThing(thing, index).Count > 0);
                                
                            }
                        }
                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            vaildityCache = finalVaildity;
                        }
                        finally
                        {
                            if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                        }
                    }
                    return vaildityCache.Value;
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }
        

        public string Name
        {
            get
            {
                NodeContainer? container = ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (nameCache == null)
                    {
                        string? finalName = null;
                        if (index < 0)
                        {
                            finalName = weapon.Label;
                        }
                        else if(childId == null)
                        {
                            finalName = weapon.Label + " : NO." + (index + 1);
                        }
                        else
                        {
                            Thing? child = container[childId];
                            ModularizationWeapon? weapon = child as ModularizationWeapon;
                            if (weapon != null)
                            {
                                finalName = weapon.ProtectedProperties[index].Name;
                            }
                            else if(child != null)
                            {
                                finalName = child.Label + " : NO." + (index + 1);
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
                NodeContainer? container = ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (colorCache == null)
                    {
                        Color finalColor = Color.white;
                        if(childId == null)
                        {
                            finalColor = weapon.DrawColor;
                        }
                        else
                        {
                            Thing? child = container[childId];
                            ModularizationWeapon? weapon = child as ModularizationWeapon;
                            if (weapon != null)
                            {
                                finalColor = weapon.ProtectedProperties[index].Color;
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
                NodeContainer? container = ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (iconCache == null)
                    {
                        Texture2D? finalIcon = null;
                        if(childId == null)
                        {
                            finalIcon = (weapon.Graphic?.MatSingle?.mainTexture as Texture2D) ?? weapon.def.uiIcon;
                        }
                        else
                        {
                            Thing? child = container[childId];
                            ModularizationWeapon? weapon = child as ModularizationWeapon;
                            if (weapon != null)
                            {
                                finalIcon = weapon.ProtectedProperties[index].Icon;
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


        public NodeContainer ChildNodes => weapon.ChildNodes;
        
        


        #region Multiplier

        public static FieldReaderDgitList<VerbProperties> VerbPropertiesMultiplier(string? childNodeIdForVerbProperties, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            if (container == null) throw new NullReferenceException(nameof(container));
            if (attachmentProperties == null) throw new NullReferenceException(nameof(attachmentProperties));
            FieldReaderDgitList<VerbProperties> results = new FieldReaderDgitList<VerbProperties>();
            attachmentProperties.TryGetValue(childNodeIdForVerbProperties ?? "", out WeaponAttachmentProperties? current);
            ModularizationWeapon? currentWeapon = childNodeIdForVerbProperties != null ? container[childNodeIdForVerbProperties] as ModularizationWeapon : null;
            results.DefaultValue = 1;
            foreach (var kvp in container)
            {
                string id = kvp.Key;
                ModularizationWeapon? weapon = kvp.Value as ModularizationWeapon;
                attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                if (weapon != null && properties != null && id != childNodeIdForVerbProperties)
                {
                    FieldReaderDgitList<VerbProperties> cache = properties.verbPropertiesMultiplierAffectHorizon;

                    if (current != null)
                    {
                        double defaultValue = cache.DefaultValue;
                        cache *=
                            current.verbPropertiesOtherPartMultiplierAffectHorizon
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderDgitList<VerbProperties> result = new FieldReaderDgitList<VerbProperties>();
                                    result.DefaultValue = current.verbPropertiesOtherPartMultiplierAffectHorizonDefaultValue;
                                    return result;
                                }
                            );
                        defaultValue *= current.verbPropertiesOtherPartMultiplierAffectHorizonDefaultValue;
                        cache.DefaultValue = defaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache *= currentWeapon.Props.verbPropertiesOtherPartMultiplierAffectHorizon;
                            cache.DefaultValue = defaultValue * currentWeapon.Props.verbPropertiesOtherPartMultiplierAffectHorizon.DefaultValue;
                        }
                    }

                    FieldReaderDgitList<VerbProperties> mul = weapon.Props.verbPropertiesMultiplier - 1f;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    mul = (mul * cache + 1) ?? mul;
                    mul.DefaultValue = 1;
                    results *= mul;
                    results.DefaultValue = 1;

                    mul = VerbPropertiesMultiplier(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) - 1;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    mul = (mul * cache + 1) ?? mul;
                    mul.DefaultValue = 1;
                    results *= mul;
                    results.DefaultValue = 1;

                    //result *= (comp.Props.verbPropertiesMultiplier - 1f) * properties.verbPropertiesMultiplierAffectHorizon + 1f;
                }
            }
            //Log.Message($" Final {this}.VerbPropertiesMultiplier({childNodeIdForVerbProperties}) :\nresults : {results}");
            return results;
        }


        public static FieldReaderDgitList<Tool> ToolsMultiplier(string? childNodeIdForTool, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            if (container == null) throw new NullReferenceException(nameof(container));
            if (attachmentProperties == null) throw new NullReferenceException(nameof(attachmentProperties));
            FieldReaderDgitList<Tool> results = new FieldReaderDgitList<Tool>();
            attachmentProperties.TryGetValue(childNodeIdForTool ?? "", out WeaponAttachmentProperties? current);
            ModularizationWeapon? currentWeapon = childNodeIdForTool != null ? container[childNodeIdForTool] as ModularizationWeapon : null;
            results.DefaultValue = 1;
            foreach (var kvp in container)
            {
                string id = kvp.Key;
                ModularizationWeapon? weapon = kvp.Value as ModularizationWeapon;
                attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                if (weapon != null && properties != null && id != childNodeIdForTool)
                {
                    FieldReaderDgitList<Tool> cache = properties.toolsMultiplierAffectHorizon;

                    if (current != null)
                    {
                        double defaultValue = cache.DefaultValue;
                        cache *=
                            current.toolsOtherPartMultiplierAffectHorizon
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderDgitList<Tool> result = new FieldReaderDgitList<Tool>();
                                    result.DefaultValue = current.toolsOtherPartMultiplierAffectHorizonDefaultValue;
                                    return result;
                                }
                            );
                        defaultValue *= current.toolsOtherPartMultiplierAffectHorizonDefaultValue;
                        cache.DefaultValue = defaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache *= currentWeapon.Props.toolsOtherPartMultiplierAffectHorizon;
                            cache.DefaultValue = defaultValue * currentWeapon.Props.toolsOtherPartMultiplierAffectHorizon.DefaultValue;
                        }
                    }

                    FieldReaderDgitList<Tool> mul = weapon.Props.toolsMultiplier - 1f;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    mul = (mul * cache + 1) ?? mul;
                    mul.DefaultValue = 1;
                    results *= mul;
                    results.DefaultValue = 1;

                    mul = ToolsMultiplier(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) - 1;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    mul = (mul * cache + 1) ?? mul;
                    mul.DefaultValue = 1;
                    results *= mul;
                    results.DefaultValue = 1;
                }
            }
            //Log.Message($"{container}.ToolsMultiplier({childNodeIdForTool}) :\nresults : {results}");
            return results;
        }


        public static FieldReaderDgitList<CompProperties> CompPropertiesMultiplier(string? childNodeIdForCompProperties, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            if (container == null) throw new NullReferenceException(nameof(container));
            if (attachmentProperties == null) throw new NullReferenceException(nameof(attachmentProperties));
            FieldReaderDgitList<CompProperties> results = new FieldReaderDgitList<CompProperties>();
            attachmentProperties.TryGetValue(childNodeIdForCompProperties ?? "", out WeaponAttachmentProperties? current);
            ModularizationWeapon? currentWeapon = childNodeIdForCompProperties != null ? container[childNodeIdForCompProperties] as ModularizationWeapon : null;
            results.DefaultValue = 1;
            foreach (var kvp in container)
            {
                string id = kvp.Key;
                ModularizationWeapon? weapon = kvp.Value as ModularizationWeapon;
                attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                if (weapon != null && properties != null && id != childNodeIdForCompProperties)
                {
                    FieldReaderDgitList<CompProperties> cache = properties.compPropertiesMultiplierAffectHorizon;

                    if (current != null)
                    {
                        double defaultValue = cache.DefaultValue;
                        cache *=
                            current.compPropertiesOtherPartMultiplierAffectHorizon
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderDgitList<CompProperties> result = new FieldReaderDgitList<CompProperties>();
                                    result.DefaultValue = current.compPropertiesOtherPartMultiplierAffectHorizonDefaultValue;
                                    return result;
                                }
                            );
                        defaultValue *= current.compPropertiesOtherPartMultiplierAffectHorizonDefaultValue;
                        cache.DefaultValue = defaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache *= currentWeapon.Props.compPropertiesOtherPartMultiplierAffectHorizon;
                            cache.DefaultValue = defaultValue * currentWeapon.Props.compPropertiesOtherPartMultiplierAffectHorizon.DefaultValue;
                        }
                    }

                    FieldReaderDgitList<CompProperties> mul = weapon.Props.compPropertiesMultiplier - 1f;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    mul = (mul * cache + 1) ?? mul;
                    mul.DefaultValue = 1;
                    results *= mul;
                    results.DefaultValue = 1;

                    mul = CompPropertiesMultiplier(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) - 1;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    mul = (mul * cache + 1) ?? mul;
                    mul.DefaultValue = 1;
                    results *= mul;
                    results.DefaultValue = 1;
                }
            }
            //Log.Message($"{container}.CompPropertiesMultiplier({childNodeIdForCompProperties}) :\nresults : {results}");
            return results;
        }
        #endregion

        #region Offseter
        
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

        #endregion

        #region Pather
        
        public static FieldReaderInstList<VerbProperties> VerbPropertiesObjectPatch(string? childNodeIdForVerbProperties, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            if(container == null) throw new NullReferenceException(nameof(container));
            if(attachmentProperties == null) throw new NullReferenceException(nameof(attachmentProperties));
            FieldReaderInstList<VerbProperties> results = new FieldReaderInstList<VerbProperties>();
            attachmentProperties.TryGetValue(childNodeIdForVerbProperties ?? "", out WeaponAttachmentProperties? current);
            ModularizationWeapon? currentWeapon = childNodeIdForVerbProperties != null ? container[childNodeIdForVerbProperties] as ModularizationWeapon : null;
            foreach (var kvp in container)
            {
                string id = kvp.Key;
                ModularizationWeapon? weapon = kvp.Value as ModularizationWeapon;
                attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                if (weapon != null && id != childNodeIdForVerbProperties && properties != null)
                {
                    FieldReaderFiltList<VerbProperties> cache = properties.verbPropertiesObjectPatchByChildPart;
                    if(current != null)
                    {
                        bool defaultValue = cache.DefaultValue;
                        cache &=
                            current.verbPropertiesObjectPatchByOtherPart
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderFiltList<VerbProperties> result = new FieldReaderFiltList<VerbProperties>();
                                    result.DefaultValue = current.verbPropertiesObjectPatchByOtherPartDefaultValue;
                                    return result;
                                }
                            );
                        cache.DefaultValue = defaultValue && current.verbPropertiesObjectPatchByOtherPartDefaultValue;
                        defaultValue = cache.DefaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache &= currentWeapon.Props.verbPropertiesObjectPatchByOtherPart;
                            cache.DefaultValue = defaultValue && currentWeapon.Props.verbPropertiesObjectPatchByOtherPart.DefaultValue;
                        }
                    }


                    results |= weapon.Props.verbPropertiesObjectPatch & cache;

                    results |= VerbPropertiesObjectPatch(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) & cache;
                }
            }
            return results;
        }


        public static FieldReaderInstList<Tool> ToolsObjectPatch(string? childNodeIdForTool, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            if (container == null) throw new NullReferenceException(nameof(container));
            if (attachmentProperties == null) throw new NullReferenceException(nameof(attachmentProperties));
            FieldReaderInstList<Tool> results = new FieldReaderInstList<Tool>();
            attachmentProperties.TryGetValue(childNodeIdForTool ?? "", out WeaponAttachmentProperties? current);
            ModularizationWeapon? currentWeapon = childNodeIdForTool != null ? container[childNodeIdForTool] as ModularizationWeapon : null;
            foreach (var kvp in container)
            {
                string id = kvp.Key;
                ModularizationWeapon? weapon = kvp.Value as ModularizationWeapon;
                attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                if (weapon != null && id != childNodeIdForTool && properties != null)
                {
                    FieldReaderFiltList<Tool> cache = properties.toolsObjectPatchByChildPart;
                    if (current != null)
                    {
                        bool defaultValue = cache.DefaultValue;
                        cache &=
                            current.toolsObjectPatchByOtherPart
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderFiltList<Tool> result = new FieldReaderFiltList<Tool>();
                                    result.DefaultValue = current.toolsObjectPatchByOtherPartDefaultValue;
                                    return result;
                                }
                            );
                        cache.DefaultValue = defaultValue && current.toolsObjectPatchByOtherPartDefaultValue;
                        defaultValue = cache.DefaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache &= currentWeapon.Props.toolsObjectPatchByOtherPart;
                            cache.DefaultValue = defaultValue && currentWeapon.Props.toolsObjectPatchByOtherPart.DefaultValue;
                        }
                    }


                    results |= weapon.Props.toolsObjectPatch & cache;

                    results |= ToolsObjectPatch(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) & cache;
                }
            }
            return results;
        }

        public static FieldReaderInstList<CompProperties> CompPropertiesObjectPatch(string? childNodeIdForCompProperties, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            if (container == null) throw new NullReferenceException(nameof(container));
            if (attachmentProperties == null) throw new NullReferenceException(nameof(attachmentProperties));
            FieldReaderInstList<CompProperties> results = new FieldReaderInstList<CompProperties>();
            attachmentProperties.TryGetValue(childNodeIdForCompProperties ?? "", out WeaponAttachmentProperties? current);
            ModularizationWeapon? currentWeapon = childNodeIdForCompProperties != null ? container[childNodeIdForCompProperties] as ModularizationWeapon : null;
            foreach (var kvp in container)
            {
                string id = kvp.Key;
                ModularizationWeapon? weapon = kvp.Value as ModularizationWeapon;
                attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                if (weapon != null && id != childNodeIdForCompProperties && properties != null)
                {
                    FieldReaderFiltList<CompProperties> cache = properties.compPropertiesObjectPatchByChildPart;
                    if (current != null)
                    {
                        bool defaultValue = cache.DefaultValue;
                        cache &=
                            current.compPropertiesObjectPatchByOtherPart
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderFiltList<CompProperties> result = new FieldReaderFiltList<CompProperties>();
                                    result.DefaultValue = current.compPropertiesObjectPatchByOtherPartDefaultValue;
                                    return result;
                                }
                            );
                        cache.DefaultValue = defaultValue && current.compPropertiesObjectPatchByOtherPartDefaultValue;
                        defaultValue = cache.DefaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache &= currentWeapon.Props.compPropertiesObjectPatchByOtherPart;
                            cache.DefaultValue = defaultValue && currentWeapon.Props.compPropertiesObjectPatchByOtherPart.DefaultValue;
                        }
                    }


                    results |= weapon.Props.compPropertiesObjectPatch & cache;

                    results |= CompPropertiesObjectPatch(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) & cache;
                }
            }
            return results;
        }

        public static FieldReaderBoolList<VerbProperties> VerbPropertiesBoolAndPatch(string? childNodeIdForVerbProperties, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            if (container == null) throw new NullReferenceException(nameof(container));
            if (attachmentProperties == null) throw new NullReferenceException(nameof(attachmentProperties));
            FieldReaderBoolList<VerbProperties> results = new FieldReaderBoolList<VerbProperties>();
            attachmentProperties.TryGetValue(childNodeIdForVerbProperties ?? "", out WeaponAttachmentProperties? current);
            ModularizationWeapon? currentWeapon = childNodeIdForVerbProperties != null ? container[childNodeIdForVerbProperties] as ModularizationWeapon : null;
            results.DefaultValue = true;
            foreach (var kvp in container)
            {
                string id = kvp.Key;
                ModularizationWeapon? weapon = kvp.Value as ModularizationWeapon;
                attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                if (weapon != null && id != childNodeIdForVerbProperties && properties != null)
                {
                    FieldReaderBoolList<VerbProperties> cache = properties.verbPropertiesBoolAndPatchByChildPart;
                    if (current != null)
                    {
                        bool defaultValue = cache.DefaultValue;
                        cache &=
                            current.verbPropertiesBoolAndPatchByOtherPart
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderBoolList<VerbProperties> result = new FieldReaderBoolList<VerbProperties>();
                                    result.DefaultValue = current.verbPropertiesBoolAndPatchByOtherPartDefaultValue;
                                    return result;
                                }
                            );
                        cache.DefaultValue = defaultValue && current.verbPropertiesBoolAndPatchByOtherPartDefaultValue;
                        defaultValue = cache.DefaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache &= currentWeapon.Props.verbPropertiesBoolAndPatchByOtherPart;
                            cache.DefaultValue = defaultValue && currentWeapon.Props.verbPropertiesBoolAndPatchByOtherPart.DefaultValue;
                        }
                    }


                    results &= weapon.Props.verbPropertiesBoolAndPatch & cache;
                    results.DefaultValue = true;

                    results &= VerbPropertiesBoolAndPatch(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) & cache;
                    results.DefaultValue = true;
                }
            }
            return results;
        }


        public static FieldReaderBoolList<Tool> ToolsBoolAndPatch(string? childNodeIdForTool, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            if (container == null) throw new NullReferenceException(nameof(container));
            if (attachmentProperties == null) throw new NullReferenceException(nameof(attachmentProperties));
            FieldReaderBoolList<Tool> results = new FieldReaderBoolList<Tool>();
            attachmentProperties.TryGetValue(childNodeIdForTool ?? "", out WeaponAttachmentProperties? current);
            ModularizationWeapon? currentWeapon = childNodeIdForTool != null ? container[childNodeIdForTool] as ModularizationWeapon : null;
            results.DefaultValue = true;
            foreach (var kvp in container)
            {
                string id = kvp.Key;
                ModularizationWeapon? weapon = kvp.Value as ModularizationWeapon;
                attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                if (weapon != null && id != childNodeIdForTool && properties != null)
                {
                    FieldReaderBoolList<Tool> cache = properties.toolsBoolAndPatchByChildPart;
                    if (current != null)
                    {
                        bool defaultValue = cache.DefaultValue;
                        cache &=
                            current.toolsBoolAndPatchByOtherPart
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderBoolList<Tool> result = new FieldReaderBoolList<Tool>();
                                    result.DefaultValue = current.toolsBoolAndPatchByOtherPartDefaultValue;
                                    return result;
                                }
                            );
                        cache.DefaultValue = defaultValue && current.toolsBoolAndPatchByOtherPartDefaultValue;
                        defaultValue = cache.DefaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache &= currentWeapon.Props.toolsBoolAndPatchByOtherPart;
                            cache.DefaultValue = defaultValue && currentWeapon.Props.toolsBoolAndPatchByOtherPart.DefaultValue;
                        }
                    }


                    results &= weapon.Props.toolsBoolAndPatch & cache;
                    results.DefaultValue = true;

                    results &= ToolsBoolAndPatch(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) & cache;
                    results.DefaultValue = true;
                }
            }
            return results;
        }


        public static FieldReaderBoolList<CompProperties> CompPropertiesBoolAndPatch(string? childNodeIdForCompProperties, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            if (container == null) throw new NullReferenceException(nameof(container));
            if (attachmentProperties == null) throw new NullReferenceException(nameof(attachmentProperties));
            FieldReaderBoolList<CompProperties> results = new FieldReaderBoolList<CompProperties>();
            attachmentProperties.TryGetValue(childNodeIdForCompProperties ?? "", out WeaponAttachmentProperties? current);
            ModularizationWeapon? currentWeapon = childNodeIdForCompProperties != null ? container[childNodeIdForCompProperties] as ModularizationWeapon : null;
            results.DefaultValue = true;
            foreach (var kvp in container)
            {
                string id = kvp.Key;
                ModularizationWeapon? weapon = kvp.Value as ModularizationWeapon;
                attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                if (weapon != null && id != childNodeIdForCompProperties && properties != null)
                {
                    FieldReaderBoolList<CompProperties> cache = properties.compPropertiesBoolAndPatchByChildPart;
                    if (current != null)
                    {
                        bool defaultValue = cache.DefaultValue;
                        cache &=
                            current.compPropertiesBoolAndPatchByOtherPart
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderBoolList<CompProperties> result = new FieldReaderBoolList<CompProperties>();
                                    result.DefaultValue = current.compPropertiesBoolAndPatchByOtherPartDefaultValue;
                                    return result;
                                }
                            );
                        cache.DefaultValue = defaultValue && current.compPropertiesBoolAndPatchByOtherPartDefaultValue;
                        defaultValue = cache.DefaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache &= currentWeapon.Props.compPropertiesBoolAndPatchByOtherPart;
                            cache.DefaultValue = defaultValue && currentWeapon.Props.compPropertiesBoolAndPatchByOtherPart.DefaultValue;
                        }
                    }


                    results &= weapon.Props.compPropertiesBoolAndPatch & cache;
                    results.DefaultValue = true;

                    results &= CompPropertiesBoolAndPatch(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) & cache;
                    results.DefaultValue = true;
                }
            }
            return results;
        }


        public static FieldReaderBoolList<VerbProperties> VerbPropertiesBoolOrPatch(string? childNodeIdForVerbProperties, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            if (container == null) throw new NullReferenceException(nameof(container));
            if (attachmentProperties == null) throw new NullReferenceException(nameof(attachmentProperties));
            FieldReaderBoolList<VerbProperties> results = new FieldReaderBoolList<VerbProperties>();
            attachmentProperties.TryGetValue(childNodeIdForVerbProperties ?? "", out WeaponAttachmentProperties? current);
            ModularizationWeapon? currentWeapon = childNodeIdForVerbProperties != null ? container[childNodeIdForVerbProperties] as ModularizationWeapon : null;
            results.DefaultValue = true;
            foreach (var kvp in container)
            {
                string id = kvp.Key;
                ModularizationWeapon? weapon = kvp.Value as ModularizationWeapon;
                attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                if (weapon != null && id != childNodeIdForVerbProperties && properties != null)
                {
                    FieldReaderBoolList<VerbProperties> cache = properties.verbPropertiesBoolOrPatchByChildPart;
                    if (current != null)
                    {
                        bool defaultValue = cache.DefaultValue;
                        cache &=
                            current.verbPropertiesBoolOrPatchByOtherPart
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderBoolList<VerbProperties> result = new FieldReaderBoolList<VerbProperties>();
                                    result.DefaultValue = current.verbPropertiesBoolOrPatchByOtherPartDefaultValue;
                                    return result;
                                }
                            );
                        cache.DefaultValue = defaultValue && current.verbPropertiesBoolOrPatchByOtherPartDefaultValue;
                        defaultValue = cache.DefaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache &= currentWeapon.Props.verbPropertiesBoolOrPatchByOtherPart;
                            cache.DefaultValue = defaultValue && currentWeapon.Props.verbPropertiesBoolOrPatchByOtherPart.DefaultValue;
                        }
                    }


                    results |= weapon.Props.verbPropertiesBoolOrPatch & cache;
                    results.DefaultValue = false;

                    results |= VerbPropertiesBoolOrPatch(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) & cache;
                    results.DefaultValue = false;
                }
            }
            return results;
        }


        public static FieldReaderBoolList<Tool> ToolsBoolOrPatch(string? childNodeIdForTool, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            if (container == null) throw new NullReferenceException(nameof(container));
            if (attachmentProperties == null) throw new NullReferenceException(nameof(attachmentProperties));
            FieldReaderBoolList<Tool> results = new FieldReaderBoolList<Tool>();
            attachmentProperties.TryGetValue(childNodeIdForTool ?? "", out WeaponAttachmentProperties? current);
            ModularizationWeapon? currentWeapon = childNodeIdForTool != null ? container[childNodeIdForTool] as ModularizationWeapon : null;
            results.DefaultValue = true;
            foreach (var kvp in container)
            {
                string id = kvp.Key;
                ModularizationWeapon? weapon = kvp.Value as ModularizationWeapon;
                attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                if (weapon != null && id != childNodeIdForTool && properties != null)
                {
                    FieldReaderBoolList<Tool> cache = properties.toolsBoolOrPatchByChildPart;
                    if (current != null)
                    {
                        bool defaultValue = cache.DefaultValue;
                        cache &=
                            current.toolsBoolOrPatchByOtherPart
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderBoolList<Tool> result = new FieldReaderBoolList<Tool>();
                                    result.DefaultValue = current.toolsBoolOrPatchByOtherPartDefaultValue;
                                    return result;
                                }
                            );
                        cache.DefaultValue = defaultValue && current.toolsBoolOrPatchByOtherPartDefaultValue;
                        defaultValue = cache.DefaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache &= currentWeapon.Props.toolsBoolOrPatchByOtherPart;
                            cache.DefaultValue = defaultValue && currentWeapon.Props.toolsBoolOrPatchByOtherPart.DefaultValue;
                        }
                    }


                    results |= weapon.Props.toolsBoolOrPatch & cache;
                    results.DefaultValue = false;

                    results |= ToolsBoolOrPatch(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) & cache;
                    results.DefaultValue = false;
                }
            }
            return results;
        }


        public static FieldReaderBoolList<CompProperties> CompPropertiesBoolOrPatch(string? childNodeIdForCompProperties, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            if (container == null) throw new NullReferenceException(nameof(container));
            if (attachmentProperties == null) throw new NullReferenceException(nameof(attachmentProperties));
            FieldReaderBoolList<CompProperties> results = new FieldReaderBoolList<CompProperties>();
            attachmentProperties.TryGetValue(childNodeIdForCompProperties ?? "", out WeaponAttachmentProperties? current);
            ModularizationWeapon? currentWeapon = childNodeIdForCompProperties != null ? container[childNodeIdForCompProperties] as ModularizationWeapon : null;
            results.DefaultValue = true;
            foreach (var kvp in container)
            {
                string id = kvp.Key;
                ModularizationWeapon? weapon = kvp.Value as ModularizationWeapon;
                attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
                if (weapon != null && id != childNodeIdForCompProperties && properties != null)
                {
                    FieldReaderBoolList<CompProperties> cache = properties.compPropertiesBoolOrPatchByChildPart;
                    if (current != null)
                    {
                        bool defaultValue = cache.DefaultValue;
                        cache &=
                            current.compPropertiesBoolOrPatchByOtherPart
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderBoolList<CompProperties> result = new FieldReaderBoolList<CompProperties>();
                                    result.DefaultValue = current.compPropertiesBoolOrPatchByOtherPartDefaultValue;
                                    return result;
                                }
                            );
                        cache.DefaultValue = defaultValue && current.compPropertiesBoolOrPatchByOtherPartDefaultValue;
                        defaultValue = cache.DefaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache &= currentWeapon.Props.compPropertiesBoolOrPatchByOtherPart;
                            cache.DefaultValue = defaultValue && currentWeapon.Props.compPropertiesBoolOrPatchByOtherPart.DefaultValue;
                        }
                    }


                    results |= weapon.Props.compPropertiesBoolOrPatch & cache;
                    results.DefaultValue = false;

                    results |= CompPropertiesBoolOrPatch(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) & cache;
                    results.DefaultValue = false;
                }
            }
            return results;
        }
        #endregion
        
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


        public ReadOnlyCollection<(string? id, uint index, VerbProperties afterConvert)> VerbPropertiesRegiestInfo
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

                        ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties = weapon.GetOrGenCurrentPartAttachmentProperties();
                        List<(string? id, uint index, Task<VerbProperties> afterConvert)> tasks = new List<(string? id, uint index, Task<VerbProperties> afterConvert)>();
                        if (index < 0)
                        {
                            if (!weapon.def.Verbs.NullOrEmpty())
                            {

                                //result.Capacity += parent.def.Verbs.Count;
                                tasks.Capacity += weapon.def.Verbs.Count;
                                for (uint i = 0; i < weapon.def.Verbs.Count; i++)
                                {
                                    VerbProperties properties = weapon.def.Verbs[(int)i];
                                    tasks.Add((null, i, Task.Run(() => VerbPropertiesAfterAffect(properties, null, container, attachmentProperties))));
                                    //VerbPropertiesRegiestInfo prop = ;
                                    //result.Add(prop);
                                }
                            }

                            foreach (var kv in container)
                            {
                                if (!ModularizationWeapon.NotUseVerbProperties(kv.Item2, attachmentProperties[kv.Item1]))
                                {
                                    List<VerbProperties> verbProperties = ModularizationWeapon.PublicVerbPropertiesFromThing(kv.Item2);
                                    tasks.Capacity += verbProperties.Count;
                                    for (uint i = 0; i < verbProperties.Count; i++)
                                    {
                                        VerbProperties properties = verbProperties[(int)i];
                                        tasks.Add((kv.Item1, i, Task.Run(() => VerbPropertiesAfterAffect(properties, kv.Item1, container, attachmentProperties))));
                                        //result.Add();
                                    }
                                }
                            }
                        }
                        else if(childId == null)
                        {
                            uint i = 0;
                            VerbProperties properties;
                            List<VerbProperties> verbProperties = weapon.Props.protectedVerbProperties;
                            if (!verbProperties.NullOrEmpty())
                            {
                                //result.Capacity += parent.def.Verbs.Count;
                                tasks.Capacity += verbProperties.Count;
                                for (; i < verbProperties.Count; i++)
                                {
                                    properties = verbProperties[(int)i];
                                    tasks.Add((null, i, Task.Run(() => VerbPropertiesAfterAffect(properties, null, container, attachmentProperties))));
                                    //VerbPropertiesRegiestInfo prop = ;
                                    //result.Add(prop);
                                }
                            }
                            
                            if (weapon.Props.allPrimaryVerbProperties.Count > index)
                            {
                                properties = weapon.Props.allPrimaryVerbProperties[index];
                                tasks.Add((null, i, Task.Run(() => VerbPropertiesAfterAffect(properties, null, container, attachmentProperties))));
                            }
                        }
                        else 
                        {
                            Thing? thing = container[childId];
                            if (thing != null && !ModularizationWeapon.NotUseVerbProperties(thing, attachmentProperties[childId]))
                            {
                                List<VerbProperties> verbProperties = ModularizationWeapon.ProtectedVerbPropertiesFromThing(thing, index);
                                tasks.Capacity += verbProperties.Count;
                                for (uint i = 0; i < verbProperties.Count; i++)
                                {
                                    VerbProperties properties = verbProperties[(int)i];
                                    tasks.Add((childId, i, Task.Run(() => VerbPropertiesAfterAffect(properties, childId, container, attachmentProperties))));
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
                        List<(string? id, uint index, VerbProperties afterConvert)> result = new List<(string? id, uint index, VerbProperties afterConvert)>(tasks.Count);
                        foreach (var info in tasks) result.Add((info.id, info.index, info.afterConvert.Result));
                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            verbPropertiesCache = new ReadOnlyCollection<(string? id, uint index, VerbProperties afterConvert)>(result);
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
        
        public ReadOnlyCollection<(string? id, uint index, Tool afterConvert)> VerbToolRegiestInfo
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

                        ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties = weapon.GetOrGenCurrentPartAttachmentProperties();
                        List<(string? id, uint index, Task<Tool> afterConvert)> tasks = new List<(string? id, uint index, Task<Tool> afterConvert)>();
                        
                        if (index < 0)
                        {
                            if (!weapon.def.tools.NullOrEmpty())
                            {
                                tasks.Capacity += weapon.def.tools.Count;
                                for (uint i = 0; i < weapon.def.tools.Count; i++)
                                {
                                    Tool tool = weapon.def.tools[(int)i];
                                    tasks.Add((null, i, Task.Run(() => ToolAfterAffect(tool, null, container, attachmentProperties))));
                                    //VerbToolRegiestInfo prop = ;
                                    //result.Add(prop);
                                }
                            }
                            foreach (var kv in container)
                            {
                                if (!ModularizationWeapon.NotUseTools(kv.Item2, attachmentProperties[kv.Item1]))
                                {
                                    List<Tool> tools = ModularizationWeapon.PublicToolsFromThing(kv.Item2);
                                    tasks.Capacity += tools.Count;
                                    for (uint i = 0; i < tools.Count; i++)
                                    {
                                        Tool tool = tools[(int)i];
                                        tasks.Add((kv.Item1, i, Task.Run(() => ToolAfterAffect(tool, kv.Item1, container, attachmentProperties))));
                                        //Tool newProp
                                        //    = ;
                                        //result.Add();
                                    }
                                }
                            }
                        }
                        else if(childId == null)
                        {
                            uint i = 0;
                            Tool tool;
                            List<Tool> tools = weapon.Props.protectedTools;
                            if (!tools.NullOrEmpty())
                            {
                                //result.Capacity += parent.def.Verbs.Count;
                                tasks.Capacity += tools.Count;
                                for (; i < tools.Count; i++)
                                {
                                    tool = tools[(int)i];
                                    tasks.Add((null, i, Task.Run(() => ToolAfterAffect(tool, null, container, attachmentProperties))));
                                    //ToolRegiestInfo prop = ;
                                    //result.Add(prop);
                                }
                            }
                        }
                        else 
                        {
                            Thing? thing = container[childId];
                            if (thing != null && !ModularizationWeapon.NotUseTools(thing, attachmentProperties[childId]))
                            {
                                List<Tool> tools = ModularizationWeapon.ProtectedToolsFromThing(thing, index);
                                tasks.Capacity += tools.Count;
                                for (uint i = 0; i < tools.Count; i++)
                                {
                                    Tool tool = tools[(int)i];
                                    tasks.Add((childId, i, Task.Run(() => ToolAfterAffect(tool, childId, container, attachmentProperties))));
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
                        List<(string? id, uint index, Tool afterConvert)> result = new List<(string? id, uint index, Tool afterConvert)>(tasks.Count);
                        foreach (var info in tasks) result.Add((info.id, info.index, info.afterConvert.Result));
                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            toolsCache = new ReadOnlyCollection<(string? id, uint index, Tool afterConvert)>(result);
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

        
        public ReadOnlyCollection<(string? id, uint index, CompProperties afterConvert)> CompPropertiesRegiestInfo
        {
            get
            {
                NodeContainer? container = ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (compPropertiesCache == null)
                    {

                        ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties = weapon.GetOrGenCurrentPartAttachmentProperties();
                        List<(string? id, uint index, Task<CompProperties> afterConvert)> tasks = new List<(string? id, uint index, Task<CompProperties> afterConvert)>();
                        if(index < 0)
                        {
                            if (!weapon.def.comps.NullOrEmpty())
                            {
                                tasks.Capacity += weapon.def.comps.Count;
                                for (uint i = 0; i < weapon.def.comps.Count; i++)
                                {
                                    CompProperties comp = weapon.def.comps[(int)i];
                                    tasks.Add((null, i, Task.Run(() => CompPropertiesAfterAffect(comp, null, container, attachmentProperties))));
                                    //VerbToolRegiestInfo prop = ;
                                    //result.Add(prop);
                                }
                            }
                            foreach (var kv in container)
                            {
                                if (!ModularizationWeapon.NotUseCompProperties(kv.Item2, attachmentProperties[kv.Item1]))
                                {
                                    List<CompProperties> comps = ModularizationWeapon.PublicCompPropertiesFromThing(kv.Item2);
                                    tasks.Capacity += comps.Count;
                                    for (uint i = 0; i < comps.Count; i++)
                                    {
                                        CompProperties comp = comps[(int)i];
                                        tasks.Add((kv.Item1, i, Task.Run(() => CompPropertiesAfterAffect(comp, kv.Item1, container, attachmentProperties))));
                                        //Tool newProp
                                        //    = ;
                                        //result.Add();
                                    }
                                }
                            }
                        }
                        else if(childId == null)
                        {
                            uint i = 0;
                            CompProperties comp;
                            List<CompProperties> comps = weapon.Props.protectedCompProperties;
                            if (!comps.NullOrEmpty())
                            {
                                //result.Capacity += parent.def.Verbs.Count;
                                tasks.Capacity += comps.Count;
                                for (; i < comps.Count; i++)
                                {
                                    comp = comps[(int)i];
                                    tasks.Add((null, i, Task.Run(() => CompPropertiesAfterAffect(comp, null, container, attachmentProperties))));
                                    //ToolRegiestInfo prop = ;
                                    //result.Add(prop);
                                }
                            }
                        }
                        else 
                        {
                            Thing? thing = container[childId];
                            if (thing != null && !ModularizationWeapon.NotUseCompProperties(thing, attachmentProperties[childId]))
                            {
                                List<CompProperties> comps = ModularizationWeapon.ProtectedCompPropertiesFromThing(thing, index);
                                tasks.Capacity += comps.Count;
                                for (uint i = 0; i < comps.Count; i++)
                                {
                                    CompProperties comp = comps[(int)i];
                                    tasks.Add((childId, i, Task.Run(() => CompPropertiesAfterAffect(comp, childId, container, attachmentProperties))));
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
                        List<(string? id, uint index, CompProperties afterConvert)> result = new List<(string? id, uint index, CompProperties afterConvert)>(tasks.Count);
                        foreach (var info in tasks) result.Add((info.id, info.index, info.afterConvert.Result));
                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            compPropertiesCache = new ReadOnlyCollection<(string? id, uint index, CompProperties afterConvert)>(result);
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
        

        internal void DestroyComps()
        {
            if(comps != null)
            {
                foreach(var destructor in weapon.Props.thingCompDestructors)
                {
                    foreach(var comp in comps)
                    {
                        destructor.DestroyComp(weapon, comp);
                    }
                }
            }
        }

        internal bool PreInitComps()
        {
            bool result = !readerWriterLockSlim.IsUpgradeableReadLockHeld && !readerWriterLockSlim.IsWriteLockHeld;
            if (result) readerWriterLockSlim.EnterUpgradeableReadLock();
            return result;
        }

        internal void RestoreComps(List<ThingComp> target)
        {
            if(comps != null)
            {
                weapon.def.comps.RemoveAll(x => comps.Find(y => y.props == x) != null);
                target.AddRange(comps);
            }
        }

        internal void FinalInitComps(bool needExitLock)
        {
            try
            {
                if(comps == null)
                {
                    bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                    if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                    try
                    {
                        ReadOnlyCollection<(string? id, uint index, CompProperties afterConvert)> comps = CompPropertiesRegiestInfo;
                        this.comps = [.. weapon.AllComps];
                        if (index < 0 && weapon.ProtectedProperties.Count != 0)
                        {
                            this.comps.RemoveAll(x => comps.FirstIndexOf(y => x.props == y.afterConvert) < 0);
                        }
                        else
                        {
                            this.comps.RemoveAll(x => x is not CompEquippable && comps.FirstIndexOf(y => x.props == y.afterConvert) < 0);
                        }
                        if (weapon.making)
                        {
                            compMaked = true;
                        }
                    }
                    finally
                    {
                        if(!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                    }
                }
            }
            finally
            {
                if (needExitLock) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }
        
        public readonly int index;
        public readonly string? childId;
        public readonly ModularizationWeapon weapon;
        
        private bool compMaked = false;
        private bool? vaildityCache = null;
        private string? nameCache = null;
        private Color? colorCache = null;
        private Texture2D? iconCache = null;
        private List<ThingComp>? comps = null;
        private ReadOnlyCollection<(string? id, uint index, CompProperties afterConvert)>? compPropertiesCache = null;
        private ReadOnlyCollection<(string? id, uint index, Tool afterConvert)>? toolsCache = null;
        private ReadOnlyCollection<(string? id, uint index, VerbProperties afterConvert)>? verbPropertiesCache = null;
        private readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();

    }
}