using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using Verse;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RW_ModularizationWeapon
{
    public partial class ModularizationWeapon
    {
        
        public FieldReaderInstList<VerbProperties> VerbPropertiesObjectPatch(string? childNodeIdForVerbProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return WeaponProperties.VerbPropertiesObjectPatch(childNodeIdForVerbProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public FieldReaderInstList<Tool> ToolsObjectPatch(string? childNodeIdForTool)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return WeaponProperties.ToolsObjectPatch(childNodeIdForTool, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public FieldReaderInstList<CompProperties> CompPropertiesObjectPatch(string? childNodeIdForCompProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return WeaponProperties.CompPropertiesObjectPatch(childNodeIdForCompProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public FieldReaderBoolList<VerbProperties> VerbPropertiesBoolAndPatch(string? childNodeIdForVerbProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return WeaponProperties.VerbPropertiesBoolAndPatch(childNodeIdForVerbProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public FieldReaderBoolList<Tool> ToolsBoolAndPatch(string? childNodeIdForTool)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return WeaponProperties.ToolsBoolAndPatch(childNodeIdForTool, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public FieldReaderBoolList<CompProperties> CompPropertiesBoolAndPatch(string? childNodeIdForCompProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return WeaponProperties.CompPropertiesBoolAndPatch(childNodeIdForCompProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public FieldReaderBoolList<VerbProperties> VerbPropertiesBoolOrPatch(string? childNodeIdForVerbProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return WeaponProperties.VerbPropertiesBoolOrPatch(childNodeIdForVerbProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public FieldReaderBoolList<Tool> ToolsBoolOrPatch(string? childNodeIdForTool)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return WeaponProperties.ToolsBoolOrPatch(childNodeIdForTool, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public FieldReaderBoolList<CompProperties> CompPropertiesBoolOrPatch(string? childNodeIdForCompProperties)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return WeaponProperties.CompPropertiesBoolOrPatch(childNodeIdForCompProperties, ChildNodes, GetOrGenCurrentPartAttachmentProperties());
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }
    }
}
