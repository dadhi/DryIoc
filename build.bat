@echo off
setlocal EnableDelayedExpansion

set SLN=DryIoc.sln

rem: Optional
rem dotnet clean --verbosity:minimal

echo:
echo:## Starting: DOTNET RESTORE... ##
echo: 
dotnet restore /p:DevMode=false %SLN%
if %ERRORLEVEL% neq 0 goto :error
echo:
echo:## Finished: DOTNET RESTORE ##
echo: 

rem Looking for MSBuild.exe path
set MSB="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\bin\MSBuild.exe"
if not exist %MSB% set MSB="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\bin\MSBuild.exe"
if not exist %MSB% for /f "tokens=4 delims='" %%p IN ('.nuget\nuget.exe restore ^| find "MSBuild auto-detection"') do set MSB="%%p\MSBuild.exe"
echo:
echo:## Using MSBuild: %MSB%
echo:
echo:## Starting: BUILD and PACKAGING... ##
echo: 
rem: Turning Off the $(DevMode) from the Directory.Build.props to alway build an ALL TARGETS
call %MSB% %SLN% /t:Rebuild /p:DevMode=false;Configuration=Release /nowarn:VSX1000 /m /v:m /bl /fl /flp:LogFile=MSBuild.log
if %ERRORLEVEL% neq 0 goto :error
echo:
echo:## Finished: BUILD and PACKAGING ##

echo:
echo:## Running: TESTS... ##

echo:
dotnet test /p:DevMode=false -c:Release --no-build .\docs\DryIoc.Docs    > TestResults.log
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.UnitTests   >> TestResults.log
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.IssuesTests     >> TestResults.log
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.MefAttributedModel.UnitTests    >> TestResults.log
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.Microsoft.DependencyInjection.Specification.Tests   >> TestResults.log
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.Web.UnitTests   >> TestResults.log
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.Mvc.UnitTests   >> TestResults.log
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.Owin.UnitTests  >> TestResults.log
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.WebApi.UnitTests   >> TestResults.log
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.WebApi.Owin.UnitTests  >> TestResults.log
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.SignalR.UnitTests  >> TestResults.log
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.CommonServiceLocator.UnitTests >> TestResults.log
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.Syntax.Autofac.UnitTests   >> TestResults.log

echo:
type TestResults.log
for /f %%i in ('type TestResults.log ^| find /i "Failed "') do if not "%%i"=="" goto :error
echo:
echo:## Finished: TESTS ##

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
