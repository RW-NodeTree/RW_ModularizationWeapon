using RW_NodeTree;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        public bool Unchangeable(string id)
        {
            NodeContainer? childs = ChildNodes;
            if (childs != null)
                return internal_Unchangeable(childs[id], CurrentPartWeaponAttachmentPropertiesById(id));
            else
                return false;
        }
        internal static bool internal_Unchangeable(Thing? thing, WeaponAttachmentProperties? properties)
        {
            if (thing != null && properties != null)
            {
                //if (Prefs.DevMode) Log.Message($"properties.unchangeable : {properties.unchangeable}");
                CompModularizationWeapon? comp = thing;
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


        public bool NotDraw(string id)
        {

            NodeContainer? childs = ChildNodes;
            if (childs != null)
                return internal_NotDraw(childs[id], CurrentPartWeaponAttachmentPropertiesById(id));
            else
                return false;
        }
        internal static bool internal_NotDraw(Thing? thing, WeaponAttachmentProperties? properties)
        {
            if (thing != null && properties != null)
            {
                CompModularizationWeapon? comp = thing;
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


        public bool NotUseTools(string id)
        {
            NodeContainer? childs = ChildNodes;
            if (childs != null)
                return internal_NotUseTools(childs[id], CurrentPartWeaponAttachmentPropertiesById(id));
            else
                return false;
        }
        internal static bool internal_NotUseTools(Thing? thing, WeaponAttachmentProperties? properties)
        {
            if (thing != null && properties != null)
            {
                CompModularizationWeapon? comp = thing;
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


        public bool NotUseVerbProperties(string id)
        {
            NodeContainer? childs = ChildNodes;
            if (childs != null)
                return internal_NotUseVerbProperties(childs[id], CurrentPartWeaponAttachmentPropertiesById(id));
            else
                return false;
        }
        internal static bool internal_NotUseVerbProperties(Thing? thing, WeaponAttachmentProperties? properties)
        {
            if (thing != null && properties != null)
            {
                CompModularizationWeapon? comp = thing;
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
    }
}
