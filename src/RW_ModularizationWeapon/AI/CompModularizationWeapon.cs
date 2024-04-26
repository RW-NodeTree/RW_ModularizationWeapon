using System.Collections.Generic;
using RW_ModularizationWeapon.AI;
using Verse;
using Verse.AI;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        internal IEnumerable<Toil> CarryTarget(JobDriver_ModifyWeapon jobDriver, TargetIndex craftingTable, TargetIndex hauledThingIndex)
        {
            if (jobDriver == null) yield break;
            foreach (string id in PartIDs)
            {
                CompModularizationWeapon comp = ChildNodes[id];
                if (comp != null)
                {
                    foreach (Toil child in comp.CarryTarget(jobDriver, craftingTable, hauledThingIndex))
                    {
                        yield return child;
                    }
                }
                if (targetPartsWithId.ContainsKey(id))
                {
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
                        yield return toil;

                        yield return
                            Toils_Goto.GotoThing(hauledThingIndex, PathEndMode.ClosestTouch)
                            .FailOnDestroyedNullOrForbidden(hauledThingIndex)
                            .FailOnBurningImmobile(hauledThingIndex);
                        yield return
                            Toils_Haul.StartCarryThing(hauledThingIndex)
                            .FailOnCannotTouch(hauledThingIndex, PathEndMode.ClosestTouch);
                        yield return
                            Toils_Haul.CarryHauledThingToCell(craftingTable, PathEndMode.ClosestTouch)
                            .FailOnDestroyedNullOrForbidden(craftingTable)
                            .FailOnBurningImmobile(craftingTable);
                        yield return
                            Toils_Haul.PlaceCarriedThingInCellFacing(craftingTable)
                            .FailOnCannotTouch(craftingTable, PathEndMode.ClosestTouch);

                        toil_JumpPoint.initAction = delegate ()
                        {
                            Pawn actor = toil_JumpPoint.actor;
                            Job job = actor.CurJob;
                            target = job.GetTarget(hauledThingIndex);
                            SetTargetPart(id, target);
                            target.Thing.Position = job.GetTarget(craftingTable).Cell;
                            actor.Reserve(targetPartsWithId[id], job, 1, 1);
                        };
                        yield return toil_JumpPoint;
                    }


                    comp = target.Thing;
                    if (comp != null)
                    {
                        foreach (Toil child in comp.CarryTarget(jobDriver, craftingTable, hauledThingIndex))
                        {
                            yield return child;
                        }
                    }
                }

            }
        }

        internal IEnumerable<LocalTargetInfo> AllTargetPart()
        {
            foreach (string id in PartIDs)
            {
                LocalTargetInfo target = ChildNodes[id];
                if (targetPartsWithId.ContainsKey(id))
                {
                    target = targetPartsWithId[id];
                    yield return target;
                }
                CompModularizationWeapon comp = target.Thing;
                if (comp != null)
                {
                    foreach (LocalTargetInfo childTarget in comp.AllTargetPart())
                    {
                        yield return childTarget;
                    }
                }
            }
            yield break;
        }
    }
}
