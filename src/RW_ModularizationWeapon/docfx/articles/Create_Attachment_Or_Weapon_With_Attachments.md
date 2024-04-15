# Create Weapon
## Before we start you must know **[previous chapter](Create_Attachment.md)**

Before diving into the creation process, it's essential to understand how to create an attachment in the game's modding system.

## Step 1: Crafting the `templateGun`

Let's begin by crafting a weapon named `templateGun`. This weapon will serve as the base for our customizations and attachments.

```xml
<Defs>
    <!-- Weapon Definition Starts Here -->
    <ThingDef ParentName="BaseHumanMakeableGun">
        <defName>templateGun</defName>
        <!-- Additional Properties -->
        <comps>
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
                <!-- Attachment Properties Will Be Defined Here -->
            </li>
        </comps>
        <!-- More Properties -->
    </ThingDef>
    <!-- Weapon Definition Ends Here -->
</Defs>
```

## Step 2: Configuring Attachment Properties

This is where we define the `attachmentProperties` that allow our weapon to accept and utilize attachments.

### Attachment Properties Explained

- `<id>`: A unique identifier for the attachment point.
- `<name>`: The name of the attachment point, displayed in the game.
- `<position>`: The in-world position offset of the attachment relative to the weapon.
- `<rotation>`: The rotation offset in Euler angles for the attachment.
- `<scale>`: The size scale of the attachment relative to its default size.
- `<defaultThing>`: The default part that is attached to this point when the weapon is not crafted.
- `<allowEmpty>`: A boolean that determines if the attachment point can be left empty.
- `<positionInPixelSize>`: A flag to indicate if the position offset should be interpreted in pixels.
- `<filter>`: A set of conditions that determine which items can be attached to this point.
- `<randomThingDefWeights>`: A list of weights for random attachment generation.

*More parameters here: [Type : WeaponAttachmentProperties](../api/RW_ModularizationWeapon.WeaponAttachmentProperties.html)*
### Example Attachment Properties

```xml
<comps>
    <li Class="RW_ModularizationWeapon.CompProperties_ModularizationWeapon">
        <attachmentProperties>
            <li>
                <id>templatePart</id>
                <name>template Part</name>
                <position>(0,0.1,0)</position>
                <rotation>(0,0,0)</rotation>
                <scale>(1,1,1)</scale>
                <defaultThing>templatePart</defaultThing>
                <allowEmpty>true</allowEmpty>
                <positionInPixelSize>false</positionInPixelSize>
                <filter>
                    <thingDefs>
                        <li>templatePart</li>
                        <!-- More Attachment Definitions -->
                    </thingDefs>
                </filter>
                <randomThingDefWeights>
                    <templatePart>1</templatePart>
                    <!-- More Weight Definitions -->
                </randomThingDefWeights>
            </li>
            <!-- More Attachment Points -->
        </attachmentProperties>
    </li>
</comps>
```

By following the steps above, you will have a weapon or part that can install an attachment named `templatePart` at the attachment point `template Part`. This modular design allows for a high degree of customization and adaptability in the game.

## Next: [Create Sub Weapon](Create_Sub_Weapon.md)

