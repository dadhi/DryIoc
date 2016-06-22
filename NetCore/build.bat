@echo off

dotnet restore

dotnet build

dotnet test ".\test\DryIoc.AspNetCore.DependencyInjection.Specification.Tests"
dotnet test ".\test\DryIoc.NetCore.UnitTests"

dotnet pack ".\src\DryIoc.AspNetCore.DependencyInjection" -c Release -o "..\bin\NuGetPackages"

pause