@echo off
setlocal EnableDelayedExpansion

set NUGET=.nuget\NuGet.exe
set NUSPECS=nuspecs
set PACKAGEDIR=.dist\packages

echo:
echo:Packing NuGet packages into %PACKAGEDIR% . . .
echo:
if not exist %PACKAGEDIR% md %PACKAGEDIR% 

echo:
echo:DryIoc source and internal packages
echo:===================================
%NUGET% pack %NUSPECS%\DryIoc.nuspec -OutputDirectory %PACKAGEDIR% -NonInteractive
REM PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '.\BuildScripts\MakeInternal.ps1'";
REM %NUGET% pack %NUSPECS%\DryIoc.Internal.nuspec -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIocZero
echo:============================
%NUGET% pack %NUSPECS%\DryIocZero.nuspec -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIocAttributes
echo:============================
REM %NUGET% pack %NUSPECS%\DryIocAttributes.nuspec -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:MefAttributedModel
echo:============================
REM %NUGET% pack %NUSPECS%\DryIoc.MefAttributedModel.nuspec -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIoc.CommonServiceLocator
echo:============================
REM %NUGET% pack %NUSPECS%\DryIoc.CommonServiceLocator.nuspec -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIoc.Web
echo:============================
REM %NUGET% pack %NUSPECS%\DryIoc.Web.nuspec -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIoc.Mvc
echo:============================
REM %NUGET% pack %NUSPECS%\DryIoc.Mvc.nuspec -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIoc.WebApi
echo:============================
REM %NUGET% pack %NUSPECS%\DryIoc.WebApi.nuspec -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIoc.Owin
echo:============================
REM %NUGET% pack %NUSPECS%\DryIoc.Owin.nuspec -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIoc.WebApi.Owin
echo:============================
REM %NUGET% pack %NUSPECS%\DryIoc.WebApi.Owin.nuspec -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIoc.SignalR
echo:============================
REM %NUGET% pack %NUSPECS%\DryIoc.SignalR.nuspec -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive

REM if not "%1"=="-nopause" pause 
REM goto:eof

REM set VERFILE=%~1
REM for /f "usebackq tokens=2,3 delims=:() " %%A in ("%VERFILE%") do (
REM 	if "%%A"=="AssemblyInformationalVersion" set VER=%%~B
REM exit /b