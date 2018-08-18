@echo off

dotnet restore

dotnet build

dotnet test ".\test\DryIoc.NetCore.UnitTests"
dotnet test ".\test\DryIoc.Microsoft.DependencyInjection.Specification.Tests"
rem dotnet test ".\test\DryIoc.Microsoft.DependencyInjection.Specification.Tests.v1.1"

dotnet pack ".\src\DryIoc.Microsoft.DependencyInjection" -c Release -o "..\bin\NuGetPackages"
dotnet pack ".\src\DryIoc.Microsoft.Hosting"             -c Release -o "..\bin\NuGetPackages

pause