using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

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
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(()=>pawn.WorkTagIsDisabled(WorkTags.ManualDumb) || !(port.parent.TryGetComp<CompPowerTrader>()?.PowerOn ?? true) || !(port.parent.TryGetComp<CompRefuelable>()?.HasFuel ?? true));

            this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
            this.FailOnBurningImmobile(TargetIndex.B);
            CompModularizationWeapon comp = TargetB.Thing;
            if (comp != null)
            {
                if (!comp.IsSwapRoot) this.EndJobWith(JobCondition.Incompletable);
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
                
                Toil toil_JumpPoint = new Toil();

                toil = new Toil();
                toil.initAction = delegate ()
                {
                    Pawn actor = toil.actor;
                    Job job = actor.CurJob;
                    job.count = 1;
                    if (!comp.parent.Spawned)
                    {
                        JumpToToil(toil_JumpPoint);
                        return;
                    }
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

                toil_JumpPoint.initAction = delegate ()
                {
                    Pawn actor = toil_JumpPoint.actor;
                    Job job = actor.CurJob;
                    TargetThingB.Position = TargetThingA.Position;
                };
                yield return toil_JumpPoint;

                foreach (Toil forCarry in comp.CarryTarget(this, TargetIndex.A, TargetIndex.C))
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
                    comp.SwapTargetPart();
                };
                yield return toil
                    .FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell)
                    .PlaySoundAtEnd(SoundDefOf.Building_Complete);
            }
            else
            {
                this.EndJobWith(JobCondition.Incompletable);
            }
            yield break;
        }
    }
}
