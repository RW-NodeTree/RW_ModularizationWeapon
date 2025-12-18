using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class ModularizationWeapon
    {
        public bool IsSwapRoot => this.occupiers == null && this.ParentPart == null;

        private bool RemoveTargetPartInternal(string id, out LocalTargetInfo prve)
        {
            bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
            if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
            try
            {
                if (targetPartsWithId.TryGetValue(id, out prve))
                {
                    targetPartsWithId.Remove(id);
                    ModularizationWeapon? part = prve.Thing as ModularizationWeapon;
                    if (part != null)
                    {
                        part.occupiers = null;
                    }
                    return true;
                }
                //targetOwner?.TryAdd(targetInfo.Thing, false);
                return false;
            }
            finally
            {
                if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
            }
        }

        private bool SetTargetPartInternal(string id, LocalTargetInfo targetInfo, out LocalTargetInfo prve)
        {
            bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
            if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
            try
            {
                ModularizationWeapon? part;
                if (targetPartsWithId.TryGetValue(id, out prve))
                {
                    part = prve.Thing as ModularizationWeapon;
                    if (targetInfo == prve) return false;
                    else if (part != null)
                    {
                        part.occupiers = null;
                    }
                    targetPartsWithId.Remove(id);
                }
                targetPartsWithId[id] = targetInfo;
                part = targetInfo.Thing as ModularizationWeapon;
                if (part != null) part.occupiers = this;
                //targetOwner?.TryAdd(targetInfo.Thing, false);
                return true;
            }
            finally
            {
                if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
            }
        }

        //invoked when setting by player or none-system opt
        public bool SetTargetPart(string id, LocalTargetInfo targetInfo)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                //only allow thing not have parent or spawned?
                //if (id != null && NodeProccesser.AllowNode(targetInfo.Thing, id))
                if (id != null && AllowNode(targetInfo.Thing, id))
                {
                    //ThingOwner prevOwner = targetPartsHoldingOwnerWithId.TryGetValue(id);
                    //targetOwner?.Remove(targetInfo.Thing);
                    //Log.Message($"{parent}->SetTargetPart {id} : {targetInfo}; {UsingTargetPart}");
                    Thing? prevPart = container[id];
                    if(prevPart == targetInfo)
                    {
                        if (RemoveTargetPartInternal(id, out LocalTargetInfo prve))
                        {
                            ModularizationWeapon? part = prve.Thing as ModularizationWeapon;
                            part?.UpdateTargetPartVNode();
                            targetPartChanged = true;
                            UpdateTargetPartVNode();
                        }
                    }
                    else
                    {
                        if (SetTargetPartInternal(id, targetInfo, out LocalTargetInfo prve))
                        {
                            ModularizationWeapon? part = prve.Thing as ModularizationWeapon;
                            part?.UpdateTargetPartVNode();
                            targetPartChanged = true;
                            UpdateTargetPartVNode();
                        }
                    }
                    return true;
                }
                return false;
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public LocalTargetInfo GetTargetPart(string id)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            bool isReadLockHeld = readerWriterLockSlim.IsReadLockHeld || readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isReadLockHeld) readerWriterLockSlim.EnterReadLock();
            try
            {
                if (!targetPartsWithId.TryGetValue(id, out LocalTargetInfo result)) result = container[id];
                return result;
            }
            finally
            {
                if (!isReadLockHeld) readerWriterLockSlim.ExitReadLock();
            }
        }

        public void UpdateCurrentPartVNode()
        {
            ModularizationWeapon root = RootPart;
            if(root != this)
            {
                root.UpdateCurrentPartVNode();
                return;
            }
            VNode node = new VNode(null,def.defName);
            AppendVNodeForCurrentPart(node);
        }

        private void AppendVNodeForCurrentPart(VNode node)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
            if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
            try
            {
                VNodeCache = node;
                foreach(string id in PartIDs)
                {
                    Thing? target = container[id];
                    if(target != null)
                    {
                        VNode child = new VNode(id, target.def.defName, node);
                        ModularizationWeapon? comp = target as ModularizationWeapon;
                        comp?.AppendVNodeForCurrentPart(child);
                    }
                }
                partAttachmentPropertiesCache = null;
            }
            finally
            {
                if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void UpdateTargetPartVNode()
        {
            ModularizationWeapon root = RootOccupierPart;
            if(root != this)
            {
                root.UpdateTargetPartVNode();
                return;
            }
            VNode node = new VNode(null,def.defName);
            AppendVNodeForTargetPart(node);
        }

        private void AppendVNodeForTargetPart(VNode node)
        {
            bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
            if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
            try
            {
                VNodeCache_TargetPart = node;
                foreach(string id in PartIDs)
                {
                    Thing? target = GetTargetPart(id).Thing;
                    if(target != null)
                    {
                        VNode child = new VNode(id, target.def.defName, node);
                        ModularizationWeapon? comp = target as ModularizationWeapon;
                        comp?.AppendVNodeForTargetPart(child);
                    }
                }
                partAttachmentPropertiesCache_TargetPart = null;
            }
            finally
            {
                if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void SwapTargetPart()
        {
            if (!IsSwapRoot) throw new InvalidOperationException("SwapTargetPart can only called at root");
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                swap = true;
                container.NeedUpdate = true;
                container.UpdateNode();
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public void ClearTargetPart()
        {
            bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
            if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
            try
            {
                NodeContainer? container = ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                foreach (ModularizationWeapon? comp in container.Values)
                {
                    comp?.ClearTargetPart();
                }
                foreach (LocalTargetInfo info in targetPartsWithId.Values)
                {
                    (info.Thing as ModularizationWeapon)?.ClearTargetPart();
                }
                if (targetPartsWithId.Count > 0)
                {
                    List<string> ids = new List<string>(targetPartsWithId.Keys);
                    foreach (string id in ids) RemoveTargetPartInternal(id, out _);
                    targetPartChanged = true;
                    UpdateTargetPartVNode();
                }
            }
            finally
            {
                if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
            }
        }


        public IEnumerator<(string, Thing?, WeaponAttachmentProperties)> GetEnumerator()
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            List<(string, Thing?, WeaponAttachmentProperties)> result = new List<(string, Thing?, WeaponAttachmentProperties)>(PartIDs.Count);
            foreach (var prop in GetOrGenCurrentPartAttachmentProperties())
            {
                result.Add((prop.Key, container[prop.Key], prop.Value));
            }
            return result.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
