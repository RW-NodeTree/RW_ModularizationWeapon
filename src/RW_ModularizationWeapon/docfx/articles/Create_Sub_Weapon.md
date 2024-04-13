# Create Sub Weapon

Before we create sub weapon, we need create a parent weapon. So, let's start create a parent weapon basing on `templateGun` from chapter '[Create Attachment Or Weapon With Attachments](Create_Attachment_Or_Weapon_With_Attachments.md)':

``` xml
<?xml version="1.0" encoding="utf-8" ?>
<Defs>
    <ThingDef ParentName="BaseHumanMakeableGun">
        <defName>templateGun</defName>
        ...
        <comps>
            ...
            <li Class="RW_ModularizationWeapon.CompProperties_ModularizationWeapon">
                ...
                <!-- it will set random part when weapon not create by crafting -->
                <setRandomPartWhenCreate>true</setRandomPartWhenCreate>
                <!-- declare the attach point for this weapon -->
                <attachmentProperties>
                    ...
                    <li>
                        <filter>
                            <thingDefs>
                                ...
                                <li>templateSubWeapon</li>
                                ...
                            </thingDefs>
                        </filter>
                        <!-- ramdom attachment generate weights,  -->
                        <randomThingDefWeights>
                            ...
                            <!-- <defname>(intger)</defname> -->
                            <templateSubWeapon>1</templateSubWeapon>
                            ...
                        </randomThingDefWeights>
                        ...
                    </li>
                    ...
                </attachmentProperties>
                ...
            </li>
            ...
        </comps>
    </ThingDef>
</Defs>
```


### The `notUseTools` Parameter

When defining sub-weapons or attachments, it is important to ensure that the `notUseTools` parameter is not set to `true`. If this parameter set to `true`, would prevent the sub-weapon's tool properties (i.e., melee weapon attributes) from being correctly utilized by the main weapon (`templateGun` in this case).

By keeping `notUseTools` as `false` (which is its default value), you ensure that the main weapon can correctly access and use the sub-weapon's melee attributes when needed.

### Example of `attachmentProperties` with `notUseTools`

```xml
<attachmentProperties>
    <li>
        <id>templateSubWeapon</id>
        <name>template SubWeapon</name>
        <!-- Other properties -->
        <notUseTools>false</notUseTools> <!-- Ensure this is set to false -->
        <!-- Rest of the attachment properties -->
    </li>
</attachmentProperties>
```

## The `notUseVerbProperties` Parameter

Similarly, the `notUseVerbProperties` parameter is just as important as `notUseTools`. It determines whether the main weapon can utilize the sub-weapon's verb properties (i.e., ranged weapon attributes). By default, `notUseVerbProperties` is also `false`, allowing for the seamless integration of sub-weapon properties into the main weapon.

### Example of `attachmentProperties` with `notUseVerbProperties`

```xml
<attachmentProperties>
    <li>
        <!-- Attachment properties -->
        <notUseVerbProperties>false</notUseVerbProperties> <!-- Ensure this is set to false -->
    </li>
</attachmentProperties>
```

Everyting prepare, time to create the sub weapon.

The sub weapon is very similar with the parent weapon. The only different between the parent weapon is we need add some parameters to CompProperties.

## The `notAllowParentUseTools` Parameter

The `notAllowParentUseTools` parameter is a boolean flag that dictates whether a parent weapon should have access to the tool properties(i.e., melee weapon attributes) of its child components.  By default, this parameter is set to `false`. Setting this to `true` would restrict the parent weapon from utilizing the tool properties(i.e., melee weapon attributes).

### Example of `CompProperties` with `notAllowParentUseTools`

```xml
<comps>
    ...
    <li Class="RW_ModularizationWeapon.CompProperties_ModularizationWeapon">
        <!-- Other properties -->
        <notAllowParentUseTools>false</notAllowParentUseTools> <!-- Ensure this is set to false -->
        <!-- Rest of the attachment properties -->
    </li>
</comps>
```

## The `notAllowParentUseVerbProperties` Parameter

The `notAllowParentUseVerbProperties` parameter is a boolean flag that dictates whether a parent weapon should have access to the verb properties (i.e., ranged weapon attributes) of its child components.  By default, this parameter is set to `false`. Setting this to `true` would restrict the parent weapon from utilizing the verb properties (i.e., ranged weapon attributes).

### Example of `CompProperties` with `notAllowParentUseVerbProperties`

```xml
<comps>
    ...
    <li Class="RW_ModularizationWeapon.CompProperties_ModularizationWeapon">
        <!-- Other properties -->
        <notAllowParentUseVerbProperties>false</notAllowParentUseVerbProperties> <!-- Ensure this is set to false -->
        <!-- Rest of the attachment properties -->
    </li>
</comps>
```

<!-- ## Next: [Create Attachment Or Weapon With Attachments](Create_Attachment_Or_Weapon_With_Attachments.md) -->