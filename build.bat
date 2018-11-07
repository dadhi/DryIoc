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
echo:## Starting: MSBUILD /t:REBUILD... ##
echo: 
rem: Turning Off the $(DevMode) from the Directory.Build.props to alway build an ALL TARGETS
call %MSB% %SLN% /t:Rebuild /p:DevMode=false;Configuration=Release /m /v:m /bl /fl /flp:LogFile=MSBuild.log
if %ERRORLEVEL% neq 0 goto :error
echo:
echo:## Finished: REBUILD ##

echo:
echo:## Starting: TESTS... ##
echo:
dotnet test /p:DevMode=false -c:Release --no-build .\docs\DryIoc.Docs
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.UnitTests
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.IssuesTests
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.MefAttributedModel.UnitTests
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.Microsoft.DependencyInjection.Specification.Tests
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.Web.UnitTests
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.Mvc.UnitTests
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.Owin.UnitTests
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.WebApi.UnitTests
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.SignalR.UnitTests
dotnet test /p:DevMode=false -c:Release --no-build .\test\DryIoc.CommonServiceLocator.UnitTests
if %ERRORLEVEL% neq 0 goto :error
echo:
echo:## Finished: TESTS ##

echo:
echo:## Starting: PACKAGING... ##
echo:
call %MSB% .\src\DryIoc\DryIoc.csproj /v:m /t:Pack /p:DevMode=false;Configuration=Release;PackageOutputPath=..\..\.dist
call %MSB% .\src\DryIocAttributes\DryIocAttributes.csproj /v:m /t:Pack /p:DevMode=false;Configuration=Release;PackageOutputPath=..\..\.dist
call %MSB% .\src\DryIoc.MefAttributedModel\DryIoc.MefAttributedModel.csproj /v:m /t:Pack /p:DevMode=false;Configuration=Release;PackageOutputPath=..\..\.dist
call %MSB% .\src\DryIoc.Microsoft.DependencyInjection\DryIoc.Microsoft.DependencyInjection.csproj /v:m /t:Pack /p:DevMode=false;Configuration=Release;PackageOutputPath=..\..\.dist
call %MSB% .\src\DryIoc.Microsoft.Hosting\DryIoc.Microsoft.Hosting.csproj /v:m /t:Pack /p:DevMode=false;Configuration=Release;PackageOutputPath=..\..\.dist
call %MSB% .\src\DryIoc.Web\DryIoc.Web.csproj /v:m /t:Pack /p:DevMode=false;Configuration=Release;PackageOutputPath=..\..\.dist
call %MSB% .\src\DryIoc.Mvc\DryIoc.Mvc.csproj /v:m /t:Pack /p:DevMode=false;Configuration=Release;PackageOutputPath=..\..\.dist
call %MSB% .\src\DryIoc.Owin\DryIoc.Owin.csproj /v:m /t:Pack /p:DevMode=false;Configuration=Release;PackageOutputPath=..\..\.dist
call %MSB% .\src\DryIoc.WebApi\DryIoc.WebApi.csproj /v:m /t:Pack /p:DevMode=false;Configuration=Release;PackageOutputPath=..\..\.dist
call %MSB% .\src\DryIoc.WebApi\DryIoc.SignalR.csproj /v:m /t:Pack /p:DevMode=false;Configuration=Release;PackageOutputPath=..\..\.dist
call %MSB% .\src\DryIoc.CommonServiceLocator\DryIoc.CommonServiceLocator.csproj /v:m /t:Pack /p:DevMode=false;Configuration=Release;PackageOutputPath=..\..\.dist
if %ERRORLEVEL% neq 0 goto :error

call BuildScripts\NugetPack.bat
if %ERRORLEVEL% neq 0 call :error "PACKAGING SOURCE PACKAGES"
echo:
echo:## Finished: PACKAGING ##
echo:
echo:## Finished: ALL ##
echo:
exit /b 0

:error
echo:
echo:## :-( Failed with ERROR: %ERRORLEVEL%
echo:
exit /b %ERRORLEVEL%