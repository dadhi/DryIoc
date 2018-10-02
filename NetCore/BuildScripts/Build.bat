@echo off
setlocal EnableDelayedExpansion

set SLN=..\DryIoc.sln
set OUTDIR=..\bin\Release
set NUGET=..\.nuget\NuGet.exe

set NOPAUSE=%1

echo:
echo:Restoring packages for solution %SLN% . . .
%NUGET% restore %SLN%
 
pause

echo:
echo:Building %SLN% into %OUTDIR% . . .

set MSBUILD15PRO="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\bin\MSBuild.exe" 
set MSBUILD15COMMUNITY="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\bin\MSBuild.exe" 

echo:
echo:... Looking for MSBuild in %MSBUILD15COMMUNITY%
if exist %MSBUILD15COMMUNITY% (set MSBUILDPATH=%MSBUILD15COMMUNITY%) else (

echo:not found!
echo:... Looking for MSBuild in %MSBUILD15PRO%
if exist %MSBUILD15PRO%       (set MSBUILDPATH=%MSBUILD15PRO%) else (

echo:not found!
echo:... Looking for MSBuild path in "HKLM\SOFTWARE\Wow6432Node\Microsoft\MSBuild\ToolsVersions\15.0 /v MSBuildToolsPath"
for /f "tokens=2*" %%S in ('reg query HKLM\SOFTWARE\Wow6432Node\Microsoft\MSBuild\ToolsVersions\15.0 /v MSBuildToolsPath') do (
if exist "%%T"                (set MSBUILDPATH="%%T\MSBuild.exe") else (

echo:not found!
echo:... Looking for MSBuild path in "HKLM\SOFTWARE\Wow6432Node\Microsoft\MSBuild\ToolsVersions\14.0 /v MSBuildToolsPath"
for /f "tokens=2*" %%S in ('reg query HKLM\SOFTWARE\Wow6432Node\Microsoft\MSBuild\ToolsVersions\14.0 /v MSBuildToolsPath') do (
if exist "%%T"                (set MSBUILDPATH="%%T\MSBuild.exe") else (
echo:Huh, MSBuild path is not found, exiting...
if not "%NOPAUSE%"=="-nopause" pause
exit 1
))))))

echo:MSBuild is found in %MSBUILDPATH%

pause

%MSBUILDPATH% %SLN% /t:Rebuild /v:minimal /m /fl /flp:LogFile=MSBuild.log ^
    /p:Configuration=Release ^
    /p:RestorePackages=false  

find /C "Build succeeded." MSBuild.log
echo:
echo:Hurrah, found words "Build succeeded" in MSBuild.log
echo:
if not "%NOPAUSE%"=="-nopause" pause
endlocal
