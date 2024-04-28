using System.Collections;
using System.Collections.Generic;
using System.Xml;
using RW_ModularizationWeapon.Tools;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        public bool AllowSwap { get => this.RootPart == this && this.occupiers == null; }


        public bool SetTargetPart(string id, LocalTargetInfo targetInfo)
        {
            //only allow thing not have parent or spawned?
            //if (id != null && NodeProccesser.AllowNode(targetInfo.Thing, id))
            LocalTargetInfo currentTargetInfo = GetTargetPart(id);
            if (id != null && currentTargetInfo != targetInfo && AllowPart(targetInfo.Thing, id))
            {

                //ThingOwner prevOwner = targetPartsHoldingOwnerWithId.TryGetValue(id);
                Thing prevPart = ChildNodes[id];
                //targetOwner?.Remove(targetInfo.Thing);
                //Log.Message($"{parent}->SetTargetPart {id} : {targetInfo}; {UsingTargetPart}");
                CompModularizationWeapon part = currentTargetInfo.Thing;
                if (part != null)
                {
                    part.occupiers = null;
                    if (!swap) part.UpdateTargetPartXmlTree();
                }
                if (targetInfo.Thing == prevPart) targetPartsWithId.Remove(id);
                else
                {
                    targetPartsWithId.SetOrAdd(id, targetInfo);
                    part = targetInfo.Thing;
                    if (part != null) part.occupiers = this;
                }
                //targetOwner?.TryAdd(targetInfo.Thing, false);
                targetPartChanged = true;
                if (!swap) UpdateTargetPartXmlTree();
                return true;
            }
            return false;
        }

        public LocalTargetInfo GetTargetPart(string id)
        {
            if (!targetPartsWithId.TryGetValue(id, out LocalTargetInfo result)) result = ChildNodes[id];
            return result;
        }

        public void UpdateCurrentPartXmlTree()
        {
            CompModularizationWeapon root = RootPart;
            if(root != this)
            {
                root.UpdateCurrentPartXmlTree();
                return;
            }
            VNode node = new VNode(null,parent.def.defName);
            AppendXmlNodeForCurrentPart(node);
        }

        private void AppendXmlNodeForCurrentPart(VNode node)
        {
            currentPartXmlNode = node;
            foreach(string id in PartIDs)
            {
                Thing target = ChildNodes[id];
                if(target != null)
                {
                    VNode child = new VNode(id,target.def.defName,node);
                    CompModularizationWeapon comp = target;
                    comp?.AppendXmlNodeForCurrentPart(child);
                }
            }
            currentPartAttachmentPropertiesCache.Clear();
        }

        public void UpdateTargetPartXmlTree()
        {
            if(occupiers != null && ParentPart != null)
            {
                CompModularizationWeapon root = this;
                CompModularizationWeapon current = occupiers ?? ParentPart;
                while (current != null)
                {
                    root = current;
                    current = current.occupiers ?? current.ParentPart;
                }
                root.UpdateTargetPartXmlTree();
                return;
            }
            VNode node = new VNode(null,parent.def.defName);
            AppendXmlNodeForTargetPart(node);
        }

        private void AppendXmlNodeForTargetPart(VNode node)
        {
            targetPartXmlNode = node;
            foreach(string id in PartIDs)
            {
                Thing target = GetTargetPart(id).Thing;
                if(target != null)
                {
                    VNode child = new VNode(id,target.def.defName,node);
                    CompModularizationWeapon comp = target;
                    comp?.AppendXmlNodeForTargetPart(child);
                }
            }
            targetPartAttachmentPropertiesCache.Clear();
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
                WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
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
