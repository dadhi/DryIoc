@echo off

dotnet restore

dotnet build

dotnet test ".\test\DryIoc.AspNetCore.DependencyInjection.Specification.Tests"

dotnet pack ".\src\DryIoc.AspNetCore.DependencyInjection" -c Release -o "..\bin\NuGetPackages"

pause