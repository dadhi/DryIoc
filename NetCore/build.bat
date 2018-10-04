@echo off
setlocal EnableDelayedExpansion

set SLN=".\DryIoc.sln"

rem finding MSBuild.exe
set MSB="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\bin\MSBuild.exe"
if not exist %MSB% set MSB="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\bin\MSBuild.exe"
if not exist %MSB% for /f "tokens=4 delims='" %%p IN ('.nuget\nuget.exe restore ^| find "MSBuild auto-detection"') do set MSB="%%p\MSBuild.exe"
echo:
echo:## USING MSBUILD: %MSB%
echo:

rem The nuget.exe executable is used directly because of error with restoring in `dotnet build` and `dotnet test`
call .nuget\nuget.exe restore %SLN%
if %ERRORLEVEL% neq 0 call :error "RESTORE"
echo:
echo:## RESTORE IS SUCCESSFUL ##
echo:

call %MSB% %SLN% /t:Rebuild;Pack /p:Configuration=Release /p:RestorePackages=false /v:minimal /fl /flp:LogFile=MSBuild.log
if %ERRORLEVEL% neq 0 call :error "BUILD;PACK"
echo:
echo:## BUILD, PACK IS SUCCESSFUL ##
echo:

dotnet test /restore:false .\docs\DryIoc.Docs
dotnet test /restore:false .\test\DryIoc.UnitTests
dotnet test /restore:false .\test\DryIoc.IssuesTests
dotnet test /restore:false .\test\DryIoc.MefAttributedModel.UnitTests
dotnet test /restore:false .\test\DryIoc.Microsoft.DependencyInjection.Specification.Tests
if %ERRORLEVEL% neq 0 call :error "TESTS"
echo:
echo:## TESTS ARE SUCCESSFUL ##
echo:

echo:## ALL DONE SUCCESSFULLY ##
echo:
exit /b 0

:error
echo:
echo:## %1 FAILED WITH ERROR: %ERRORLEVEL%
echo:
exit /b %ERRORLEVEL%