using CombatExtended;
using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace RW_ModularizationWeapon.CombatExtended
{
    public class CompAmmoUserDestructor : IThingCompDestructor
    {
        public void DestroyComp(ModularizationWeapon modularizationWeapon, ThingComp comp)
        {
            Map? map = modularizationWeapon.MapHeld;
            CompAmmoUser? compAmmoUser = comp as CompAmmoUser;
            if (compAmmoUser != null && map != null)
            {
                compAmmoUser.TryUnload(out Thing? ammo, true);
                if(ammo != null && ammo.Map != map)
                {
                    if(ammo.Spawned)
                    {
                        ammo.DeSpawn();
                    }
                    if(ammo.holdingOwner != null)
                    {
                        ammo.holdingOwner.Remove(ammo);
                    }
                    GenPlace.TryPlaceThing(ammo, modularizationWeapon.Position, map, ThingPlaceMode.Near);
                }
            }
        }
    }
}
