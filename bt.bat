@echo off
setlocal EnableDelayedExpansion

echo:
echo:# Run the TestRunner on .NET 8.0
echo:

dotnet run -f net8.0 -c Release --project test/DryIoc.TestRunner
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## ALL Successful ##
exit /b 0

:error
echo:
echo:## Build is failed :-(
exit /b 1
