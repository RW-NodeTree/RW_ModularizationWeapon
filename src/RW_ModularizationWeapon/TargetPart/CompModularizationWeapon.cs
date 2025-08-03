using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        public bool IsSwapRoot
        {
            get
            {
                lock (this)
                {
                    return this.RootPart == this && this.occupiers == null;
                }
            }
        }

        public bool SetTargetPart(string id, LocalTargetInfo targetInfo)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            lock (this)
            {
                //only allow thing not have parent or spawned?
                //if (id != null && NodeProccesser.AllowNode(targetInfo.Thing, id))
                LocalTargetInfo currentTargetInfo = GetTargetPart(id);
                if (id != null && currentTargetInfo != targetInfo && AllowPart(targetInfo.Thing, id))
                {

                    //ThingOwner prevOwner = targetPartsHoldingOwnerWithId.TryGetValue(id);
                    //targetOwner?.Remove(targetInfo.Thing);
                    //Log.Message($"{parent}->SetTargetPart {id} : {targetInfo}; {UsingTargetPart}");
                    targetPartsWithId.TryGetValue(id, out currentTargetInfo);
                    CompModularizationWeapon? part = currentTargetInfo.Thing;
                    if (part != null)
                    {
                        part.occupiers = null;
                        if (!swap) part.UpdateTargetPartVNode();
                    }
                    Thing? prevPart = container[id];
                    if (targetInfo.Thing == prevPart && !swap) targetPartsWithId.Remove(id);
                    else
                    {
                        targetPartsWithId.SetOrAdd(id, targetInfo);
                        part = targetInfo.Thing;
                        if (part != null) part.occupiers = this;
                    }
                    //targetOwner?.TryAdd(targetInfo.Thing, false);
                    targetPartChanged = true;
                    if (!swap) UpdateTargetPartVNode();
                    return true;
                }
                return false;
            }
        }

        public LocalTargetInfo GetTargetPart(string id)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            lock (this)
            {
                if (!targetPartsWithId.TryGetValue(id, out LocalTargetInfo result)) result = container[id];
                return result;
            }
        }

        public void UpdateCurrentPartVNode()
        {
            CompModularizationWeapon root = RootPart;
            if(root != this)
            {
                root.UpdateCurrentPartVNode();
                return;
            }
            VNode node = new VNode(null,parent.def.defName);
            AppendVNodeForCurrentPart(node);
        }

        private void AppendVNodeForCurrentPart(VNode node)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            lock (currentPartAttachmentPropertiesCache)
            {
                currentPartVNode = node;
                foreach(string id in PartIDs)
                {
                    Thing? target = container[id];
                    if(target != null)
                    {
                        VNode child = new VNode(id, target.def.defName, node);
                        CompModularizationWeapon? comp = target;
                        comp?.AppendVNodeForCurrentPart(child);
                    }
                }
                currentPartAttachmentPropertiesCache.Clear();
            }
        }

        public void UpdateTargetPartVNode()
        {
            CompModularizationWeapon root = RootOccupierPart;
            if(root != this)
            {
                root.UpdateTargetPartVNode();
                return;
            }
            VNode node = new VNode(null,parent.def.defName);
            AppendVNodeForTargetPart(node);
        }

        private void AppendVNodeForTargetPart(VNode node)
        {
            lock (targetPartAttachmentPropertiesCache)
            {
                targetPartVNode = node;
                foreach(string id in PartIDs)
                {
                    Thing? target = GetTargetPart(id).Thing;
                    if(target != null)
                    {
                        VNode child = new VNode(id, target.def.defName, node);
                        CompModularizationWeapon? comp = target;
                        comp?.AppendVNodeForTargetPart(child);
                    }
                }
                targetPartAttachmentPropertiesCache.Clear();
            }
        }

        public void SwapTargetPart()
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            lock (this)
            {
                if(IsSwapRoot)
                {
                    swap = true;
                    NeedUpdate = true;
                    container.Comp.UpdateNode();
                }
            }
        }

        private void Private_ClearTargetPart()
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            lock (this)
            {
                foreach (CompModularizationWeapon? comp in container.Values)
                {
                    comp?.Private_ClearTargetPart();
                }
                foreach (LocalTargetInfo info in targetPartsWithId.Values)
                {
                    ((CompModularizationWeapon?)info.Thing)?.Private_ClearTargetPart();
                }
                if (targetPartsWithId.Count > 0)
                {
                    List<string> ids = new List<string>(targetPartsWithId.Keys);
                    foreach(string id in ids) SetTargetPart(id, container[id]);
                }
            }
        }
        public void ClearTargetPart() => RootPart.Private_ClearTargetPart();


        public IEnumerator<(string, Thing?, WeaponAttachmentProperties)> GetEnumerator()
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            List<(string, Thing?, WeaponAttachmentProperties)> result = new List<(string, Thing?, WeaponAttachmentProperties)>(PartIDs.Count);
            lock (container)
            {
                foreach (string id in PartIDs)
                {
                    WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                    if (properties != null) result.Add((id, container[id], properties));
                }
            }
            return result.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
