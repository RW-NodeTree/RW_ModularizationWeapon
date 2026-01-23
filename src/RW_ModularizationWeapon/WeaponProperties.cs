
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
    public interface IWeaponPropertiesHolder
    {
        string? SourceChildID { get; }
        ModularizationWeapon Weapon { get; }
        ReadOnlyCollection<Tool> Tools { get; }
        ReadOnlyCollection<VerbProperties> VerbProperties { get; }
        ReadOnlyCollection<CompProperties> CompProperties { get; }

    }
    public class WeaponProperties
    {

        internal WeaponProperties(IWeaponPropertiesHolder holder)
        {
            this.holder = holder;
        }


        public void ExposeData()
        {
            bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
            if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
            try
            {
                for (int i = 0; i < this.comps_maked.Count; i++)
                {
                    this.comps_maked[i].PostExposeData();
                }
            }
            finally
            {
                if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void PostMake()
        {
            bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
            if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
            try
            {
                for (int i = 0; i < this.comps.Count; i++)
                {
                    this.comps[i].PostPostMake();
                }
                MarkAllMaked();
            }
            finally
            {
                if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
            }
        }


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
            VerbProperties original = properties;
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
            properties.isPrimary = original.isPrimary;
            return properties;
        }


        public static Tool ToolAfterAffect(Tool tool, string? childNodeIdForTool, IReadOnlyDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
        {
            //tool = (Tool)tool.SimpleCopy();
            Tool original = tool;
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
            CompProperties original = compProperties;
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
            compProperties.compClass = original.compClass;
            return compProperties;
        }
        
        public ReadOnlyCollection<Tool> VerbToolRegiestInfo
        {
            get
            {
                NodeContainer? container = holder.Weapon.ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(holder.Weapon.ChildNodes));
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (toolsCache == null)
                    {
                        string? id = holder.SourceChildID;
                        ReadOnlyCollection<Tool> tools = holder.Tools;
                        List<Task<Tool>> tasks = new List<Task<Tool>>(tools.Count);
                        ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties = holder.Weapon.GetOrGenCurrentPartAttachmentProperties();
                        foreach (Tool tool in tools)
                        {
                            tasks.Add(Task.Run(() => ToolAfterAffect(tool, id, container, attachmentProperties)));
                        }
                        List<Tool> result = new List<Tool>(tasks.Count);
                        foreach (Task<Tool> task in tasks)
                        {
                            result.Add(task.Result);
                        }
                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            toolsCache = new ReadOnlyCollection<Tool>(result);
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


        public ReadOnlyCollection<VerbProperties> VerbPropertiesRegiestInfo
        {
            get
            {
                NodeContainer? container = holder.Weapon.ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(holder.Weapon.ChildNodes));
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (verbPropertiesCache == null)
                    {
                        string? id = holder.SourceChildID;
                        ReadOnlyCollection<VerbProperties> verbProperties = holder.VerbProperties;
                        List<Task<VerbProperties>> tasks = new List<Task<VerbProperties>>(verbProperties.Count);
                        ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties = holder.Weapon.GetOrGenCurrentPartAttachmentProperties();
                        foreach (VerbProperties verb in verbProperties)
                        {
                            tasks.Add(Task.Run(() => VerbPropertiesAfterAffect(verb, id, container, attachmentProperties)));
                        }
                        List<VerbProperties> result = new List<VerbProperties>(tasks.Count);
                        foreach (Task<VerbProperties> task in tasks)
                        {
                            result.Add(task.Result);
                        }
                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            verbPropertiesCache = new ReadOnlyCollection<VerbProperties>(result);
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

        
        public ReadOnlyCollection<CompProperties> CompPropertiesRegiestInfo
        {
            get
            {
                NodeContainer? container = holder.Weapon.ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(holder.Weapon.ChildNodes));
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (compPropertiesCache == null)
                    {
                        string? id = holder.SourceChildID;
                        ReadOnlyCollection<CompProperties> compProperties = holder.CompProperties;
                        List<Task<CompProperties>> tasks = new List<Task<CompProperties>>(compProperties.Count);
                        ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties = holder.Weapon.GetOrGenCurrentPartAttachmentProperties();
                        foreach (CompProperties comp in compProperties)
                        {
                            tasks.Add(Task.Run(() => CompPropertiesAfterAffect(comp, id, container, attachmentProperties)));
                        }
                        List<CompProperties> result = new List<CompProperties>(tasks.Count);
                        foreach (Task<CompProperties> task in tasks)
                        {
                            result.Add(task.Result);
                        }
                        bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                        if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                        try
                        {
                            compPropertiesCache = new ReadOnlyCollection<CompProperties>(result);
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
            bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
            if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
            try
            {
                foreach(var comp in comps_maked)
                {
                    if(!holder.Weapon.AllComps.Contains(comp))
                    {
                        foreach(var destructor in holder.Weapon.Props.thingCompDestructors)
                        {
                            destructor.DestroyComp(holder.Weapon, comp);
                        }
                    }
                }
                comps_maked.Clear();
            }
            finally
            {
                if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
            }
        }

        internal bool PreInitComps()
        {
            bool result = !readerWriterLockSlim.IsUpgradeableReadLockHeld && !readerWriterLockSlim.IsWriteLockHeld;
            if (result) readerWriterLockSlim.EnterUpgradeableReadLock();
            return result;
        }

        internal void RestoreComps(List<ThingComp> next)
        {
            List<ThingComp> comps_maked = [.. this.comps_maked];
            comps_maked.RemoveAll(x => holder.Weapon.def.comps.FirstIndexOf(y => y == x.props) <  0); //comps_maked = def.comps & comps_maked
            holder.Weapon.def.comps.RemoveAll(x => comps_maked.FirstIndexOf(y => x == y.props) >= 0); //def.comps = def.comps & !comps_maked
            next.AddRange(comps_maked);
        }

        internal void FinalInitComps(List<ThingComp> comps, bool needExitLock)
        {
            try
            {
                bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                try
                {
                    ReadOnlyCollection<CompProperties> compProperties = CompPropertiesRegiestInfo;
                    this.comps.Clear();
                    this.comps.AddRange(comps);
                    this.comps.RemoveAll(x => compProperties.FirstIndexOf(y => x.props == y) < 0);
                    this.comps.RemoveAll(x => holder.Weapon.def.comps.FirstIndexOf(y => x.props == y) < 0); //this.comps = def.comps & compProperties
                }
                finally
                {
                    if(!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                }
            }
            finally
            {
                if (needExitLock) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        internal void MarkAllMaked()
        {
            bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
            if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
            try
            {
                comps_maked.AddRange(comps);
                comps.Clear();
            }
            finally
            {
                if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
            }
        }

        public readonly IWeaponPropertiesHolder holder;
        
        
        private readonly List<ThingComp> comps = new List<ThingComp>();
        private readonly List<ThingComp> comps_maked = new List<ThingComp>();
        private ReadOnlyCollection<Tool>? toolsCache = null;
        private ReadOnlyCollection<VerbProperties>? verbPropertiesCache = null;
        private ReadOnlyCollection<CompProperties>? compPropertiesCache = null;
        private readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();

    }
}