# RW_ModularizationWeapon
**A mod for the game Rimworld, base by RW_NodeTree.**

**Allow to make the weapon consist by multi part**

1 clone RW_NodeTree
``` bash
git clone https://github.com/RW-NodeTree/RW_NodeTree.git
```

2 set environment variable `RW_HOME` to your `RimWorld root`. For example:
``` bash
export RW_HOME="D:\SteamLibrary\steamapps\common\RimWorld"
```

3 Replace all path perfix with `"D:\SteamLibrary\steamapps\common\RimWorld"` to yuor `RimWorld root` in `RW_ModularizationWeapon/RW_ModularizationWeapon.csproj`. For example:

**from**

``` xml
<Reference Include="Assembly-CSharp">
    <HintPath>D:\SteamLibrary\steamapps\common\RimWorld\RimWorldWin64_Data\Managed\Assembly-CSharp.dll</HintPath>
</Reference>
```
**to**

``` xml
<Reference Include="Assembly-CSharp">
    <HintPath>(Your RimWorld root)\RimWorldWin64_Data\Managed\Assembly-CSharp.dll</HintPath>
</Reference>
```

4 Replace all path perfix with `"D:\SteamLibrary\steamapps\workshop\content\294100"` to yuor `RimWorld SteamLibrary WorkShop Root` in `RW_ModularizationWeapon/RW_ModularizationWeapon.csproj`.

**from**

``` xml
<Reference Include="0Harmony">
    <HintPath>D:\SteamLibrary\steamapps\workshop\content\294100\2009463077\Current\Assemblies\0Harmony.dll</HintPath>
</Reference>
```
**to**

``` xml
<Reference Include="0Harmony">
    <HintPath>(Your RimWorld SteamLibrary WorkShop Root)\2009463077\Current\Assemblies\0Harmony.dll</HintPath>
</Reference>
```
---
## Content :
> - [Create Weapon](Docs/Create_Weapon.md)
> - [Create Part](Docs/Create_Part.md)
> - [Create Sub Weapon](Docs/Create_Sub_Weapon.md)

## Base Usage

