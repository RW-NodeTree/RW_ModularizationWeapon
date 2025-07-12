#!pwsh

dotnet build -c ReleaseV13WithDoc
dotnet build -c ReleaseV14WithDoc
dotnet build -c ReleaseV15WithDoc
dotnet build -c ReleaseV16WithDoc
if(Test-Path ./1.3/Assemblies/RW_ModularizationWeapon.pdb)
{
    Remove-Item -Force ./1.3/Assemblies/RW_ModularizationWeapon.pdb
}
if(Test-Path ./1.4/Assemblies/RW_ModularizationWeapon.pdb)
{
    Remove-Item -Force ./1.4/Assemblies/RW_ModularizationWeapon.pdb
}
if(Test-Path ./1.5/Assemblies/RW_ModularizationWeapon.pdb)
{
    Remove-Item -Force ./1.5/Assemblies/RW_ModularizationWeapon.pdb
}
if(Test-Path ./1.6/Assemblies/RW_ModularizationWeapon.pdb)
{
    Remove-Item -Force ./1.6/Assemblies/RW_ModularizationWeapon.pdb
}