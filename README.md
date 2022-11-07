# RW_ModularizationWeapon
**A mod for the game Rimworld, base by RW_NodeTree.**

**Allow to make the weapon consist by multi part**

1 clone RW_NodeTree
``` bash
git clone https://github.com/RW-NodeTree/RW_NodeTree.git
```

2 set environment variable <kbd>**RW_HOME**</kbd> to your RimWorld root. For example:
``` bash
export RW_HOME="D:\SteamLibrary\steamapps\common\RimWorld"
```

3 open RW_ModularizationWeapon/RW_ModularizationWeapon.csproj, and replace all path perfix with <kbd>**"D:\SteamLibrary\steamapps\common\RimWorld"**</kbd> to yuor <kbd>**RimWorld root**</kbd>. For example:

**from**

``` xml
<Reference Include="Assembly-CSharp">
    <HintPath>D:\SteamLibrary\steamapps\common\RimWorld\RimWorldWin64_Data\Managed\Assembly-CSharp.dll</HintPath>
</Reference>
```
**to**

``` xml
<Reference Include="Assembly-CSharp">
    <HintPath>E:\RimWorld\RimWorldWin64_Data\Managed\Assembly-CSharp.dll</HintPath>
</Reference>
```

4 open RW_ModularizationWeapon/RW_ModularizationWeapon.csproj, and replace all path perfix with <kbd>**"D:\SteamLibrary\steamapps\workshop\content\294100"**</kbd> to yuor <kbd>**RimWorld SteamLibrary WorkShop Root**</kbd>.

**from**

``` xml
<Reference Include="0Harmony">
    <HintPath>D:\SteamLibrary\steamapps\workshop\content\294100\2009463077\Current\Assemblies\0Harmony.dll</HintPath>
</Reference>
```
**to**

``` xml
<Reference Include="0Harmony">
    <HintPath>E:\SteamLibrary\steamapps\workshop\content\294100\2009463077\Current\Assemblies\0Harmony.dll</HintPath>
</Reference>
```
> **Content :**
> - [Create Weapon](Docs/Create_Weapon.md)
> - [Create Part](Docs/Create_Part.md)
> - [Create Sub Weapon](Docs/Create_Sub_Weapon.md)

## Base Usage

