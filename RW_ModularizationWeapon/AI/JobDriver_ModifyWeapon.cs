using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;

namespace RW_ModularizationWeapon.AI
{
    public class JobDriver_ModifyWeapon : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            Job job = this.job;
            LocalTargetInfo targetA = TargetA;
            if(targetA.HasThing)
            {
                CompCustomWeaponPort port = targetA.Thing.TryGetComp<CompCustomWeaponPort>();
                if(port != null)
                {
                    job.targetB = port.GetTarget();
                    LocalTargetInfo targetB = TargetB;
                    return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed) && pawn.Reserve(targetB, job, 1, -1, null, errorOnFailed) && ((CompModularizationWeapon)targetB.Thing) != null;
                }
            }
            return false;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {

            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);

            CompCustomWeaponPort port = TargetA.Thing.TryGetComp<CompCustomWeaponPort>();
            if (port != null)
            {
                job.targetB = port.GetTarget();
            }
            else
            {
                this.EndJobWith(JobCondition.Incompletable);
                yield break;
            }

            this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
            this.FailOnBurningImmobile(TargetIndex.B);
            CompModularizationWeapon comp = TargetB.Thing;
            if (comp != null)
            {
                if (!comp.AllowSwap) this.EndJobWith(JobCondition.Incompletable);
                Toil toil = new Toil();
                toil.initAction = delegate ()
                {
                    Pawn actor = toil.actor;
                    Job job = actor.CurJob;
                    foreach (LocalTargetInfo target in comp.AllTargetPart())
                    {
                        //Log.Message(target.ToString());
                        if (target.HasThing && target.Thing.Spawned && !actor.Reserve(target, job, 1, 1))
                        {
                            this.EndJobWith(JobCondition.Incompletable);
                            return;
                        }
                    }
                };
                yield return toil;

                if(comp.parent.Spawned)
                {
                    toil = new Toil();
                    toil.initAction = delegate ()
                    {
                        Pawn actor = toil.actor;
                        Job job = actor.CurJob;
                        job.count = 1;
                        if (!actor.Reserve(comp.parent, job, 1, 1))
                        {
                            this.EndJobWith(JobCondition.Incompletable);
                            return;
                        }
                    };
                    yield return toil;

                    yield return
                        Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
                        .FailOnDestroyedNullOrForbidden(TargetIndex.B)
                        .FailOnBurningImmobile(TargetIndex.B);
                    yield return Toils_Haul.StartCarryThing(TargetIndex.B)
                        .FailOnCannotTouch(TargetIndex.B, PathEndMode.ClosestTouch);
                    yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.A, PathEndMode.ClosestTouch)
                        .FailOnDestroyedNullOrForbidden(TargetIndex.A)
                        .FailOnBurningImmobile(TargetIndex.A);
                    yield return Toils_Haul.PlaceCarriedThingInCellFacing(TargetIndex.A)
                        .FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch);

                    toil = new Toil();
                    toil.initAction = delegate ()
                    {
                        Pawn actor = toil.actor;
                        Job job = actor.CurJob;
                        TargetThingB.Position = TargetThingA.Position;
                    };
                    yield return toil;
                }

                foreach (Toil forCarry in comp.CarryTarget(TargetIndex.A, TargetIndex.C))
                {
                    yield return forCarry;
                }

                yield return
                    Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell)
                    .FailOnDestroyedNullOrForbidden(TargetIndex.A)
                    .FailOnBurningImmobile(TargetIndex.A);

                yield return Toils_General.Wait(200)
                    .FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell)
                    .FailOnDestroyedNullOrForbidden(TargetIndex.A)
                    .WithProgressBarToilDelay(TargetIndex.A);
                
                toil = new Toil();
                toil.initAction = delegate ()
                {
                    Pawn actor = toil.actor;
                    Job job = actor.CurJob;
                    if (!comp.parent.Spawned)
                    {
                        GenPlace.TryPlaceThing(comp.parent, TargetA.Cell, actor.Map, ThingPlaceMode.Near);
                    }
                    comp.parent.Position = TargetA.Cell;
                    comp.SetChildPostion();
                    comp.SwapTargetPart();
                    comp.ClearTargetPart();
                };
                yield return toil
                    .FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            }
            else
            {
                this.EndJobWith(JobCondition.Incompletable);
            }
            yield break;
        }
    }
}
