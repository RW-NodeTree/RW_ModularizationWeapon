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

        public bool UsingTargetPart
        {
            get => usingTargetPart;
            set
            {
                //Log.Message($"UsingTargetPart {parent} : {value}; org : {usingTargetPart}");
                if (usingTargetPart != value) NodeProccesser.UpdateNode("UpdateUsingTargetPart", value);
            }
        }


        public LocalTargetInfo OrginalPart(string id) => UsingTargetPart ? (targetPartsWithId.TryGetValue(id).Thing ?? ChildNodes[id]) : ChildNodes[id];


        public bool SetTargetPart(string id, LocalTargetInfo targetInfo)
        {
            bool prevSetingTargetPart = setingTargetPart;
            setingTargetPart = true;
            if (id != null && NodeProccesser.AllowNode(targetInfo.Thing, id))
            {
                ThingOwner targetOwner = targetInfo.Thing?.holdingOwner;
                ThingOwner prevOwner = targetPartsHoldingOwnerWithId.TryGetValue(id);
                Thing prevPart = ChildNodes[id];
                targetOwner?.Remove(targetInfo.Thing);
                //Log.Message($"{parent}->SetTargetPart {id} : {targetInfo}; {UsingTargetPart}");
                if (UsingTargetPart)
                {

                    if(!targetPartsWithId.ContainsKey(id)) targetPartsWithId.Add(id, prevPart);
                    targetPartsHoldingOwnerWithId.SetOrAdd(id, targetOwner);

                    ChildNodes[id] = targetInfo.Thing;

                    if ((targetPartsWithId.TryGetValue(id).Thing ?? prevPart) == targetInfo.Thing)
                    {
                        targetPartsWithId.Remove(id);
                        targetPartsHoldingOwnerWithId.Remove(id);
                    }
                    if (prevPart != targetInfo.Thing)
                    {
                        ((CompChildNodeProccesser)prevPart)?.UpdateNode("UpdateUsingTargetPart", false);
                        prevOwner?.TryAdd(prevPart, false);
                    }
                    NodeProccesser.UpdateNode();

                }
                else
                {
                    if (targetInfo.Thing == prevPart)
                    {
                        targetPartsWithId.Remove(id);
                        targetPartsHoldingOwnerWithId.Remove(id);
                    }
                    else
                    {
                        targetPartsWithId.SetOrAdd(id, targetInfo.Thing);
                        targetPartsHoldingOwnerWithId.SetOrAdd(id, targetOwner);
                    }
                    targetOwner?.TryAdd(targetInfo.Thing, false);
                }
                setingTargetPart = prevSetingTargetPart;
                return true;
            }
            setingTargetPart = prevSetingTargetPart;
            return false;
        }


        public void ApplyTargetPart(IntVec3 pos, Map map, bool updateNode = true)
        {
            UsingTargetPart = false;
            foreach (KeyValuePair<string, LocalTargetInfo> data in targetPartsWithId)
            {
                Thing thing = ChildNodes[data.Key];
                if (data.Value.HasThing && data.Value.Thing.Spawned) data.Value.Thing.DeSpawn();
                ChildNodes[data.Key] = data.Value.Thing;
                if (thing != null && ChildNodes[data.Key] != thing)
                {
                    if(map != null) GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near);
                    CompChildNodeProccesser comp = thing;
                    if(comp != null)
                    {
                        comp.NeedUpdate = true;
                        comp.UpdateNode();
                    }

                }
            }
            //NeedUpdate = true;

            targetPartsWithId.Clear();
            targetPartsHoldingOwnerWithId.Clear();

            //Log.Message($"{parent}->NeedUpdate : {NeedUpdate}");

            foreach (Thing item in ChildNodes.Values)
            {
                CompModularizationWeapon comp = item;
                if (comp != null)
                {
                    comp.ApplyTargetPart(pos, map,false);
                }
            }

            if (CombatExtended_CompAmmoUser != null &&
                CombatExtended_CompAmmoUser_currentAmmoInt != null &&
                CombatExtended_CompAmmoUser_CurMagCount_get != null &&
                CombatExtended_CompAmmoUser_CurMagCount_set != null
                )
            {
                for (int i = 0; i < parent.AllComps.Count; i++)
                {
                    ThingComp comp = parent.AllComps[i];
                    Type type = comp.GetType();
                    if (CombatExtended_CompAmmoUser.IsAssignableFrom(type))
                    {
                        ThingDef def = CombatExtended_CompAmmoUser_currentAmmoInt(comp);
                        int count = (int)CombatExtended_CompAmmoUser_CurMagCount_get.Invoke(comp, null);
                        if (def != null && count > 0)
                        {
                            Thing thing = ThingMaker.MakeThing(def, null);
                            thing.stackCount = count;
                            CombatExtended_CompAmmoUser_CurMagCount_set.Invoke(comp, new object[] { 0 });
                            GenThing.TryDropAndSetForbidden(thing, pos, map, ThingPlaceMode.Near, out _, false);
                        }
                    }
                }
            }

            if(updateNode) NodeProccesser.UpdateNode();
        }


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
