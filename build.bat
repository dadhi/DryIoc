@echo off
setlocal EnableDelayedExpansion

dotnet clean -v:m
dotnet build -c:Release -v:m -p:DevMode=false
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## Finished: RESTORE and BUILD
echo:
@REM echo:## Starting: TestRunner... ##
@REM echo:

@REM dotnet run --no-build -c Release --project test/DryIoc.TestRunner/DryIoc.TestRunner.csproj
@REM if %ERRORLEVEL% neq 0 goto :error
@REM dotnet run --no-build -c Release --project test/DryIoc.TestRunner.net472/DryIoc.TestRunner.net472.csproj

@REM if %ERRORLEVEL% neq 0 goto :error
@REM echo:## Finished: TestRunner ##
@REM echo:
echo:## Starting: TESTS...
echo: 

dotnet test --no-build -c:Release -p:DevMode=false


if %ERRORLEVEL% neq 0 goto :error

echo: 
echo:## Finished: TESTS
echo:
echo:## Starting: DOCUMENTATION GENERATION ##
echo:

dotnet build docs\DryIoc.Docs\DryIoc.Docs.csproj -f netcoreapp3.1 -target:MdGenerate

echo:
echo:## Finished: DOCUMENTATION GENERATION ##
echo:

echo:## Finished: ALL Successful ##
exit /b 0

:error
echo:
echo:## Build is failed :-(
exit /b 1
