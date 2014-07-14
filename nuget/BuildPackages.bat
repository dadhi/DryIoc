@echo off
set NUGET="..\.nuget\NuGet.exe"
set OUTDIR="..\bin\NuGetPackages"
if not exist %OUTDIR% (md %OUTDIR%) else (pushd %OUTDIR% && del *.nupkg & popd) 

call :ParseVersion "..\DryIoc\Properties\Version.cs" || call :Exit
%NUGET% pack DryIoc.nuspec -Version %VER% -OutputDirectory %OUTDIR% -NonInteractive
%NUGET% pack DryIoc.dll.nuspec -Version %VER% -OutputDirectory %OUTDIR% -Symbols -NonInteractive

call :ParseVersion "..\DryIoc.MefAttributedModel\Properties\Version.cs" || call :Exit
%NUGET% pack DryIoc.MefAttributedModel.nuspec -Version %VER% -OutputDirectory %OUTDIR% -NonInteractive
%NUGET% pack DryIoc.MefAttributedModel.dll.nuspec -Version %VER% -OutputDirectory %OUTDIR% -Symbols -NonInteractive

:Exit
pause && exit

:ParseVersion
set VERFILE=%~1
for /f "usebackq delims==; tokens=2" %%Q in ("%VERFILE%") do for /f %%V in (%%Q) do set VER=%%V
exit /b