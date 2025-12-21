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

        internal static List<ThingComp> RestoreComps(List<ThingComp> next, List<ThingComp>? prve, ThingWithComps thing)
        {
            ModularizationWeapon? weapon = thing as ModularizationWeapon;
            if (weapon != null)
            {
                if (prve != null)
                {
                    prve.RemoveAll(x => weapon.def.comps.FirstIndexOf(y => y == x.props) < 0);
                    weapon.def.comps.RemoveAll(x => prve.FirstIndexOf(y => x == y.props) >= 0);
                    next = prve;
                }
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

        
        internal static void FinalInitComps(Thing thing, List<ThingComp> comps, bool needExitLock, bool publicPropertiesNeedExitLock, bool protectedPropertiesNeedExitLock)
        {
            ModularizationWeapon? weapon = thing as ModularizationWeapon;
            if(weapon != null)
            {
                int errorCount = 0;
                try
                {
                    var props = weapon.ProtectedProperties;
                    if (props.Count > weapon.currentWeaponMode)
                    {
                        props[(int)weapon.currentWeaponMode].FinalInitComps(comps, protectedPropertiesNeedExitLock);
                    }
                }
                catch(Exception ex)
                {
                    errorCount++;
                    Log.Error(ex.ToString());
                }
                try
                {
                    weapon.PublicProperties.FinalInitComps(comps, publicPropertiesNeedExitLock);
                }
                catch(Exception ex)
                {
                    errorCount++;
                    Log.Error(ex.ToString());
                }
                try
                {
                    if (needExitLock) weapon.readerWriterLockSlim.ExitWriteLock();
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

        internal class CompProperties_Equippable : CompProperties
        {
            public readonly uint mode;
            public CompProperties_Equippable(uint mode) : base(typeof(CompEquippable))
            {
                this.mode = mode;
            }
        }
    }
}
