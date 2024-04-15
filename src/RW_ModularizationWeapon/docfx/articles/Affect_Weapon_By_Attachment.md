# Affect Weapon By Attachment
## Before we start you must know **[previous chapter](Create_Attachment.md)**

To affect the weapon performent, we need set some **multiplier** or **offseter** value in comp. Here are some common seting for attachment:

## Stat
**final = original * Multiplier + Offest**
### Offest
- `statOffset`: Alters `StatWorker.GetValue` results with addition.
- `statOffsetDefaultValue`: Default value for `statOffset` when no stat is
### Multiplier
- `statMultiplier`: Multiplies `StatWorker.GetValue` results.
- `statMultiplierDefaultValue`: Default value for `statMultiplier` when no stat is provided.
### Usage Example
```xml
<statOffset>
    <!-- Just write as statBases -->
    <AccuracyTouch>0.8</AccuracyTouch><!-- Parent weapon AccuracyTouch +0.8  -->
    <AccuracyShort>0.9</AccuracyShort><!-- Parent weapon AccuracyShort +0.9  -->
</statOffset>
<statOffsetDefaultValue>0.1</statOffsetDefaultValue><!-- let other stat value +0.1 -->
<statMultiplier>
    <!-- Just write as statBases -->
    <AccuracyMedium>1.1</AccuracyMedium><!-- Parent weapon AccuracyMedium *1.1  -->
    <AccuracyLong>1.1</AccuracyLong><!-- Parent weapon AccuracyLong *1.1  -->
</statMultiplier>
<statMultiplierDefaultValue>0.9</statMultiplierDefaultValue><!-- let other stat value *0.9 -->
```
## VerbProperties And Tools
**finalNumber = originalNumber * Multiplier + Offest**

**finalBoolean = originalBoolean && AndPatchs || OrPatchs**

**finaleObject = InstPatchs ?? finaleObject**

---
### Offest
- `verbPropertiesOffseter`: Modifies `IVerbOwner.VerbProperties` values with addition.

- `toolsOffseter`: Modifies `IVerbOwner.Tools` values with addition.
### Multiplier
- `verbPropertiesOtherPartOffseterAffectHorizon`: Defines the impact of other parts' `verbPropertiesOffseter` on `IVerbOwner.VerbProperties`.

- `toolsMultiplier`: Applies multiplier calculations to `IVerbOwner.Tools` values.

