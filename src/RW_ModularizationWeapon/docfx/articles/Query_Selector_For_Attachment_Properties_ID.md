# Query Selector For Attachment Properties ID
## Before we start you must know **[previous chapter](Affect_Weapon_By_Attachment.md)**

## Writing Query Selectors for `<attachmentProperties>` IDs

In our framework, you can define a query selector for the `<id>` of an item within `<attachmentProperties>`. The logic and syntax of these selectors are reminiscent of **CSS** and **XPATH**. Let's start with an example that includes `<attachmentProperties>`:

```xml
<comps>
    <li Class="RW_ModularizationWeapon.CompProperties_ModularizationWeapon">
        <attachmentProperties>
            <!-- Attachment properties list -->
        </attachmentProperties>
    </li>
</comps>
```

### Example of Modifying Attachment Properties

Here's an example that modifies all properties of a matched attachment point:

```xml
<comps>
    <li Class="RW_ModularizationWeapon.CompProperties_ModularizationWeapon">
        <attachmentProperties>
            <!-- Previous Attachment Points -->
            <li>
                <id>templatePart1:templatePartB</id>
                <!-- Property modifications -->
            </li>
            <!-- More Attachment Points -->
        </attachmentProperties>
    </li>
</comps>
```

Note the unique syntax in the `<id>` property, which is used to query the Attachment Node Tree. The `id` text checks if the attachment holder has a `thingDef` equal to **"templatePartB"** at the attachment point with the ID **"templatePart1"**. If the query selector finds a match, it replaces all properties of the attachment point with the ID **"templatePart1"**.

### Optional Attachment Properties

It's common to replace only a small part of an attachment point. In our framework, you can mark an attachment point as *Optional* by using the class `RW_ModularizationWeapon.OptionalWeaponAttachmentProperties`, as shown below:

```xml
<comps>
    <li Class="RW_ModularizationWeapon.CompProperties_ModularizationWeapon">
        <attachmentProperties>
            <!-- Previous Attachment Points -->
            <li Class="RW_ModularizationWeapon.OptionalWeaponAttachmentProperties">
                <id>!templatePart1:templatePartC</id>
                <!-- Property modifications -->
            </li>
            <!-- More Attachment Points -->
        </attachmentProperties>
    </li>
</comps>
```

This will only replace the rotation value to `(0,45,0)` if **"templatePartC"** is attached to **"templatePart1"** and does not exist.

### Querying Parent or Child Nodes

To check the parent or child of the attachment node tree, you can use child and parent conditions:

```xml
<comps>
    <li Class="RW_ModularizationWeapon.CompProperties_ModularizationWeapon">
        <attachmentProperties>
            <!-- Previous Attachment Points -->
            <li Class="RW_ModularizationWeapon.OptionalWeaponAttachmentProperties">
                <id>templatePart1:templatePartD[:[templatePart2:templatePartB]/:/:]</id>
                <!-- Property modifications -->
            </li>
            <!-- More Attachment Points -->
        </attachmentProperties>
    </li>
</comps>
```

The query selector means "when **"templatePartB"** is attached to **"templatePart2"** and **"templatePartD"** is attached to **"templatePart1"**". If this condition is met, the rotation value will be replaced with `(0,-45,0)`.

### Match Scores for Replacement Order

The query selector also has match scores to determine the order of replacements. For example:

```xml
<comps>
    <li Class="RW_ModularizationWeapon.CompProperties_ModularizationWeapon">
        <attachmentProperties>
            <!-- Previous Attachment Points -->
            <li Class="RW_ModularizationWeapon.OptionalWeaponAttachmentProperties">
                <id>templatePart1:templatePartD</id>
                <!-- Property modifications -->
            </li>
            <li Class="RW_ModularizationWeapon.OptionalWeaponAttachmentProperties">
                <id>templatePart1:templatePartD[templatePartD_Slot:Sth]</id>
                <!-- Property modifications -->
            </li>
            <!-- More Attachment Points -->
        </attachmentProperties>
    </li>
</comps>
```

In this example, if **"templatePart1:templatePartD"** matches but **"templatePart1:templatePartD[templatePartD_Slot:Sth]"** does not, the rotation will be replaced with `(0,45,0)`. However, if **"templatePart1:templatePartD[templatePartD_Slot:Sth]"** also matches, the rotation will be replaced with `(0,-45,0)` because the second query has more conditions and thus a higher match score, resulting in a later replacement order.