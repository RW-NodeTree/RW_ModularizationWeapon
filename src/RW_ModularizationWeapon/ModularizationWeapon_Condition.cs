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
    }
}
