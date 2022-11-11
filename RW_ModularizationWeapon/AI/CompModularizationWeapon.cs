using RW_NodeTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        internal IEnumerable<Toil> CarryTarget(TargetIndex craftingTable, TargetIndex hauledThingIndex)
        {
            foreach (string id in NodeProccesser.RegiestedNodeId)
            {
                LocalTargetInfo target = ChildNodes[id];
                if (targetPartsWithId.ContainsKey(id))
                {
                    target = targetPartsWithId[id];
                    if (target.HasThing && target.Thing.Spawned)
                    {
                        Toil toil = new Toil();
                        toil.initAction = delegate ()
                        {
                            Pawn actor = toil.actor;
                            Job job = actor.CurJob;
                            job.SetTarget(hauledThingIndex, target);
                            job.count = 1;
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

                        toil = new Toil();
                        toil.initAction = delegate ()
                        {
                            Pawn actor = toil.actor;
                            Job job = actor.CurJob;
                            targetPartsWithId[id] = job.GetTarget(hauledThingIndex);
                            targetPartsWithId[id].Thing.Position = job.GetTarget(craftingTable).Cell;
                            actor.Reserve(targetPartsWithId[id], job, 1, 1);
                        };
                        yield return toil;
                    }

                }

                CompModularizationWeapon comp = target.Thing;
                if (comp != null)
                {
                    foreach (Toil child in comp.CarryTarget(craftingTable, hauledThingIndex))
                    {
                        yield return child;
                    }
                }

            }
        }

        internal IEnumerable<LocalTargetInfo> AllTargetPart()
        {
            foreach (string id in NodeProccesser.RegiestedNodeId)
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
