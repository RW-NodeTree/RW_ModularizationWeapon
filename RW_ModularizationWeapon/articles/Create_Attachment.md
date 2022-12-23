# Create Attachment

**we need create the ThingDef like this :**

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
				<TextureSizeFactor>256</TextureSizeFactor>
				<TextureFilterMode>Point</TextureFilterMode>
				<ExceedanceFactor>2</ExceedanceFactor>
				<ExceedanceOffset>1</ExceedanceOffset>
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
**It will create attachment called `templatePart`.**

**Now, we can set this part able attach to other part or weapon.**

## Next: [Create Attachment Or Weapon Use Attachments](Create_Attachment_Or_Weapon_Use_Attachments.md)