#!/bin/bash

dotnet build -c ReleaseV13
dotnet build -c ReleaseV14
dotnet build -c ReleaseV15
dotnet build -c ReleaseV16
if [ -f ./1.3/Assemblies/RW_ModularizationWeapon.pdb ]
then
    rm -f ./1.3/Assemblies/RW_ModularizationWeapon.pdb
fi
if [ -f ./1.4/Assemblies/RW_ModularizationWeapon.pdb ]
then
    rm -f ./1.4/Assemblies/RW_ModularizationWeapon.pdb
fi
if [ -f ./1.5/Assemblies/RW_ModularizationWeapon.pdb ]
then
    rm -f ./1.5/Assemblies/RW_ModularizationWeapon.pdb
fi
if [ -f ./1.6/Assemblies/RW_ModularizationWeapon.pdb ]
then
    rm -f ./1.6/Assemblies/RW_ModularizationWeapon.pdb
fi