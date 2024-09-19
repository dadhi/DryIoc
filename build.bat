@echo off
setlocal EnableDelayedExpansion

rem Calculate start time
set started_at=%time%
set /a started_at_ms=%started_at:~0,2%*24*60*100+%started_at:~3,2%*60*100+%started_at:~6,2%*100+%started_at:~9,2%

echo:
echo:# Starting: ALL...
echo:[started at %started_at%]
echo:
echo:## Starting: Clean, Restore and Build...
echo:

dotnet clean -v:m
dotnet build -c:Release -v:m
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## Finished: Clean, Restore and Build
echo:
echo:## Starting: TestRunners for .NET 8.0 and .NET FRAMEWORK 4.7.2...
echo:
echo:### .NET 8.0

dotnet run --no-build -f:net8.0 -c:Release --project test/DryIoc.TestRunner/DryIoc.TestRunner.csproj

echo:
echo:### .NET FRAMEWORK 4.7.2

dotnet run --no-build -c:Release --project test/DryIoc.TestRunner.net472/DryIoc.TestRunner.net472.csproj
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## Finished: TestRunners
echo:
echo:## Starting: Documentation generation
echo:

dotnet build docs\DryIoc.Docs\DryIoc.Docs.csproj -f:net7.0 -target:MdGenerate

echo:
echo:## Finished: Documentation generation
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
