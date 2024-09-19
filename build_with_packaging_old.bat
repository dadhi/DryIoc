@echo off
setlocal EnableDelayedExpansion

rem Calculate start time
set started_at=%time%
set /a started_at_ms=%started_at:~0,2%*24*60*100+%started_at:~3,2%*60*100+%started_at:~6,2%*100+%started_at:~9,2%

echo:
echo:## Starting the build...
echo:[started at %started_at%]
echo:

dotnet clean -v:m
dotnet build -c:Release -v:m
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## Finished: RESTORE and BUILD
echo:
echo:## Starting: TestRunner...
echo:

dotnet run --no-build -f:net8.0 -c:Release --project test/DryIoc.TestRunner/DryIoc.TestRunner.csproj
dotnet run --no-build -c:Release --project test/DryIoc.TestRunner.net472/DryIoc.TestRunner.net472.csproj
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## Finished: TestRunner
echo:
echo:## Starting: TESTS...
echo: 

dotnet test --no-build -c:Release

if %ERRORLEVEL% neq 0 goto :error

echo: 
echo:## Finished: TESTS
echo:
echo:## Starting: DOCUMENTATION GENERATION ##
echo:

dotnet build docs\DryIoc.Docs\DryIoc.Docs.csproj -f:net7.0 -target:MdGenerate

echo:
echo:## Finished: DOCUMENTATION GENERATION ##
echo:
echo:## Starting: PACKAGING NuGet  for source packages ##
echo:

call build\NugetPack.bat
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## Finished: Packaging

rem Calculate elapsed time
set finished_at=%time%
set /a finished_at_ms=%finished_at:~0,2%*24*60*100+%finished_at:~3,2%*60*100+%finished_at:~6,2%*100+%finished_at:~9,2%
set /a ellapsed_ms=%finished_at_ms%*10-%started_at_ms%*10

echo:
echo:[finished at %finished_at%, elapsed: %ellapsed_ms% ms]
echo:## Finished: ALL Successful ##
exit /b 0

:error
echo:
echo:## Build is failed :-(
exit /b 1
