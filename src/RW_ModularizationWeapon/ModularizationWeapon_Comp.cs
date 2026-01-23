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

        internal List<CompProperties> PreInitComps(ref bool needExitLock, ref bool publicPropertiesNeedExitLock, ref bool equippablePropertiesNeedExitLock, ref bool inheritablePropertiesNeedExitLock)
        {
            needExitLock = !readerWriterLockSlim.IsWriteLockHeld;
            if(needExitLock) readerWriterLockSlim.EnterWriteLock();
            WeaponProperties privateProperties = PrivateProperties;
            publicPropertiesNeedExitLock = privateProperties.PreInitComps();
            var props = InheritableProperties;
            if (Props.equippable && props.Count > CurrentMode)
            {
                props[(int)CurrentMode].PreInitComps(ref equippablePropertiesNeedExitLock, ref inheritablePropertiesNeedExitLock);
            }
            return CompPropertiesFromThing(CurrentMode);
        }

        internal static List<ThingComp> RestoreComps(List<ThingComp> next, List<ThingComp>? prve, ThingWithComps thing)
        {
            ModularizationWeapon? weapon = thing as ModularizationWeapon;
            if (weapon != null)
            {
                if (prve != null)
                {
                    prve.RemoveAll(x => weapon.def.comps.FirstIndexOf(y => y == x.props) <  0);
                    weapon.def.comps.RemoveAll(x => prve.FirstIndexOf(y => x == y.props) >= 0);
                    next = prve;
                }
                WeaponProperties privateProperties = weapon.PrivateProperties;
                privateProperties.RestoreComps(next);
                var props = weapon.InheritableProperties;
                if (weapon.Props.equippable && props.Count > weapon.CurrentMode)
                {
                    props[(int)weapon.CurrentMode].RestoreComps(next);
                }
            }
            return next;
        }

        
        internal void FinalInitComps(List<ThingComp> comps, bool needExitLock, bool publicPropertiesNeedExitLock, bool equippablePropertiesNeedExitLock, bool inheritablePropertiesNeedExitLock)
        {
            int errorCount = 0;
            var props = InheritableProperties;
            if (Props.equippable && props.Count > CurrentMode)
            {
                try
                {
                    props[(int)CurrentMode].FinalInitComps(comps, equippablePropertiesNeedExitLock, inheritablePropertiesNeedExitLock);
                }
                catch(Exception ex)
                {
                    errorCount++;
                    Log.Error(ex.ToString());
                }
            }
            try
            {
                PrivateProperties.FinalInitComps(comps, publicPropertiesNeedExitLock);
            }
            catch(Exception ex)
            {
                errorCount++;
                Log.Error(ex.ToString());
            }
            try
            {
                if (needExitLock) readerWriterLockSlim.ExitWriteLock();
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
