@echo off
setlocal EnableDelayedExpansion

dotnet clean -v:m -p:DevMode=false
dotnet build -c:Release -v:m -p:DevMode=false
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## Finished: RESTORE and BUILD
echo:
echo:## Starting: TestRunner...
echo:

dotnet run --no-build -f net7.0 -c Release -p:DevMode=false --project test/DryIoc.TestRunner/DryIoc.TestRunner.csproj
if %ERRORLEVEL% neq 0 goto :error
dotnet run --no-build -c Release --project test/DryIoc.TestRunner.net472/DryIoc.TestRunner.net472.csproj
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## Finished: TestRunner
echo:
echo:## Starting: TESTS...
echo: 

dotnet test --no-build -c:Release -p:DevMode=false

if %ERRORLEVEL% neq 0 goto :error

echo: 
echo:## Finished: TESTS
echo:
echo:## Starting: DOCUMENTATION GENERATION ##
echo:

dotnet build docs\DryIoc.Docs\DryIoc.Docs.csproj -f net6.0 -target:MdGenerate -p:DevMode=false

echo:
echo:## Finished: DOCUMENTATION GENERATION ##
echo:

call build\NugetPack.bat
if %ERRORLEVEL% neq 0 goto :error
echo:
echo:## Finished: PACKAGING ##
echo:
echo:## Finished: ALL Successful ##
exit /b 0

:error
echo:
echo:## Build is failed :-(
exit /b 1
