@echo off
setlocal EnableDelayedExpansion

dotnet clean -v:m
dotnet build -c:Release -v:m
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:# Bare build and TestRunner, no NUnit test runner, no docs gen, no comp-time gen
echo:
echo:## Finished: RESTORE and BUILD
echo:
echo:## Starting: TestRunner...
echo:

dotnet run --no-build -c:Release -f:net8.0 --project test/DryIoc.TestRunner
if %ERRORLEVEL% neq 0 goto :error
dotnet run --no-build -c:Release --project test/DryIoc.TestRunner.net472
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## ALL Successful ##
exit /b 0

:error
echo:
echo:## Build is failed :-(
exit /b 1