*[Type : FieldReaderDgitList&lt;T&gt;](../api/RW_ModularizationWeapon.FieldReaderDgitList-1.html#RW_ModularizationWeapon_FieldReaderDgitList_1_LoadDataFromXmlCustom_System_Xml_XmlNode_)*

*[Type : FieldReaderDigit&lt;T&gt;](../api/RW_ModularizationWeapon.FieldReaderDigit-1.html#RW_ModularizationWeapon_FieldReaderDigit_1_LoadDataFromXmlCustom_System_Xml_XmlNode_)*

---
### AndPatchs
- `verbPropertiesBoolAndPatch`: Performs boolean and calculations with `IVerbOwner.VerbProperties`.

- `toolsBoolAndPatch`: Performs boolean and calculations with `IVerbOwner.Tools`.
### OrPatchs
- `verbPropertiesBoolOrPatch`: Performs boolean OR calculations with `IVerbOwner.VerbProperties`.

- `toolsBoolOrPatch`: Performs boolean OR calculations with `IVerbOwner.Tools`.

*[Type : FieldReaderBoolList&lt;T&gt;](../api/RW_ModularizationWeapon.FieldReaderBoolList-1.html#RW_ModularizationWeapon_FieldReaderBoolList_1_LoadDataFromXmlCustom_System_Xml_XmlNode_)*

*[Type : FieldReaderBoolean&lt;T&gt;](../api/RW_ModularizationWeapon.FieldReaderBoolean-1.html#RW_ModularizationWeapon_FieldReaderBoolean_1_LoadDataFromXmlCustom_System_Xml_XmlNode_)*

---

### InstPatchs
- `verbPropertiesObjectPatch`: Replaces parameters in `IVerbOwner.VerbProperties` from the parent.

- `toolsObjectPatch`: Replaces parameters in `IVerbOwner.Tools` from the parent.

*[Type : FieldReaderInstList&lt;T&gt;](../api/RW_ModularizationWeapon.FieldReaderInstList-1.html#RW_ModularizationWeapon_FieldReaderInstList_1_LoadDataFromXmlCustom_System_Xml_XmlNode_)*

*[Type : FieldReaderInstance&lt;T&gt;](../api/RW_ModularizationWeapon.FieldReaderInstance-1.html#RW_ModularizationWeapon_FieldReaderInstance_1_LoadDataFromXmlCustom_System_Xml_XmlNode_)*

---
### Usage Example
```xml
<!-- Adjusts the starting values for verb properties with a slight margin -->
<verbPropertiesOffseter Default="0.2">
    <!-- General default, set to a baseline zero -->
    <li Default="0">
        <!-- Basic setup for actions, like a 0.5-second prep before getting started -->
        <warmupTime>0.5</warmupTime>
    </li>
    <!-- Special case for CombatExtended weapons, increase 0.5 recoil amount -->
    <li Reader-Class="CombatExtended.VerbPropertiesCE">
        <recoilAmount>0.5</recoilAmount>
    </li>
</verbPropertiesOffseter>

<!-- Gives a moderate boost to verb properties -->
<verbPropertiesMultiplier Default="1.1">
    <li Default="1">
        <!-- A bit more time to get ready, bumped up by 20% -->
        <warmupTime>1.2</warmupTime>
        <!-- Range gets a modest increase, just a bit further than before -->
        <range>1.25</range>
    </li>
    <li Reader-Class="CombatExtended.VerbPropertiesCE">
        <!-- Indirect fire gets a slight edge, 20% more challenging -->
        <indirectFirePenalty>1.2</indirectFirePenalty>
        <!-- Ammo consumption doubles up, making each shot count -->
        <ammoConsumedPerShotCount>2</ammoConsumedPerShotCount>
    </li>
</verbPropertiesMultiplier>
<!-- replace value and instance -->
<verbPropertiesObjectPatch>
    <li>
        <!-- dgital -->
        <burstShotCount>10</burstShotCount>
        <!-- ThingDef -->
        <defaultProjectile>Bullet_AssaultRifleLongBarrel</defaultProjectile>
        <!-- SoundDef -->
        <soundCast>Shot_Silenced</soundCast>
        <!-- SoundDef, but set to null -->
        <soundCastTail IsNull="true"/>
    </li>
    <li Reader-Class="CombatExtended.VerbPropertiesCE">
        <!-- enum RecoilPattern -->
        <recoilPattern>Regular</recoilPattern>
    </li>
</verbPropertiesObjectPatch>
<!-- for tool, other parameters like Multiplier,ObjectPatch are same as verbProperties version -->
<toolsOffseter Default="0.1">
    <!-- General default, set to a baseline zero -->
    <li Default="0">
        <!-- increase 2 attack power -->
        <power>2</power>
    </li>
    <!-- Special case for CombatExtended weapons, increase 0.5 blunt armor penetration -->
    <li Reader-Class="CombatExtended.ToolCE">
        <armorPenetrationBlunt>3</armorPenetrationBlunt>
    </li>
</toolsOffseter>
```

## List Of Affecter


### Offset
#### Parent Offsets
- `verbPropertiesOffseter`: Modifies `IVerbOwner.VerbProperties` values with addition.

- `toolsOffseter`: Modifies `IVerbOwner.Tools` values with addition.

- `compPropertiesOffseter`: Modifies `ThingDef.Comps` values with addition.

- `statOffset`: Alters `StatWorker.GetValue` results with addition.

- `statOffsetDefaultValue`: Default value for `statOffset` when no stat is provided.

#### Other Part Offsets
- `verbPropertiesOtherPartOffseterAffectHorizon`: Defines the impact of other parts' `verbPropertiesOffseter` on `IVerbOwner.VerbProperties`.

- `toolsOtherPartOffseterAffectHorizon`: Defines the impact of other parts' `toolsOffseter` on `IVerbOwner.Tools`.

- `statOtherPartOffseterAffectHorizon`: Influences how other parts' `statOffset` affects `StatWorker.GetValue`.

- `statOtherPartOffseterAffectHorizonDefaultValue`: Default value for `statOtherPartOffseterAffectHorizon` when no stat is provided.

---
### Multiplier
#### Parent Multipliers
- `verbPropertiesMultiplier`: Applies multiplier calculations to `IVerbOwner.VerbProperties` values.

- `toolsMultiplier`: Applies multiplier calculations to `IVerbOwner.Tools` values.

- `compPropertiesMultiplier`: Applies multiplier calculations to `ThingDef.Comps` values.

- `statMultiplier`: Multiplies `StatWorker.GetValue` results.

- `statMultiplierDefaultValue`: Default value for `statMultiplier` when no stat is provided.

#### Other Part Multipliers
- `verbPropertiesOtherPartMultiplierAffectHorizon`: Determines how other parts' `verbPropertiesMultiplier` affects `IVerbOwner.VerbProperties`.

- `toolsOtherPartMultiplierAffectHorizon`: Determines how other parts' `toolsMultiplier` affects `IVerbOwner.Tools`.

- `statOtherPartMultiplierAffectHorizon`: Influences how other parts' `statMultiplier` affects `StatWorker.GetValue`.

- `statOtherPartMultiplierAffectHorizonDefaultValue`: Default value for `statOtherPartMultiplierAffectHorizon` when no stat is provided.

---
### AndPatchs
#### Parent AndPatchs
- `verbPropertiesBoolAndPatch`: Performs boolean and calculations with `IVerbOwner.VerbProperties`.

- `toolsBoolAndPatch`: Performs boolean and calculations with `IVerbOwner.Tools`.

- `compPropertiesBoolAndPatch`: Performs boolean and calculations with `ThingDef.Comps`.

#### Other Part AndPatchs
- `verbPropertiesBoolAndPatchByOtherPart`: Performs boolean and calculations with this part's `verbPropertiesBoolAndPatch`.

- `toolsBoolAndPatchByOtherPart`: Performs boolean and calculations with this part's `toolsBoolAndPatch`.

---
### OrPatchs
#### Parent OrPatchs
- `verbPropertiesBoolOrPatch`: Performs boolean OR calculations with `IVerbOwner.VerbProperties`.

- `toolsBoolOrPatch`: Performs boolean OR calculations with `IVerbOwner.Tools`.

- `compPropertiesBoolOrPatch`: Performs boolean OR calculations with `ThingDef.Comps`.

#### Other Part OrPatchs
- `verbPropertiesBoolOrPatchByOtherPart`: Performs boolean OR calculations with this part's `verbPropertiesBoolOrPatch`.

- `toolsBoolOrPatchByOtherPart`: Performs boolean OR calculations with this part's `toolsBoolOrPatch`.

---
### InstPatchs
#### Parent InstPatchs
- `verbPropertiesObjectPatch`: Replaces parameters in `IVerbOwner.VerbProperties` from the parent.

- `toolsObjectPatch`: Replaces parameters in `IVerbOwner.Tools` from the parent.

- `compPropertiesObjectPatch`: Replaces parameters in `ThingDef.Comps` from the parent.

#### Other Part InstPatchs
- `verbPropertiesObjectPatchByOtherPart`: Defines which parameters can replace this part's `IVerbOwner.VerbProperties`.

- `toolsObjectPatchByOtherPart`: Defines which parameters can replace this part's `IVerbOwner.Tools`.

### *All parameters : [Type : CompProperties_ModularizationWeapon](../api/RW_ModularizationWeapon.CompProperties_ModularizationWeapon.html)*
<!-- ## Next: [Create Attachment Or Weapon With Attachments](Create_Attachment_Or_Weapon_With_Attachments.md) -->