using System;
using RW_NodeTree;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class ModularizationWeapon
    {
        public bool Unchangeable(string id)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            GetOrGenCurrentPartAttachmentProperties().TryGetValue(id, out WeaponAttachmentProperties? current);
            return Unchangeable(container[id], current);
        }
        public static bool Unchangeable(Thing? thing, WeaponAttachmentProperties? properties)
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
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            GetOrGenCurrentPartAttachmentProperties().TryGetValue(id, out WeaponAttachmentProperties? current);
            return NotDraw(container[id], current);
        }
        public static bool NotDraw(Thing? thing, WeaponAttachmentProperties? properties)
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
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            GetOrGenCurrentPartAttachmentProperties().TryGetValue(id, out WeaponAttachmentProperties? current);
            return NotUseTools(container[id], current);
        }
        public static bool NotUseTools(Thing? thing, WeaponAttachmentProperties? properties)
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
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            GetOrGenCurrentPartAttachmentProperties().TryGetValue(id, out WeaponAttachmentProperties? current);
            return NotUseVerbProperties(container[id], current);
        }
        public static bool NotUseVerbProperties(Thing? thing, WeaponAttachmentProperties? properties)
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
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            GetOrGenCurrentPartAttachmentProperties().TryGetValue(id, out WeaponAttachmentProperties? current);
            return NotUseCompProperties(container[id], current);
        }
        public static bool NotUseCompProperties(Thing? thing, WeaponAttachmentProperties? properties)
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
