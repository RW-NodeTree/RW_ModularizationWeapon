using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using Verse;
using System;
using System.Collections.Generic;

namespace RW_ModularizationWeapon
{
    public partial class ModularizationWeapon
    {
        public FieldReaderInstList<VerbProperties> VerbPropertiesObjectPatch(string? childNodeIdForVerbProperties)
        {
            NodeContainer? container = ChildNodes;
            if(container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderInstList<VerbProperties> results = new FieldReaderInstList<VerbProperties>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForVerbProperties);
            ModularizationWeapon? currentWeapon = childNodeIdForVerbProperties != null ? container[childNodeIdForVerbProperties] as ModularizationWeapon : null;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
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

                    results |= weapon.VerbPropertiesObjectPatch(null) & cache;
                }
            }
            return results;
        }


        public FieldReaderInstList<Tool> ToolsObjectPatch(string? childNodeIdForTool)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderInstList<Tool> results = new FieldReaderInstList<Tool>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForTool);
            ModularizationWeapon? currentWeapon = childNodeIdForTool != null ? container[childNodeIdForTool] as ModularizationWeapon : null;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
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

                    results |= weapon.ToolsObjectPatch(null) & cache;
                }
            }
            return results;
        }


        public FieldReaderInstList<CompProperties> CompPropertiesObjectPatch(string? childNodeIdForCompProperties)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderInstList<CompProperties> results = new FieldReaderInstList<CompProperties>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForCompProperties);
            ModularizationWeapon? currentWeapon = childNodeIdForCompProperties != null ? container[childNodeIdForCompProperties] as ModularizationWeapon : null;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
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

                    results |= weapon.CompPropertiesObjectPatch(null) & cache;
                }
            }
            return results;
        }


        public FieldReaderBoolList<VerbProperties> VerbPropertiesBoolAndPatch(string? childNodeIdForVerbProperties)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderBoolList<VerbProperties> results = new FieldReaderBoolList<VerbProperties>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForVerbProperties);
            ModularizationWeapon? currentWeapon = childNodeIdForVerbProperties != null ? container[childNodeIdForVerbProperties] as ModularizationWeapon : null;
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
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

                    results &= weapon.VerbPropertiesBoolAndPatch(null) & cache;
                    results.DefaultValue = true;
                }
            }
            return results;
        }


        public FieldReaderBoolList<Tool> ToolsBoolAndPatch(string? childNodeIdForTool)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderBoolList<Tool> results = new FieldReaderBoolList<Tool>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForTool);
            ModularizationWeapon? currentWeapon = childNodeIdForTool != null ? container[childNodeIdForTool] as ModularizationWeapon : null;
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
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

                    results &= weapon.ToolsBoolAndPatch(null) & cache;
                    results.DefaultValue = true;
                }
            }
            return results;
        }


        public FieldReaderBoolList<CompProperties> CompPropertiesBoolAndPatch(string? childNodeIdForCompProperties)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderBoolList<CompProperties> results = new FieldReaderBoolList<CompProperties>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForCompProperties);
            ModularizationWeapon? currentWeapon = childNodeIdForCompProperties != null ? container[childNodeIdForCompProperties] as ModularizationWeapon : null;
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
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

                    results &= weapon.CompPropertiesBoolAndPatch(null) & cache;
                    results.DefaultValue = true;
                }
            }
            return results;
        }


        public FieldReaderBoolList<VerbProperties> VerbPropertiesBoolOrPatch(string? childNodeIdForVerbProperties)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderBoolList<VerbProperties> results = new FieldReaderBoolList<VerbProperties>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForVerbProperties);
            ModularizationWeapon? currentWeapon = childNodeIdForVerbProperties != null ? container[childNodeIdForVerbProperties] as ModularizationWeapon : null;
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
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

                    results |= weapon.VerbPropertiesBoolOrPatch(null) & cache;
                    results.DefaultValue = false;
                }
            }
            return results;
        }


        public FieldReaderBoolList<Tool> ToolsBoolOrPatch(string? childNodeIdForTool)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderBoolList<Tool> results = new FieldReaderBoolList<Tool>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForTool);
            ModularizationWeapon? currentWeapon = childNodeIdForTool != null ? container[childNodeIdForTool] as ModularizationWeapon : null;
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
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

                    results |= weapon.ToolsBoolOrPatch(null) & cache;
                    results.DefaultValue = false;
                }
            }
            return results;
        }


        public FieldReaderBoolList<CompProperties> CompPropertiesBoolOrPatch(string? childNodeIdForCompProperties)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderBoolList<CompProperties> results = new FieldReaderBoolList<CompProperties>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForCompProperties);
            ModularizationWeapon? currentWeapon = childNodeIdForCompProperties != null ? container[childNodeIdForCompProperties] as ModularizationWeapon : null;
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
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

                    results |= weapon.CompPropertiesBoolOrPatch(null) & cache;
                    results.DefaultValue = false;
                }
            }
            return results;
        }
    }
}
