@echo off
pushd ".."

set NUGET=".nuget\NuGet.exe"
set PACKAGEDIR="bin\NuGetPackages"

echo:
echo:Packing NuGet packages into %PACKAGEDIR% . . .

if exist %PACKAGEDIR% rd /s /q %PACKAGEDIR%
md %PACKAGEDIR% 

echo:
call :ParseVersion "DryIoc\Properties\AssemblyInfo.cs"
echo:DryIoc v%VER%
echo:================
%NUGET% pack "NuGet\DryIoc.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive
%NUGET% pack "NuGet\DryIoc.dll.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive -Symbols

echo:
call :ParseVersion "DryIoc\Properties\AssemblyInfo.cs"
echo:DryIoc.Internal v%VER%
echo:================
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '.\BuildScripts\MakeInternal.ps1'";
%NUGET% pack "NuGet\DryIoc.Internal.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive

echo:
call :ParseVersion "DryIocAttributes\Properties\AssemblyInfo.cs"
echo:DryIocAttributes v%VER%
echo:============================
%NUGET% pack "NuGet\DryIocAttributes.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive
%NUGET% pack "NuGet\DryIocAttributes.dll.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive -Symbols 

echo:
call :ParseVersion "DryIoc.MefAttributedModel\Properties\AssemblyInfo.cs"
echo:MefAttributedModel v%VER%
echo:============================
%NUGET% pack "NuGet\DryIoc.MefAttributedModel.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive
%NUGET% pack "NuGet\DryIoc.MefAttributedModel.dll.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive -Symbols 

echo:
call :ParseVersion "Extensions\DryIoc.CommonServiceLocator\Properties\AssemblyInfo.cs"
echo:DryIoc.CommonServiceLocator v%VER%
echo:============================
%NUGET% pack "NuGet\DryIoc.CommonServiceLocator.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive
%NUGET% pack "NuGet\DryIoc.CommonServiceLocator.dll.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive -Symbols 

echo:
call :ParseVersion "Extensions\DryIoc.Web\Properties\AssemblyInfo.cs"
echo:DryIoc.Web v%VER%
echo:============================
%NUGET% pack "NuGet\DryIoc.Web.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive
%NUGET% pack "NuGet\DryIoc.Web.dll.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive -Symbols

echo:
call :ParseVersion "Extensions\DryIoc.Mvc\Properties\AssemblyInfo.cs"
echo:DryIoc.Mvc v%VER%
echo:============================
%NUGET% pack "NuGet\DryIoc.Mvc.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive
%NUGET% pack "NuGet\DryIoc.Mvc.dll.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive -Symbols

echo:
call :ParseVersion "Extensions\DryIoc.WebApi\Properties\AssemblyInfo.cs"
echo:DryIoc.WebApi v%VER%
echo:============================
%NUGET% pack "NuGet\DryIoc.WebApi.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive
%NUGET% pack "NuGet\DryIoc.WebApi.dll.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive -Symbols

echo:
call :ParseVersion "Extensions\DryIoc.Owin\Properties\AssemblyInfo.cs"
echo:DryIoc.Owin v%VER%
echo:============================
%NUGET% pack "NuGet\DryIoc.Owin.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive
%NUGET% pack "NuGet\DryIoc.Owin.dll.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive -Symbols

echo:
call :ParseVersion "Extensions\DryIoc.WebApi.Owin\Properties\AssemblyInfo.cs"
echo:DryIoc.WebApi.Owin v%VER%
echo:============================
%NUGET% pack "NuGet\DryIoc.WebApi.Owin.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive
%NUGET% pack "NuGet\DryIoc.WebApi.Owin.dll.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive -Symbols

echo:
call :ParseVersion "Extensions\DryIoc.SignalR\Properties\AssemblyInfo.cs"
echo:DryIoc.SignalR v%VER%
echo:============================
%NUGET% pack "NuGet\DryIoc.SignalR.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive
%NUGET% pack "NuGet\DryIoc.SignalR.dll.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive -Symbols


echo:
call :ParseVersion "Net45\DryIocZero\Properties\AssemblyInfo.cs"
echo:DryIocZero v%VER%
echo:============================
%NUGET% pack "NuGet\DryIocZero.nuspec" -Version %VER% -OutputDirectory %PACKAGEDIR% -NonInteractive

echo: 
echo:Packaging succeeded.
popd

if not "%1"=="-nopause" pause 
goto:eof

:ParseVersion
set VERFILE=%~1
for /f "usebackq tokens=2,3 delims=:() " %%A in ("%VERFILE%") do (
	if "%%A"=="AssemblyInformationalVersion" set VER=%%~B
)
exit /b