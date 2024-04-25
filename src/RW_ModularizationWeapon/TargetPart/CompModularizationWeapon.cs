using System.Collections;
using System.Collections.Generic;
using System.Xml;
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
            if (id != null && AllowPart(targetInfo.Thing, id))
            {

                //ThingOwner prevOwner = targetPartsHoldingOwnerWithId.TryGetValue(id);
                Thing prevPart = ChildNodes[id];
                //targetOwner?.Remove(targetInfo.Thing);
                //Log.Message($"{parent}->SetTargetPart {id} : {targetInfo}; {UsingTargetPart}");
                CompModularizationWeapon part = GetTargetPart(id).Thing;
                if (part != null) part.occupiers = null;
                if (targetInfo.Thing == prevPart) targetPartsWithId.Remove(id);
                else
                {
                    targetPartsWithId.SetOrAdd(id, targetInfo);
                    part = targetInfo.Thing;
                    if (part != null) part.occupiers = this;
                }
                //targetOwner?.TryAdd(targetInfo.Thing, false);
                targetPartChanged = true;
                targetPartXmlNode = null;
                return true;
            }
            return false;
        }

        public LocalTargetInfo GetTargetPart(string id)
        {
            if (!targetPartsWithId.TryGetValue(id, out LocalTargetInfo result)) result = ChildNodes[id];
            return result;
        }

        public void UpdateTargetPartXmlTree()
        {
            if(occupiers != null)
            {
                CompModularizationWeapon root = this;
                CompModularizationWeapon current = occupiers;
                while (current != null)
                {
                    root = current;
                    current = current.occupiers;
                }
                root.UpdateTargetPartXmlTree();
                return;
            }
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement node = xmlDocument.CreateElement("root");
            node.SetAttribute("defName", parent.def.defName);
            xmlDocument.AppendChild(node);
            AppendXmlNodeForTargetPart(node);
        }

        private void AppendXmlNodeForTargetPart(XmlElement node)
        {
            targetPartXmlNode = node;
            cachedAttachmentProperties.Clear();
            foreach(string id in PartIDs)
            {
                Thing target = GetTargetPart(id).Thing;
                if(target != null)
                {
                    XmlElement child = node.OwnerDocument.CreateElement("id");
                    child.SetAttribute("defName", parent.def.defName);
                    node.AppendChild(child);
                    CompModularizationWeapon comp = target;
                    comp?.AppendXmlNodeForTargetPart(child);
                }
            }
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
                WeaponAttachmentProperties properties = WeaponAttachmentPropertiesById(id);
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
