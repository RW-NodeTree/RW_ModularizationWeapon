﻿using System;
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
        public bool ShowTargetPart
        {
            get
            {
                bool result = false;
                CompModularizationWeapon current = this;
                while (!result && current != null)
                {
                    result = current.showTargetPart;
                    current = current.targetModeParent?.parent;
                }
                return result;
            }
            set
            {
                //Log.Message($"ShowTargetPart {parent} : {value}; org : {ShowTargetPart}");

                showTargetPart = value;
                UsingTargetPart = ShowTargetPart;
                NodeProccesser?.UpdateNode();
            }
        }

        private bool UsingTargetPart
        {
            get => usingTargetPart;
            set
            {
                //Log.Message($"UsingTargetPart {parent} : {value}; org : {usingTargetPart}");
                if (usingTargetPart != value)
                {
                    usingTargetPart = value;
                    foreach (string id in NodeProccesser.RegiestedNodeId)
                    {
                        LocalTargetInfo cache;
                        if (targetPartsWithId.TryGetValue(id, out cache))
                        {
                            targetPartsWithId[id] = ChildNodes[id];
                            ChildNodes[id] = cache.Thing;
                        }
                        else
                        {
                            CompModularizationWeapon comp = ChildNodes[id];
                            if (comp != null)
                            {
                                comp.targetModeParent = NodeProccesser;
                                comp.UsingTargetPart = value;
                            }
                        }
                    }
                }
            }
        }


        public LocalTargetInfo OrginalPart(string id) => UsingTargetPart ? ((targetPartsWithId.TryGetValue(id)).Thing ?? ChildNodes[id]) : ChildNodes[id];


        public bool SetTargetPart(string id, LocalTargetInfo targetInfo)
        {
            if (id != null && NodeProccesser.AllowNode(targetInfo.Thing, id))
            {

                //Log.Message($"SetTargetPart {id} : {targetInfo}; {UsingTargetPart}");
                if (UsingTargetPart)
                {
                    if (!targetPartsWithId.ContainsKey(id)) targetPartsWithId.Add(id, ChildNodes[id]);
                    ChildNodes[id] = targetInfo.Thing;
                    if (targetPartsWithId[id].Thing == targetInfo.Thing) targetPartsWithId.Remove(id);
                    NeedUpdate = true;
                    NodeProccesser?.UpdateNode();
                }
                else
                {
                    if (targetInfo.Thing == ChildNodes[id])
                        targetPartsWithId.Remove(id);
                    else if ((targetInfo.Thing?.Spawned ?? true))
                        targetPartsWithId.SetOrAdd(id, targetInfo);
                }
                return true;
            }
            return false;
        }


        public void ResetTargetPart()
        {
            targetPartsWithId.Clear();
        }


        public void ApplyTargetPart(IntVec3 pos, Map map)
        {
            UsingTargetPart = false;
            foreach ((string id, LocalTargetInfo item) in targetPartsWithId)
            {
                Thing thing = ChildNodes[id];
                if (item.HasThing && item.Thing.Spawned) item.Thing.DeSpawn();
                ChildNodes[id] = item.Thing;
                if (thing != null && map != null && ChildNodes[id] != thing)
                {
                    GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near);
                }
            }

            ResetTargetPart();

            foreach (Thing item in ChildNodes.Values)
            {
                CompModularizationWeapon comp = item;
                if (comp != null)
                {
                    comp.ApplyTargetPart(pos, map);
                }
            }
        }


        public IEnumerator<(string, Thing, WeaponAttachmentProperties)> GetEnumerator()
        {
            foreach (string id in NodeProccesser.RegiestedNodeId)
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