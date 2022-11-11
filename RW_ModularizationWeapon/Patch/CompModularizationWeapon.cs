using RW_NodeTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        public List<FieldReaderInst<VerbProperties>> VerbPropertiesObjectPatch(string childNodeIdForVerbProperties)
        {
            NodeContainer container = ChildNodes;
            List<FieldReaderInst<VerbProperties>> results = new List<FieldReaderInst<VerbProperties>>();
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && id != childNodeIdForVerbProperties && Props.WeaponAttachmentPropertiesById(id).verbPropertiesBoolAndPatchByChildPart)
                {
                    foreach (FieldReaderInst<VerbProperties> child in comp.Props.verbPropertiesObjectPatch)
                    {
                        int index = results.FindIndex(x => x.UsedType == child.UsedType);
                        if (index < 0) results.Add(child);
                        else results[index] |= child;
                    }
                }
            }
            return results;
        }


        public List<FieldReaderInst<Tool>> ToolsObjectPatch(string childNodeIdForTool)
        {
            NodeContainer container = ChildNodes;
            List<FieldReaderInst<Tool>> results = new List<FieldReaderInst<Tool>>();
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && id != childNodeIdForTool && Props.WeaponAttachmentPropertiesById(id).toolsBoolAndPatchByChildPart)
                {
                    foreach (FieldReaderInst<Tool> child in comp.Props.toolsObjectPatch)
                    {
                        int index = results.FindIndex(x => x.UsedType == child.UsedType);
                        if (index < 0) results.Add(child);
                        else results[index] |= child;
                    }
                }
            }
            return results;
        }



        public List<FieldReaderInst<CompProperties>> CompPropertiesObjectPatch()
        {
            NodeContainer container = ChildNodes;
            List<FieldReaderInst<CompProperties>> results = new List<FieldReaderInst<CompProperties>>();
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && Props.WeaponAttachmentPropertiesById(id).compPropertiesObjectPatchByChildPart)
                {
                    foreach (FieldReaderInst<CompProperties> child in comp.Props.compPropertiesObjectPatch)
                    {
                        int index = results.FindIndex(x => x.UsedType == child.UsedType);
                        if (index < 0) results.Add(child);
                        else results[index] |= child;
                    }
                }
            }
            return results;
        }


        public FieldReaderBoolList<VerbProperties> VerbPropertiesBoolAndPatch(string childNodeIdForVerbProperties)
        {
            NodeContainer container = ChildNodes;
            FieldReaderBoolList<VerbProperties> results = new FieldReaderBoolList<VerbProperties>();
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && id != childNodeIdForVerbProperties && Props.WeaponAttachmentPropertiesById(id).verbPropertiesBoolAndPatchByChildPart)
                {
                    results &= comp.Props.verbPropertiesBoolAndPatch;
                    results.DefaultValue = true;
                }
            }
            return results;
        }


        public FieldReaderBoolList<Tool> ToolsBoolAndPatch(string childNodeIdForTool)
        {
            NodeContainer container = ChildNodes;
            FieldReaderBoolList<Tool> results = new FieldReaderBoolList<Tool>();
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && id != childNodeIdForTool && Props.WeaponAttachmentPropertiesById(id).toolsBoolAndPatchByChildPart)
                {
                    results &= comp.Props.toolsBoolAndPatch;
                    results.DefaultValue = true;
                }
            }
            return results;
        }


        public FieldReaderBoolList<CompProperties> CompPropertiesBoolAndPatch()
        {
            NodeContainer container = ChildNodes;
            FieldReaderBoolList<CompProperties> results = new FieldReaderBoolList<CompProperties>();
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && Props.WeaponAttachmentPropertiesById(id).compPropertiesBoolAndPatchByChildPart)
                {
                    results &= comp.Props.compPropertiesBoolAndPatch;
                    results.DefaultValue = true;
                }
            }
            return results;
        }


        public FieldReaderBoolList<VerbProperties> VerbPropertiesBoolOrPatch(string childNodeIdForVerbProperties)
        {
            NodeContainer container = ChildNodes;
            FieldReaderBoolList<VerbProperties> results = new FieldReaderBoolList<VerbProperties>();
            results.DefaultValue = false;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && id != childNodeIdForVerbProperties && Props.WeaponAttachmentPropertiesById(id).verbPropertiesBoolOrPatchByChildPart)
                {
                    results |= comp.Props.verbPropertiesBoolOrPatch;
                    results.DefaultValue = false;
                }
            }
            return results;
        }


        public FieldReaderBoolList<Tool> ToolsBoolOrPatch(string childNodeIdForTool)
        {
            NodeContainer container = ChildNodes;
            FieldReaderBoolList<Tool> results = new FieldReaderBoolList<Tool>();
            results.DefaultValue = false;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && id != childNodeIdForTool && Props.WeaponAttachmentPropertiesById(id).toolsBoolOrPatchByChildPart)
                {
                    results |= comp.Props.toolsBoolOrPatch;
                    results.DefaultValue = false;
                }
            }
            return results;
        }


        public FieldReaderBoolList<CompProperties> CompPropertiesBoolOrPatch()
        {
            NodeContainer container = ChildNodes;
            FieldReaderBoolList<CompProperties> results = new FieldReaderBoolList<CompProperties>();
            results.DefaultValue = true;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                if (comp != null && comp.Validity && Props.WeaponAttachmentPropertiesById(id).compPropertiesBoolOrPatchByChildPart)
                {
                    results |= comp.Props.compPropertiesBoolOrPatch;
                    results.DefaultValue = true;
                }
            }
            return results;
        }
    }
}
