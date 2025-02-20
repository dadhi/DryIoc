@echo off
setlocal EnableDelayedExpansion

set "LatestSupportedNet=net8.0"

rem Calculate start time
set started_at=%time%
set /a started_at_ms=1%started_at:~0,2%*24*60*100-100+%started_at:~3,2%*60*100+%started_at:~6,2%*100+%started_at:~9,2%

echo:[STARTED AT %started_at%]
echo:
echo:# Build and Run TestRunners for %LatestSupportedNet% and .NET FRAMEWORK 4.7.2
echo:
echo:## %LatestSupportedNet%
dotnet run -v:minimal -c:Release -f:net9.0 -p:LatestSupportedNet=net9.0 --project test/DryIoc.TestRunner/DryIoc.TestRunner.csproj

echo:
echo:## .NET FRAMEWORK 4.7.2
dotnet run -v:minimal -c:Release --project test/DryIoc.TestRunner.net472/DryIoc.TestRunner.net472.csproj

if %ERRORLEVEL% neq 0 goto :error

rem Calculate elapsed time
set finished_at=%time%
set /a finished_at_ms=1%finished_at:~0,2%*24*60*100-100+%finished_at:~3,2%*60*100+%finished_at:~6,2%*100+%finished_at:~9,2%
set /a ellapsed_ms=%finished_at_ms%*10-%started_at_ms%*10

echo:
echo:[FINISHED AT %finished_at%, ELAPSED: %ellapsed_ms% MS]
echo:# Finished: ALL Successful
exit /b 0

:error
echo:
echo:# Finished: Something failed :-(
exit /b 1
