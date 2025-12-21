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

        internal List<CompProperties> PreInitComps(ref bool needExitLock, ref bool publicPropertiesNeedExitLock, ref bool protectedPropertiesNeedExitLock)
        {
            needExitLock = !readerWriterLockSlim.IsWriteLockHeld;
            if(needExitLock) readerWriterLockSlim.EnterWriteLock();
            WeaponProperties publicProperties = PublicProperties;
            publicPropertiesNeedExitLock = publicProperties.PreInitComps();
            var props = ProtectedProperties;
            if (props.Count > CurrentMode)
            {
                WeaponProperties protectedProperties = props[(int)CurrentMode];
                protectedPropertiesNeedExitLock = protectedProperties.PreInitComps();
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
                    prve.RemoveAll(x => weapon.def.comps.FirstIndexOf(y => y == x.props) < 0);
                    weapon.def.comps.RemoveAll(x => prve.FirstIndexOf(y => x == y.props) >= 0);
                    next = prve;
                }
                WeaponProperties publicProperties = weapon.PublicProperties;
                publicProperties.RestoreComps(next);
                var props = weapon.ProtectedProperties;
                if (props.Count > weapon.CurrentMode)
                {
                    WeaponProperties protectedProperties = props[(int)weapon.CurrentMode];
                    protectedProperties.RestoreComps(next);
                }
            }
            return next;
        }

        
        internal void FinalInitComps(List<ThingComp> comps, bool needExitLock, bool publicPropertiesNeedExitLock, bool protectedPropertiesNeedExitLock)
        {
            int errorCount = 0;
            try
            {
                var props = ProtectedProperties;
                if (props.Count > CurrentMode)
                {
                    props[(int)CurrentMode].FinalInitComps(comps, protectedPropertiesNeedExitLock);
                }
            }
            catch(Exception ex)
            {
                errorCount++;
                Log.Error(ex.ToString());
            }
            try
            {
                PublicProperties.FinalInitComps(comps, publicPropertiesNeedExitLock);
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
