@echo off
setlocal EnableDelayedExpansion

echo:
echo:# Build the TestRunner (.NET 472)
echo:

dotnet build -c:Release test/DryIoc.TestRunner.net472

echo:
echo:# Run the TestRunner
echo:

dotnet run --no-build -c Release --project test/DryIoc.TestRunner.net472
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## ALL Successful ##
exit /b 0

:error
echo:
echo:## Build is failed :-(
exit /b 1
