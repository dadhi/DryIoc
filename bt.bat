@echo off
setlocal EnableDelayedExpansion

echo:
echo:# Build the TestRunner (.NET 7 only)
echo:

dotnet build  -f net7.0 -c Release test/DryIoc.TestRunner/DryIoc.TestRunner.csproj
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:# Run the TestRunner
echo:

dotnet run --no-build -f net7.0 -c Release --project test/DryIoc.TestRunner/DryIoc.TestRunner.csproj
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## ALL Successful ##
exit /b 0

:error
echo:
echo:## Build is failed :-(
exit /b 1
