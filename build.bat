@echo off
setlocal EnableDelayedExpansion

set SLN=".\DryIoc.sln"

rem Looking for MSBuild.exe path
set MSB="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\bin\MSBuild.exe"
if not exist %MSB% set MSB="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\bin\MSBuild.exe"
if not exist %MSB% for /f "tokens=4 delims='" %%p IN ('.nuget\nuget.exe restore ^| find "MSBuild auto-detection"') do set MSB="%%p\MSBuild.exe"
echo:
echo:## USING MSBUILD: %MSB%
echo:

rem dotnet clean --verbosity:minimal

rem: Turning Off the $(DevMode) from the Directory.Build.props to alway build an ALL TARGETS
call %MSB% %SLN% /t:Restore;Rebuild /p:DevMode=false /p:Configuration=Release /verbosity:minimal /fl /flp:LogFile=MSBuild.log
if %ERRORLEVEL% neq 0 call :error "BUILD;PACK"
echo:
echo:## RESTORE and BUILD is SUCCESSFUL ##
echo:

dotnet test -c:Release --no-build .\docs\DryIoc.Docs
dotnet test -c:Release --no-build .\test\DryIoc.UnitTests
dotnet test -c:Release --no-build .\test\DryIoc.IssuesTests
dotnet test -c:Release --no-build .\test\DryIoc.MefAttributedModel.UnitTests
dotnet test -c:Release --no-build .\test\DryIoc.Microsoft.DependencyInjection.Specification.Tests
if %ERRORLEVEL% neq 0 call :error "TESTS"
echo:
echo:## TESTS are SUCCESSFUL ##
echo:

dotnet pack -c:Release --no-build -o:..\..\.dist\packages .\src\DryIoc\DryIoc.csproj
dotnet pack -c:Release --no-build -o:..\..\.dist\packages .\src\DryIocAttributes\DryIocAttributes.csproj
dotnet pack -c:Release --no-build -o:..\..\.dist\packages .\src\DryIoc.MefAttributedModel\DryIoc.MefAttributedModel.csproj
dotnet pack -c:Release --no-build -o:..\..\.dist\packages .\src\DryIoc.Microsoft.DependencyInjection\DryIoc.Microsoft.DependencyInjection.csproj
dotnet pack -c:Release --no-build -o:..\..\.dist\packages .\src\DryIoc.Microsoft.Hosting\DryIoc.Microsoft.Hosting.csproj
if %ERRORLEVEL% neq 0 call :error "PACKAGING"

call BuildScripts\NugetPack.bat
if %ERRORLEVEL% neq 0 call :error "PACKAGING SOURCE PACKAGES"
echo:
echo:## PACKAGING is SUCCESSFUL ##
echo:

echo:## ALL done SUCCESSFULLY ##
echo:
exit /b 0

:error
echo:
echo:## %1 FAILED WITH ERROR: %ERRORLEVEL%
echo:
exit /b %ERRORLEVEL%