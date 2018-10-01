@echo off

dotnet restore
if %ERRORLEVEL% neq 0 call :error "dotnet restore"

dotnet build
if %ERRORLEVEL% neq 0 call :error "dotnet build"

dotnet test ".\src\DryIoc.UnitTests"
REM dotnet test ".\test\DryIoc.Microsoft.DependencyInjection.Specification.Tests"
REM dotnet test ".\test\DryIoc.Microsoft.DependencyInjection.Specification.Tests.v1.1"
if %ERRORLEVEL% neq 0 call :error "dotnet test"

dotnet pack ".\src\DryIoc" -c Release -o ".\bin\NuGetPackages"
REM dotnet pack ".\src\DryIoc.Microsoft.DependencyInjection" -c Release -o "..\bin\NuGetPackages"
REM dotnet pack ".\src\DryIoc.Microsoft.Hosting"             -c Release -o "..\bin\NuGetPackages"
if %ERRORLEVEL% neq 0 call :error "dotnet pack"

echo:Success.
exit 0

:error
echo:%1 failed with error: %ERRORLEVEL%
exit %ERRORLEVEL%