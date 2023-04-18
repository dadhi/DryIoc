@echo off
setlocal EnableDelayedExpansion

echo:
echo:## Starting: TestRunner... ##
echo:

dotnet run -c Release --project test/DryIoc.TestRunner/DryIoc.TestRunner.csproj

if %ERRORLEVEL% neq 0 goto :error
echo:## Finished: TestRunner ##

echo:## Finished: ALL Successful ##
exit /b 0

:error
echo:
echo:## Build is failed :-(
exit /b 1
