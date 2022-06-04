@echo off
setlocal EnableDelayedExpansion

dotnet clean -v:m
dotnet build -c:Release -v:m -p:DevMode=false
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:# Bare build and TestRunner, no NUnit test runner, no docs gen, no comp-time gen
echo:
echo:## Finished: RESTORE and BUILD
echo:
echo:## Starting: TestRunner... ##
echo:

dotnet run --no-build -c Release --project test/DryIoc.TestRunner/DryIoc.TestRunner.csproj
dotnet run --no-build -c Release --project test/DryIoc.TestRunner.net472/DryIoc.TestRunner.net472.csproj

echo:
echo:## Finished: ALL Successful ##
exit /b 0

:error
echo:
echo:## Build is failed :-(
exit /b 1
