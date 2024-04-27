using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        public FieldReaderInstList<VerbProperties> VerbPropertiesObjectPatch(string childNodeIdForVerbProperties)
        {
            FieldReaderInstList<VerbProperties> results = new FieldReaderInstList<VerbProperties>();
            WeaponAttachmentProperties current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForVerbProperties);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForVerbProperties];
            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForVerbProperties)
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
                        if (currentComp != null && comp != currentComp)
                        {
                            cache &= currentComp.Props.verbPropertiesObjectPatchByOtherPart;
                            cache.DefaultValue = defaultValue && currentComp.Props.verbPropertiesObjectPatchByOtherPart.DefaultValue;
                        }
                    }


                    results |= comp.Props.verbPropertiesObjectPatch & cache;

                    results |= comp.VerbPropertiesObjectPatch(null) & cache;
                }
            }
            return results;
        }


        public FieldReaderInstList<Tool> ToolsObjectPatch(string childNodeIdForTool)
        {
            FieldReaderInstList<Tool> results = new FieldReaderInstList<Tool>();
            WeaponAttachmentProperties current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForTool);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForTool];
            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForTool)
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
                        if (currentComp != null && comp != currentComp)
                        {
                            cache &= currentComp.Props.toolsObjectPatchByOtherPart;
                            cache.DefaultValue = defaultValue && currentComp.Props.toolsObjectPatchByOtherPart.DefaultValue;
                        }
                    }


                    results |= comp.Props.toolsObjectPatch & cache;

                    results |= comp.ToolsObjectPatch(null) & cache;
                }
            }
            return results;
        }



        public FieldReaderInstList<CompProperties> CompPropertiesObjectPatch()
        {
            FieldReaderInstList<CompProperties> results = new FieldReaderInstList<CompProperties>();
            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity)
                {
                    results |= (comp.Props.compPropertiesObjectPatch | comp.CompPropertiesObjectPatch()) & properties.compPropertiesObjectPatchByChildPart;
                }
            }
            return results;
        }


        public FieldReaderBoolList<VerbProperties> VerbPropertiesBoolAndPatch(string childNodeIdForVerbProperties)
        {
            FieldReaderBoolList<VerbProperties> results = new FieldReaderBoolList<VerbProperties>();
            WeaponAttachmentProperties current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForVerbProperties);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForVerbProperties];
            NodeContainer container = ChildNodes;
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForVerbProperties)
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
                        if (currentComp != null && comp != currentComp)
                        {
                            cache &= currentComp.Props.verbPropertiesBoolAndPatchByOtherPart;
                            cache.DefaultValue = defaultValue && currentComp.Props.verbPropertiesBoolAndPatchByOtherPart.DefaultValue;
                        }
                    }


                    results &= comp.Props.verbPropertiesBoolAndPatch & cache;
                    results.DefaultValue = true;

                    results &= comp.VerbPropertiesBoolAndPatch(null) & cache;
                    results.DefaultValue = true;
                }
            }
            return results;
        }


        public FieldReaderBoolList<Tool> ToolsBoolAndPatch(string childNodeIdForTool)
        {
            FieldReaderBoolList<Tool> results = new FieldReaderBoolList<Tool>();
            WeaponAttachmentProperties current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForTool);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForTool];
            NodeContainer container = ChildNodes;
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForTool)
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
                        if (currentComp != null && comp != currentComp)
                        {
                            cache &= currentComp.Props.toolsBoolAndPatchByOtherPart;
                            cache.DefaultValue = defaultValue && currentComp.Props.toolsBoolAndPatchByOtherPart.DefaultValue;
                        }
                    }


                    results &= comp.Props.toolsBoolAndPatch & cache;
                    results.DefaultValue = true;

                    results &= comp.ToolsBoolAndPatch(null) & cache;
                    results.DefaultValue = true;
                }
            }
            return results;
        }


        public FieldReaderBoolList<CompProperties> CompPropertiesBoolAndPatch()
        {
            FieldReaderBoolList<CompProperties> results = new FieldReaderBoolList<CompProperties>();
            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity)
                {
                    results &= comp.Props.compPropertiesBoolAndPatch & properties.compPropertiesBoolAndPatchByChildPart;
                    results.DefaultValue = true;
                }
            }
            return results;
        }


        public FieldReaderBoolList<VerbProperties> VerbPropertiesBoolOrPatch(string childNodeIdForVerbProperties)
        {
            FieldReaderBoolList<VerbProperties> results = new FieldReaderBoolList<VerbProperties>();
            WeaponAttachmentProperties current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForVerbProperties);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForVerbProperties];
            NodeContainer container = ChildNodes;
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForVerbProperties)
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
                        if (currentComp != null && comp != currentComp)
                        {
                            cache &= currentComp.Props.verbPropertiesBoolOrPatchByOtherPart;
                            cache.DefaultValue = defaultValue && currentComp.Props.verbPropertiesBoolOrPatchByOtherPart.DefaultValue;
                        }
                    }


                    results |= comp.Props.verbPropertiesBoolOrPatch & cache;
                    results.DefaultValue = false;

                    results |= comp.VerbPropertiesBoolOrPatch(null) & cache;
                    results.DefaultValue = false;
                }
            }
            return results;
        }


        public FieldReaderBoolList<Tool> ToolsBoolOrPatch(string childNodeIdForTool)
        {
            FieldReaderBoolList<Tool> results = new FieldReaderBoolList<Tool>();
            WeaponAttachmentProperties current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForTool);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForTool];
            NodeContainer container = ChildNodes;
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForTool)
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
                        if (currentComp != null && comp != currentComp)
                        {
                            cache &= currentComp.Props.toolsBoolOrPatchByOtherPart;
                            cache.DefaultValue = defaultValue && currentComp.Props.toolsBoolOrPatchByOtherPart.DefaultValue;
                        }
                    }


                    results |= comp.Props.toolsBoolOrPatch & cache;
                    results.DefaultValue = false;

                    results |= comp.ToolsBoolOrPatch(null) & cache;
                    results.DefaultValue = false;
                }
            }
            return results;
        }


        public FieldReaderBoolList<CompProperties> CompPropertiesBoolOrPatch()
        {
            FieldReaderBoolList<CompProperties> results = new FieldReaderBoolList<CompProperties>();
            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity)
                {
                    results |= comp.Props.compPropertiesBoolOrPatch & properties.compPropertiesBoolOrPatchByChildPart;
                    results.DefaultValue = false;
                }
            }
            return results;
        }
    }
}
