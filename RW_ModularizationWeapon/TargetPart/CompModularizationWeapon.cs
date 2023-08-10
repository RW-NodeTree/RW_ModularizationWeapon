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
            internal set
            {
                //Log.Message($"UsingTargetPart {parent} : {value}; org : {usingTargetPart}");
                if (usingTargetPart != value)
                {
                    usingTargetPartChange = true;
                    NeedUpdate = true;
                    NodeProccesser.UpdateNode();
                }
            }
        }


        public LocalTargetInfo OrginalPart(string id)
        {
            if(UsingTargetPart && targetPartsWithId.TryGetValue(id, out LocalTargetInfo info)) return info;
            return ChildNodes[id];
        }

        public bool SetTargetPart(string id, LocalTargetInfo targetInfo)
        {
            bool prevUsingTargetPart = UsingTargetPart;
            UsingTargetPart = false;
            if (id != null && NodeProccesser.AllowNode(targetInfo.Thing, id))
            {
                ThingOwner targetOwner = targetInfo.Thing?.holdingOwner;
                //ThingOwner prevOwner = targetPartsHoldingOwnerWithId.TryGetValue(id);
                Thing prevPart = ChildNodes[id];
                //targetOwner?.Remove(targetInfo.Thing);
                //Log.Message($"{parent}->SetTargetPart {id} : {targetInfo}; {UsingTargetPart}");
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
                targetPartChanged = true;
                //targetOwner?.TryAdd(targetInfo.Thing, false);
                UsingTargetPart = prevUsingTargetPart;
                return true;
            }
            UsingTargetPart = prevUsingTargetPart;
            return false;
        }


        public void ApplyTargetPart(IntVec3 pos, Map map)
        {
            bool prevUsingTargetPart = UsingTargetPart;
            UsingTargetPart = false;
            targetPartsHoldingOwnerWithId.Clear();
            foreach(Thing target in targetPartsWithId.Values)
            {
                if (target != null && target.Spawned) target.DeSpawn();
            }

            UsingTargetPart = true;

            if(map != null)
            {
                foreach (Thing target in targetPartsWithId.Values)
                {
                    if (target != null && !target.Spawned) GenPlace.TryPlaceThing(target, pos, map, ThingPlaceMode.Near);
                }
            }


            foreach (Thing item in ChildNodes.Values)
            {
                CompModularizationWeapon comp = item;
                if (comp != null)
                {
                    comp.ApplyTargetPart(pos, map);
                }
            }
            //NeedUpdate = true;
            if (targetPartsWithId.Count > 0)
            {
                targetPartsHoldingOwnerWithId.Clear();
                targetPartsWithId.Clear();

                targetPartChanged = true;
            }
            UsingTargetPart = false;


            //Log.Message($"{parent}->NeedUpdate : {NeedUpdate}");




            //Log.Message($"{parent}->NeedUpdate : {NeedUpdate}");

            if (map != null &&
                CombatExtended_CompAmmoUser != null &&
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
            UsingTargetPart = prevUsingTargetPart;
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
