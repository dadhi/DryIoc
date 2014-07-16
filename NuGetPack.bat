@echo off

set NUGET=".nuget\NuGet.exe"
set PACKDIR="bin\NuGetPackages"

echo:
echo:Packing NuGet packages into %PACKDIR% ..
echo:


if exist %PACKDIR% rd /s /q %PACKDIR%
md %PACKDIR% 

call :ParseVersion "DryIoc\Properties\Version.cs"
%NUGET% pack "NuGet\DryIoc.nuspec" -Version %VER% -OutputDirectory %PACKDIR% -NonInteractive
%NUGET% pack "NuGet\DryIoc.dll.nuspec" -Version %VER% -OutputDirectory %PACKDIR% -Symbols -NonInteractive

call :ParseVersion "DryIoc.MefAttributedModel\Properties\Version.cs"
%NUGET% pack "NuGet\DryIoc.MefAttributedModel.nuspec" -Version %VER% -OutputDirectory %PACKDIR% -NonInteractive
%NUGET% pack "NuGet\DryIoc.MefAttributedModel.dll.nuspec" -Version %VER% -OutputDirectory %PACKDIR% -Symbols -NonInteractive

echo: 
echo:Packaging succeeded. 
echo:

if not "%1"=="-nopause" pause 
goto:eof

:ParseVersion
set VERFILE=%~1
for /f "usebackq delims==; tokens=2" %%Q in ("%VERFILE%") do for /f %%V in (%%Q) do set VER=%%V
exit /b