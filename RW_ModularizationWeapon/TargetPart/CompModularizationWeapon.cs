using RimWorld;
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

        public bool SetTargetPart(string id, LocalTargetInfo targetInfo)
        {
            //only allow thing not have parent or spawned?
            //if (id != null && NodeProccesser.AllowNode(targetInfo.Thing, id))
            if (id != null && AllowPart(targetInfo.Thing, id))
            {
                //ThingOwner prevOwner = targetPartsHoldingOwnerWithId.TryGetValue(id);
                Thing prevPart = ChildNodes[id];
                //targetOwner?.Remove(targetInfo.Thing);
                //Log.Message($"{parent}->SetTargetPart {id} : {targetInfo}; {UsingTargetPart}");
                if (targetInfo.Thing == prevPart) targetPartsWithId.Remove(id);
                else targetPartsWithId.SetOrAdd(id, targetInfo);
                //targetOwner?.TryAdd(targetInfo.Thing, false);
                MarkTargetPartChanged();
                return true;
            }
            return false;
        }

        public LocalTargetInfo GetTargetPart(string id)
        {
            if (!targetPartsWithId.TryGetValue(id, out LocalTargetInfo result)) result = ChildNodes[id];
            return result;
        }

        public void SwapTargetPart()
        {
            RootNode.NeedUpdate = true;
            NodeProccesser.UpdateNode();
        }

        private void Private_ClearTargetPart()
        {
            foreach (CompModularizationWeapon comp in ChildNodes.Values)
            {
                comp?.Private_ClearTargetPart();
            }
            foreach (LocalTargetInfo info in targetPartsWithId.Values)
            {
                ((CompModularizationWeapon)info.Thing)?.Private_ClearTargetPart();
            }
            if (targetPartsWithId.Count > 0)
            {
                targetPartsWithId.Clear();
                MarkTargetPartChanged();
            }
        }
        public void ClearTargetPart() => RootPart.Private_ClearTargetPart();


        public IEnumerator<(string, Thing, WeaponAttachmentProperties)> GetEnumerator()
        {
            foreach (string id in PartIDs)
            {
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (properties != null) yield return (id, ChildNodes[id], properties);
            }
            yield break;
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
