using System.Collections;
using System.Collections.Generic;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        public bool AllowSwap { get => this.RootPart == this && !this.Occupyed; }


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
                CompModularizationWeapon
                part = GetTargetPart(id).Thing;
                if (part != null) part.Occupyed = false;
                if (targetInfo.Thing == prevPart) targetPartsWithId.Remove(id);
                else
                {
                    targetPartsWithId.SetOrAdd(id, targetInfo);
                    part = targetInfo.Thing;
                    if (part != null) part.Occupyed = true;
                }
                //targetOwner?.TryAdd(targetInfo.Thing, false);
                targetPartChanged = true;
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
            if(AllowSwap)
            {
                swap = true;
                NeedUpdate = true;
                NodeProccesser.UpdateNode();
            }
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
                List<string> ids = new List<string>(targetPartsWithId.Keys);
                foreach(string id in ids) SetTargetPart(id, ChildNodes[id]);
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
