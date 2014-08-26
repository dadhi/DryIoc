@echo off
pushd ".."

set NUGET=".nuget\NuGet.exe"
set PACKAGE_OUTDIR="bin\NuGetPackages"

echo:
echo:Packing NuGet packages into %PACKAGE_OUTDIR% . . .
echo:

if exist %PACKAGE_OUTDIR% rd /s /q %PACKAGE_OUTDIR%
md %PACKAGE_OUTDIR% 

call :ParseVersion "DryIoc\Properties\Version.cs"
%NUGET% pack "NuGet\DryIoc.nuspec" -Version %VER% -OutputDirectory %PACKAGE_OUTDIR% -NonInteractive
%NUGET% pack "NuGet\DryIoc.dll.nuspec" -Version %VER% -OutputDirectory %PACKAGE_OUTDIR% -Symbols -NonInteractive

call :ParseVersion "DryIoc.MefAttributedModel\Properties\Version.cs"
%NUGET% pack "NuGet\DryIoc.MefAttributedModel.nuspec" -Version %VER% -OutputDirectory %PACKAGE_OUTDIR% -NonInteractive
%NUGET% pack "NuGet\DryIoc.MefAttributedModel.dll.nuspec" -Version %VER% -OutputDirectory %PACKAGE_OUTDIR% -Symbols -NonInteractive

echo: 
echo:Packaging succeeded.
popd

if not "%1"=="-nopause" pause 
goto:eof

:ParseVersion
set VERFILE=%~1
for /f "usebackq delims==; tokens=2" %%Q in ("%VERFILE%") do for /f %%V in (%%Q) do set VER=%%V
exit /b