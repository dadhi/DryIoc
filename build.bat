@echo off
setlocal EnableDelayedExpansion

rem Calculate start time
set started_at=%time%
set /a started_at_ms=%started_at:~0,2%*24*60*100+%started_at:~3,2%*60*100+%started_at:~6,2%*100+%started_at:~9,2%

set "FrameworkParam=-f:net9.0"
set "LatestSupportedNetProp=-p:LatestSupportedNet=net9.0"
if [%1] NEQ [] (
    set "FrameworkParam=-f:%1"
    set "LatestSupportedNetProp=-p:LatestSupportedNet=%1"
)
echo:FrameworkParam == '%FrameworkParam%', LatestSupportedNetProp == '%LatestSupportedNetProp%'

echo:
echo:# Starting: ALL...
echo:[started at %started_at%]
echo:
echo:## Starting: Clean, Restore and Build...
echo:

dotnet clean -v:m %LatestSupportedNetProp%
dotnet build -v:m %LatestSupportedNetProp% -c:Release
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## Finished: Clean, Restore and Build
echo:
echo:## Starting: TestRunners for the latest supported .NET and .NET FRAMEWORK 4.7.2...
echo:
echo:### Latest supported .NET  - Default rules (interpretation, then compilation)

dotnet run --no-build %LatestSupportedNetProp% %FrameworkParam% -c:Release --project test/DryIoc.TestRunner/DryIoc.TestRunner.csproj

echo:
echo:### Latest supported .NET  - Compilation only

dotnet run %LatestSupportedNetProp% %FrameworkParam% -c:Release -p:UseCompilationOnly=true --project test/DryIoc.TestRunner/DryIoc.TestRunner.csproj

echo:
echo:
echo:### .NET FRAMEWORK 4.7.2 - Default rules (interpretation, then compilation)

dotnet run --no-build -c:Release --project test/DryIoc.TestRunner.net472/DryIoc.TestRunner.net472.csproj
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:### .NET FRAMEWORK 4.7.2 - Compilation only

dotnet run %LatestSupportedNetProp% -c:Release -p:UseCompilationOnly=true --project test/DryIoc.TestRunner.net472/DryIoc.TestRunner.net472.csproj
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:
echo:## Finished: TestRunners
echo:
echo:## Starting: Documentation generation
echo:

dotnet build docs\DryIoc.Docs\DryIoc.Docs.csproj -f:net7.0 -target:MdGenerate

echo:
echo:## Finished: Documentation generation
echo:
echo:## Starting: Packaging NuGet for source packages
echo:

call build\NugetPack.bat
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## Finished: Packaging
echo:

rem Calculate elapsed time
set finished_at=%time%
set /a finished_at_ms=%finished_at:~0,2%*24*60*100+%finished_at:~3,2%*60*100+%finished_at:~6,2%*100+%finished_at:~9,2%
set /a ellapsed_ms=%finished_at_ms%*10-%started_at_ms%*10

echo:
echo:[finished at %finished_at%, elapsed: %ellapsed_ms% ms]
echo:# Finished: ALL Successful
exit /b 0

:error
echo:
echo:# Finished: Something failed :-(
exit /b 1
