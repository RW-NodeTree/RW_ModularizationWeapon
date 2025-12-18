using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class ModularizationWeapon
    {

        internal static List<CompProperties> PreInitComps(Thing thing, ref bool needExitLock, ref bool publicPropertiesNeedExitLock, ref bool protectedPropertiesNeedExitLock)
        {
            ModularizationWeapon? weapon = thing as ModularizationWeapon;
            if (weapon != null)
            {
                needExitLock = !weapon.readerWriterLockSlim.IsWriteLockHeld;
                if(needExitLock) weapon.readerWriterLockSlim.EnterWriteLock();
                WeaponProperties publicProperties = weapon.PublicProperties;
                publicPropertiesNeedExitLock = publicProperties.PreInitComps();
                var props = weapon.ProtectedProperties;
                if (props.Count > weapon.currentWeaponMode)
                {
                    WeaponProperties protectedProperties = props[(int)weapon.currentWeaponMode];
                    protectedPropertiesNeedExitLock = protectedProperties.PreInitComps();
                }
            }
            return CompPropertiesFromThing(thing);
        }

        internal static List<ThingComp> RestoreComps(List<ThingComp> next, ThingWithComps thing)
        {
            ModularizationWeapon? weapon = thing as ModularizationWeapon;
            if (weapon != null && weapon.swap)
            {
                WeaponProperties publicProperties = weapon.PublicProperties;
                publicProperties.RestoreComps(next);
                var props = weapon.ProtectedProperties;
                if (props.Count > weapon.currentWeaponMode)
                {
                    WeaponProperties protectedProperties = props[(int)weapon.currentWeaponMode];
                    protectedProperties.RestoreComps(next);
                }
            }
            return next;
        }

        
        internal static void FinalInitComps(Thing thing, bool needExitLock, bool publicPropertiesNeedExitLock, bool protectedPropertiesNeedExitLock)
        {
            ModularizationWeapon? weapon = thing as ModularizationWeapon;
            if(weapon != null)
            {
                int errorCount = 0;
                try
                {
                    if (needExitLock) weapon.readerWriterLockSlim.ExitWriteLock();
                }
                catch(Exception ex)
                {
                    errorCount++;
                    Log.Error(ex.ToString());
                }
                try
                {
                    weapon.PublicProperties.FinalInitComps(publicPropertiesNeedExitLock);
                }
                catch(Exception ex)
                {
                    errorCount++;
                    Log.Error(ex.ToString());
                }
                try
                {
                    var props = weapon.ProtectedProperties;
                    if (props.Count > weapon.currentWeaponMode)
                    {
                        props[(int)weapon.currentWeaponMode].FinalInitComps(protectedPropertiesNeedExitLock);
                    }
                }
                catch(Exception ex)
                {
                    errorCount++;
                    Log.Error(ex.ToString());
                }
                if (errorCount > 0)
                {
                    throw new Exception($"catch {errorCount} errors at FinalInitComps");
                }
            }
        }

        
    }
}
