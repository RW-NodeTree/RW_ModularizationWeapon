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
            get
            {
                if(RootPart.usingTargetPart != usingTargetPart)
                {
                    NeedUpdate = true;
                    NodeProccesser.UpdateNode();
                }
                return usingTargetPart;
            }
            set
            {
                //Log.Message($"UsingTargetPart {parent} : {value}; org : {usingTargetPart}");
                while (UsingTargetPart != value)
                {
                    CompModularizationWeapon root = RootPart;
                    if (root == this)
                    {
                        NeedUpdate = true;
                        usingTargetPartChange = true;
                        NodeProccesser.UpdateNode();
                    }
                    else root.UsingTargetPart = value;
                }
            }
        }

        private bool TargetPartChanged
        {
            get => targetPartChanged;
            set
            {
                if (targetPartChanged = value)
                {
                    CompModularizationWeapon parent = ParentPart;
                    if (parent != null) parent.TargetPartChanged = value;
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
            bool result = false;
            bool prveUsingTargetPart = UsingTargetPart;
            UsingTargetPart = false;
            ThingOwner targetOwner = targetInfo.Thing?.holdingOwner;
            if(targetOwner == ChildNodes) targetInfo.Thing.holdingOwner = null;
            if (id != null && NodeProccesser.AllowNode(targetInfo.Thing, id))
            {
                if (targetOwner == ChildNodes) targetInfo.Thing.holdingOwner = targetOwner;
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
                TargetPartChanged = true;
                //targetOwner?.TryAdd(targetInfo.Thing, false);
                result = true;
            }
            UsingTargetPart = prveUsingTargetPart;
            return result;
        }


        public void ApplyTargetPart(IntVec3 pos, Map map)
        {
            if(RootPart != this)
            {
                RootPart.ApplyTargetPart(pos, map);
                return;
            }
            bool prevUsingTargetPart = UsingTargetPart;

            UsingTargetPart = false;
            targetPartsHoldingOwnerWithId.Clear();
            foreach(Thing target in AllTargetPart())
            {
                if (target != null && target.Spawned) target.DeSpawn();
            }


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

            UsingTargetPart = true;

            if (map != null)
            {
                foreach (Thing target in AllTargetPart())
                {
                    if (target != null && !target.Spawned) GenPlace.TryPlaceThing(target, pos, map, ThingPlaceMode.Near);
                }
            }



            //NeedUpdate = true;

            //notUpdateComp = true;
            //UsingTargetPart = false;

            //Log.Message($"{parent}->NeedUpdate : {NeedUpdate}");
            void ClearTargetPart(CompModularizationWeapon values)
            {
                foreach (Thing part in values.ChildNodes.Values)
                {
                    CompModularizationWeapon comp = part;
                    if(comp != null) ClearTargetPart(comp);
                }
                if (values.targetPartsWithId.Count > 0)
                {
                    values.targetPartsHoldingOwnerWithId.Clear();
                    values.targetPartsWithId.Clear();

                    values.TargetPartChanged = true;
                }
            }
            ClearTargetPart(this);


            //Log.Message($"{parent}->NeedUpdate : {NeedUpdate}");

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
