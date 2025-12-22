using System;
using System.Collections.Generic;
using RW_ModularizationWeapon.AI;
using RW_NodeTree;
using Verse;
using Verse.AI;

namespace RW_ModularizationWeapon
{
    public partial class ModularizationWeapon
    {
        internal List<Toil> GenCarryTargetToils(JobDriver_ModifyWeapon jobDriver, TargetIndex craftingTable, TargetIndex hauledThingIndex)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            bool isReadLockHeld = readerWriterLockSlim.IsReadLockHeld || readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isReadLockHeld) readerWriterLockSlim.EnterReadLock();
            try
            {
                List<Toil> result = new List<Toil>();
                if (jobDriver == null) return result;
                foreach (string id in PartIDs)
                {
                    ModularizationWeapon? comp = container[id] as ModularizationWeapon;
                    if (comp != null)
                    {
                        result.AddRange(comp.GenCarryTargetToils(jobDriver, craftingTable, hauledThingIndex));
                    }
                    if (targetPartsWithId.ContainsKey(id))
                    {
                        result.Capacity += 6;
                        LocalTargetInfo target = targetPartsWithId[id];
                        if (target.HasThing)
                        {
                            LocalTargetInfo temp = target;
                            Toil toil_JumpPoint = new Toil();
                            Toil toil = new Toil();
                            toil.initAction = delegate ()
                            {
                                //Log.Message(temp.Thing.ToString());
                                Pawn actor = toil.actor;
                                Job job = actor.CurJob;
                                job.SetTarget(hauledThingIndex, temp);
                                job.count = 1;
                                if(!temp.Thing.Spawned)
                                {
                                    jobDriver.JumpToToil(toil_JumpPoint);
                                    return;
                                }
                                if (!actor.Reserve(temp, job, 1, 1))
                                {
                                    jobDriver.EndJobWith(JobCondition.Incompletable);
                                    return;
                                }
                            };
                            result.Add(toil);

                            result.Add(
                                Toils_Goto.GotoThing(hauledThingIndex, PathEndMode.ClosestTouch)
                                .FailOnDestroyedNullOrForbidden(hauledThingIndex)
                                .FailOnBurningImmobile(hauledThingIndex)
                            );
                            result.Add(
                                Toils_Haul.StartCarryThing(hauledThingIndex)
                                .FailOnCannotTouch(hauledThingIndex, PathEndMode.ClosestTouch)
                            );
                            result.Add(
                                Toils_Haul.CarryHauledThingToCell(craftingTable, PathEndMode.ClosestTouch)
                                .FailOnDestroyedNullOrForbidden(craftingTable)
                                .FailOnBurningImmobile(craftingTable)
                            );
                            result.Add(
                                Toils_Haul.PlaceCarriedThingInCellFacing(craftingTable)
                                .FailOnCannotTouch(craftingTable, PathEndMode.ClosestTouch)
                            );

                            toil_JumpPoint.initAction = delegate ()
                            {
                                Pawn actor = toil_JumpPoint.actor;
                                Job job = actor.CurJob;
                                target = job.GetTarget(hauledThingIndex);
                                SetTargetPart(id, target);
                                target.Thing.Position = job.GetTarget(craftingTable).Cell;
                                actor.Reserve(targetPartsWithId[id], job, 1, 1);
                            };
                            result.Add(toil_JumpPoint);
                        }


                        comp = target.Thing as ModularizationWeapon;
                        if (comp != null)
                        {
                            result.AddRange(comp.GenCarryTargetToils(jobDriver, craftingTable, hauledThingIndex));
                        }
                    }
                }
                return result;
            }
            finally
            {
                if (!isReadLockHeld) readerWriterLockSlim.ExitReadLock();
            }
        }

        internal IEnumerable<LocalTargetInfo> AllTargetPart()
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            bool isReadLockHeld = readerWriterLockSlim.IsReadLockHeld || readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isReadLockHeld) readerWriterLockSlim.EnterReadLock();
            try
            {
                foreach (string id in PartIDs)
                {
                    LocalTargetInfo target = container[id];
                    if (targetPartsWithId.ContainsKey(id))
                    {
                        target = targetPartsWithId[id];
                        yield return target;
                    }
                    ModularizationWeapon? weapon = target.Thing as ModularizationWeapon;
                    if (weapon != null)
                    {
                        foreach (LocalTargetInfo childTarget in weapon.AllTargetPart())
                        {
                            yield return childTarget;
                        }
                    }
                }
                yield break;
            }
            finally
            {
                if (!isReadLockHeld) readerWriterLockSlim.ExitReadLock();
            }
        }
    }
}
