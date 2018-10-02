@echo off

set SLN=".\DryIoc.sln"
set MSB="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\bin\MSBuild.exe"

call .nuget\nuget.exe restore %SLN%
if %ERRORLEVEL% neq 0 call :error "RESTORE"
echo:
echo:## RESTORE IS SUCCESSFUL ##
echo:

call %MSB% %SLN% /t:Rebuild /p:Configuration=Release /p:RestorePackages=false /v:minimal /fl /flp:LogFile=MSBuild.log
if %ERRORLEVEL% neq 0 call :error "BUILD"
echo:
echo:## BUILD IS SUCCESSFUL ##
echo:

REM dotnet test ".\test\DryIoc.UnitTests"
REM dotnet test ".\test\DryIoc.Microsoft.DependencyInjection.Specification.Tests"
REM dotnet test ".\test\DryIoc.Microsoft.DependencyInjection.Specification.Tests.v1.1"
REM if %ERRORLEVEL% neq 0 call :error "dotnet test"

REM dotnet pack ".\src\DryIoc" -c Release -o ".\bin\NuGetPackages"
REM dotnet pack ".\src\DryIoc.Microsoft.DependencyInjection" -c Release -o "..\bin\NuGetPackages"
REM dotnet pack ".\src\DryIoc.Microsoft.Hosting"             -c Release -o "..\bin\NuGetPackages"
REM if %ERRORLEVEL% neq 0 call :error "dotnet pack"

echo:All is successful.
exit 0

:error
echo:%1 failed with error: %ERRORLEVEL%
exit %ERRORLEVEL%