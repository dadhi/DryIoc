@echo off

dotnet restore

dotnet build

dotnet test ".\test\DryIoc.Microsoft.DependencyInjection.Specification.Tests"

dotnet pack ".\src\DryIoc.Microsoft.DependencyInjection" -c Release -o "..\bin\NuGetPackages"

pause