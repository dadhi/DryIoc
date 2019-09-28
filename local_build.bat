@echo off
setlocal EnableDelayedExpansion

dotnet clean -v:m
dotnet build -c:Release -v:m -p:DevMode=false;LocalBuild=true
if %ERRORLEVEL% neq 0 goto :error

echo:
echo:## Finished: RESTORE and BUILD
echo: 
echo:## Starting: TESTS...
echo: 

dotnet test -c:Release -p:GeneratePackageOnBuild=false;DevMode=false;LocalBuild=true

if %ERRORLEVEL% neq 0 goto :error
echo:## Finished: TESTS

echo: 
echo:## Finished: TESTS

call BuildScripts\NugetPack.bat
if %ERRORLEVEL% neq 0 goto :error
echo:
echo:## Finished: PACKAGING ##

echo:
echo:## Finished: ALL Successful ##
exit /b 0

:error
echo:
echo:## Build Failed :-(
exit /b 1
