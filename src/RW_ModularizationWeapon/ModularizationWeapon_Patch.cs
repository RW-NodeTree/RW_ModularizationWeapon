using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using Verse;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RW_ModularizationWeapon
{
    public partial class ModularizationWeapon
    {
        public static FieldReaderInstList<VerbProperties> VerbPropertiesObjectPatch(string? childNodeIdForVerbProperties, IDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
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
        
        public FieldReaderInstList<VerbProperties> VerbPropertiesObjectPatch(string? childNodeIdForVerbProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return VerbPropertiesObjectPatch(childNodeIdForVerbProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }


        public static FieldReaderInstList<Tool> ToolsObjectPatch(string? childNodeIdForTool, IDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
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

        public FieldReaderInstList<Tool> ToolsObjectPatch(string? childNodeIdForTool)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return ToolsObjectPatch(childNodeIdForTool, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public static FieldReaderInstList<CompProperties> CompPropertiesObjectPatch(string? childNodeIdForCompProperties, IDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
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

        public FieldReaderInstList<CompProperties> CompPropertiesObjectPatch(string? childNodeIdForCompProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return CompPropertiesObjectPatch(childNodeIdForCompProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public static FieldReaderBoolList<VerbProperties> VerbPropertiesBoolAndPatch(string? childNodeIdForVerbProperties, IDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
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

        public FieldReaderBoolList<VerbProperties> VerbPropertiesBoolAndPatch(string? childNodeIdForVerbProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return VerbPropertiesBoolAndPatch(childNodeIdForVerbProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }


        public static FieldReaderBoolList<Tool> ToolsBoolAndPatch(string? childNodeIdForTool, IDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
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

        public FieldReaderBoolList<Tool> ToolsBoolAndPatch(string? childNodeIdForTool)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return ToolsBoolAndPatch(childNodeIdForTool, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }


        public static FieldReaderBoolList<CompProperties> CompPropertiesBoolAndPatch(string? childNodeIdForCompProperties, IDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
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

        public FieldReaderBoolList<CompProperties> CompPropertiesBoolAndPatch(string? childNodeIdForCompProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return CompPropertiesBoolAndPatch(childNodeIdForCompProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }


        public static FieldReaderBoolList<VerbProperties> VerbPropertiesBoolOrPatch(string? childNodeIdForVerbProperties, IDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
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

        public FieldReaderBoolList<VerbProperties> VerbPropertiesBoolOrPatch(string? childNodeIdForVerbProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return VerbPropertiesBoolOrPatch(childNodeIdForVerbProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }


        public static FieldReaderBoolList<Tool> ToolsBoolOrPatch(string? childNodeIdForTool, IDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
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

        public FieldReaderBoolList<Tool> ToolsBoolOrPatch(string? childNodeIdForTool)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return ToolsBoolOrPatch(childNodeIdForTool, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }


        public static FieldReaderBoolList<CompProperties> CompPropertiesBoolOrPatch(string? childNodeIdForCompProperties, IDictionary<string, Thing?> container, ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties)
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


                    results |= weapon.Props.compPropertiesBoolAndPatch & cache;
                    results.DefaultValue = false;

                    results |= CompPropertiesBoolOrPatch(null, weapon.ChildNodes, weapon.GetOrGenCurrentPartAttachmentProperties()) & cache;
                    results.DefaultValue = false;
                }
            }
            return results;
        }

        public FieldReaderBoolList<CompProperties> CompPropertiesBoolOrPatch(string? childNodeIdForCompProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return CompPropertiesBoolOrPatch(childNodeIdForCompProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }
    }
}
