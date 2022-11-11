using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        public bool Unchangeable(string id) => internal_Unchangeable(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_Unchangeable(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.unchangeable || properties.unchangeable;
                }
                else
                {
                    return properties.unchangeable;
                }
            }
            return false;
        }


        public bool NotDraw(string id) => internal_NotDraw(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_NotDraw(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.notDrawInParent || properties.notDraw;
                }
                else
                {
                    return properties.notDraw;
                }
            }
            return false;
        }


        public bool NotUseTools(string id) => internal_NotUseTools(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_NotUseTools(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.notAllowParentUseTools || properties.notUseTools;
                }
                else
                {
                    return properties.notUseTools;
                }
            }
            return false;
        }


        public bool NotUseVerbProperties(string id) => internal_NotUseVerbProperties(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_NotUseVerbProperties(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.notAllowParentUseVerbProperties || properties.notUseVerbProperties;
                }
                else
                {
                    return properties.notUseVerbProperties;
                }
            }
            return false;
        }

        public bool VerbPropertiesObjectPatchByOtherPart(string id) => internal_VerbPropertiesObjectPatchByOtherPart(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_VerbPropertiesObjectPatchByOtherPart(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.verbPropertiesObjectPatchByOtherPart && properties.verbPropertiesObjectPatchByOtherPart;
                }
                else
                {
                    return properties.verbPropertiesObjectPatchByOtherPart;
                }
            }
            return false;
        }

        public bool ToolsObjectPatchByOtherPart(string id) => internal_ToolsObjectPatchByOtherPart(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_ToolsObjectPatchByOtherPart(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.toolsObjectPatchByOtherPart && properties.toolsObjectPatchByOtherPart;
                }
                else
                {
                    return properties.toolsObjectPatchByOtherPart;
                }
            }
            return false;
        }

        public bool VerbPropertiesBoolAndPatchByOtherPart(string id) => internal_VerbPropertiesBoolAndPatchByOtherPart(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_VerbPropertiesBoolAndPatchByOtherPart(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.verbPropertiesBoolAndPatchByOtherPart && properties.verbPropertiesBoolAndPatchByOtherPart;
                }
                else
                {
                    return properties.verbPropertiesBoolAndPatchByOtherPart;
                }
            }
            return false;
        }

        public bool ToolsBoolAndPatchByOtherPart(string id) => internal_ToolsBoolAndPatchByOtherPart(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_ToolsBoolAndPatchByOtherPart(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.toolsBoolAndPatchByOtherPart && properties.toolsBoolAndPatchByOtherPart;
                }
                else
                {
                    return properties.toolsBoolAndPatchByOtherPart;
                }
            }
            return false;
        }

        public bool VerbPropertiesBoolOrPatchByOtherPart(string id) => internal_VerbPropertiesBoolOrPatchByOtherPart(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_VerbPropertiesBoolOrPatchByOtherPart(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.verbPropertiesBoolOrPatchByOtherPart && properties.verbPropertiesBoolOrPatchByOtherPart;
                }
                else
                {
                    return properties.verbPropertiesBoolOrPatchByOtherPart;
                }
            }
            return false;
        }

        public bool ToolsBoolOrPatchByOtherPart(string id) => internal_ToolsBoolOrPatchByOtherPart(ChildNodes[id], Props.WeaponAttachmentPropertiesById(id));
        internal static bool internal_ToolsBoolOrPatchByOtherPart(Thing thing, WeaponAttachmentProperties properties)
        {
            if (thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon comp = thing;
                if (comp != null && comp.Validity)
                {
                    return comp.Props.toolsBoolOrPatchByOtherPart && properties.toolsBoolOrPatchByOtherPart;
                }
                else
                {
                    return properties.toolsBoolOrPatchByOtherPart;
                }
            }
            return false;
        }
    }
}
