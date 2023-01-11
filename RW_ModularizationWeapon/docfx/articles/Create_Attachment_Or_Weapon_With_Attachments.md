# Create Weapon
## Before we start you must know how to **[Create Attachment](Create_Attachment.md)**.
**Let's start create a weapon called `templateGun` :**

``` xml
<?xml version="1.0" encoding="utf-8" ?>
<Defs>
    <!-- Or use parentName BaseMeleeWeapon_Sharp_Quality or BaseMeleeWeapon_Blunt_Quality for melee weapon, or not use any ParentName for simple attachment -->
	<ThingDef ParentName="BaseHumanMakeableGun">
		<defName>templateGun</defName>
        ...
        <!-- your weapon verbs define -->
		<verbs>
            ...
			<li>
				<verbClass>Verb_Shoot</verbClass>
				<hasStandardCommand>true</hasStandardCommand>
				<defaultProjectile>Bullet_AssaultRifle</defaultProjectile>
				<warmupTime>1.0</warmupTime>
				<range>30.9</range>
				<ticksBetweenBurstShots>10</ticksBetweenBurstShots>
				<soundCast>Shot_AssaultRifle</soundCast>
				<soundCastTail>GunTail_Medium</soundCastTail>
				<muzzleFlashScale>9</muzzleFlashScale>
			</li>
            ...
		</verbs>
        ...
        <!-- your weapon melee define-->
		<tools>
            ...
			<li>
				<label>stock</label>
				<capacities>
					<li>Blunt</li>
				</capacities>
				<power>9</power>
				<cooldownTime>2</cooldownTime>
			</li>
            ...
		</tools>
		<comps>
            ...
            <!-- node tree dependence -->
			<li Class="RW_NodeTree.CompProperties_ChildNodeProccesser">
				<TextureSizeFactor>256</TextureSizeFactor>
				<TextureFilterMode>Point</TextureFilterMode>
				<ExceedanceFactor>2</ExceedanceFactor>
				<ExceedanceOffset>1</ExceedanceOffset>
				<ForceNodeIdControl>true</ForceNodeIdControl>
			</li>
            <!-- weapon framework -->
			<li Class="RW_ModularizationWeapon.CompProperties_ModularizationWeapon">
                ...
                <!-- it will set random part when weapon not create by crafting -->
				<setRandomPartWhenCreate>true</setRandomPartWhenCreate>
                <!-- declare the attach point for this weapon -->
				<attachmentProperties>
                    ...
				</attachmentProperties>
                ...
			</li>
            ...
		</comps>
	</ThingDef>
</Defs>
```
*More parameters here: [Type : CompProperties_ModularizationWeapon](../api/RW_ModularizationWeapon.CompProperties_ModularizationWeapon)*

**Then, set `attachmentProperties` to make weapon able to use attachment.**

**Here is an example to make a attachment point for our previous attachment `templatePart`**

```xml
...
<li>
    <!-- Indispensable parameter. it should able to use as xml node -->
    <id>templatePart</id>
    <!-- the name of this attachment -->
    <name>template Part</name>
    <!-- ICON of this attachment point -->
    <UITexPath>templatePart</UITexPath>
    <!-- attachment drawing postion -->
    <postion>(0,0.1,0)</postion>
    <!-- default part when create -->
    <defultThing>templatePart</defultThing>
    <!-- if is true, this point can set to empty -->
    <allowEmpty>true</allowEmpty>
    <!-- ThingFilter parameter, to set witch thing can attach on this point -->
    <filter>
        <thingDefs>
            ...
            <li>templatePart</li>
            ...
        </thingDefs>
    </filter>
    <!-- ramdom attachment generate weights,  -->
    <randomThingDefWeights>
        ...
        <!-- <defname>(intger)</defname> -->
        <templatePart>1</templatePart>
        ...
    </randomThingDefWeights>
    ...
</li>
...
```
*More parameters here: [Type : WeaponAttachmentProperties](../api/RW_ModularizationWeapon.WeaponAttachmentProperties)*

**And final, you will get a weapon or part that able to install an attachment called templatePart on attach point `template Part`**

