#!pwsh

dotnet build -c ReleaseV13
dotnet build -c ReleaseV14
dotnet build -c ReleaseV15
if(Test-Path ./1.3/Assemblies/RW_NodeTree.pdb)
{
    Remove-Item -Force ./1.3/Assemblies/RW_NodeTree.pdb
}
if(Test-Path ./1.4/Assemblies/RW_NodeTree.pdb)
{
    Remove-Item -Force ./1.4/Assemblies/RW_NodeTree.pdb
}
if(Test-Path ./1.5/Assemblies/RW_NodeTree.pdb)
{
    Remove-Item -Force ./1.5/Assemblies/RW_NodeTree.pdb
}