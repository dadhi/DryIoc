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
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '.\BuildScripts\MakeInternal.ps1'";
%NUGET% pack %NUSPECS%\DryIoc.Internal.nuspec -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIocZero
echo:============================
%NUGET% pack %NUSPECS%\DryIocZero.nuspec -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIocAttributes
echo:============================
%NUGET% pack %NUSPECS%\DryIocAttributes.nuspec -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:MefAttributedModel
echo:============================
%NUGET% pack %NUSPECS%\DryIoc.MefAttributedModel.nuspec -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIoc.CommonServiceLocator
echo:============================
%NUGET% pack %NUSPECS%\DryIoc.CommonServiceLocator.nuspec -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIoc.Microsoft.DependencyInjection
echo:============================
%NUGET% pack %NUSPECS%\DryIoc.Microsoft.DependencyInjection.src.nuspec -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIoc.Web
echo:============================
%NUGET% pack %NUSPECS%\DryIoc.Web.nuspec -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIoc.Mvc
echo:============================
%NUGET% pack %NUSPECS%\DryIoc.Mvc.nuspec -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIoc.WebApi
echo:============================
%NUGET% pack %NUSPECS%\DryIoc.WebApi.nuspec -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIoc.Owin
echo:============================
%NUGET% pack %NUSPECS%\DryIoc.Owin.nuspec -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIoc.WebApi.Owin
echo:============================
%NUGET% pack %NUSPECS%\DryIoc.WebApi.Owin.nuspec -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
echo:DryIoc.SignalR
echo:============================
%NUGET% pack %NUSPECS%\DryIoc.SignalR.nuspec -OutputDirectory %PACKAGEDIR% -NonInteractive

REM if not "%1"=="-nopause" pause 
REM goto:eof

REM set VERFILE=%~1
REM for /f "usebackq tokens=2,3 delims=:() " %%A in ("%VERFILE%") do (
REM 	if "%%A"=="AssemblyInformationalVersion" set VER=%%~B
REM exit /b