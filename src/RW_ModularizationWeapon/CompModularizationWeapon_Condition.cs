using RW_NodeTree;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class ModularizationWeapon
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
                ModularizationWeapon? weapon = thing as ModularizationWeapon;
                if (weapon != null)
                {
                    return weapon.Props.unchangeable || properties.unchangeable;
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
                ModularizationWeapon? weapon = thing as ModularizationWeapon;
                if (weapon != null)
                {
                    return weapon.Props.notDrawInParent || properties.notDraw;
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
                ModularizationWeapon? weapon = thing as ModularizationWeapon;
                if (weapon != null)
                {
                    return weapon.Props.notAllowParentUseTools || properties.notUseTools;
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
                ModularizationWeapon? weapon = thing as ModularizationWeapon;
                if (weapon != null)
                {
                    return weapon.Props.notAllowParentUseVerbProperties || properties.notUseVerbProperties;
                }
                else
                {
                    return properties.notUseVerbProperties;
                }
            }
            return false;
        }


        public bool NotUseCompProperties(string id)
        {
            NodeContainer? childs = ChildNodes;
            if (childs != null)
                return internal_NotUseCompProperties(childs[id], CurrentPartWeaponAttachmentPropertiesById(id));
            else
                return false;
        }
        internal static bool internal_NotUseCompProperties(Thing? thing, WeaponAttachmentProperties? properties)
        {
            if (thing != null && properties != null)
            {
                ModularizationWeapon? weapon = thing as ModularizationWeapon;
                if (weapon != null)
                {
                    return weapon.Props.notAllowParentUseCompProperties || properties.notUseCompProperties;
                }
                else
                {
                    return properties.notUseCompProperties;
                }
            }
            return false;
        }
    }
}
