# Create Attachment

**We need create the ThingDef like this :**

``` xml
<?xml version="1.0" encoding="utf-8" ?>
<Defs>
    ...
    <ThingDef>
        <defName>templatePart</defName>
        ...
        <comps>
            ...
            <!-- node tree dependence -->
            <li Class="RW_NodeTree.CompProperties_ChildNodeProccesser">
                <!-- Texture size factor -->
                <TextureSizeFactor>256</TextureSizeFactor>
                <!-- Texture filter mode -->
                <TextureFilterMode>Point</TextureFilterMode>
                <!-- Exceedance factor -->
                <ExceedanceFactor>2</ExceedanceFactor>
                <!-- Exceedance offset -->
                <ExceedanceOffset>1</ExceedanceOffset>
                <!-- Force node ID control -->
                <ForceNodeIdControl>true</ForceNodeIdControl>
            </li>
            <!-- weapon framework -->
            <li Class="RW_ModularizationWeapon.CompProperties_ModularizationWeapon">
                ...
            </li>
            ...
        </comps>
    </ThingDef>
</Defs>
```
It will create attachment called `templatePart`.

Now, we can set this part to be attachable to other parts or weapons.

## Next: [Create Attachment Or Weapon With Attachments](Create_Attachment_Or_Weapon_With_Attachments.md)