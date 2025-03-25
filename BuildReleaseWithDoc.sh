#!/bin/bash

dotnet build -c ReleaseV13WithDoc
dotnet build -c ReleaseV14WithDoc
dotnet build -c ReleaseV15WithDoc
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