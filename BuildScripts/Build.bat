@echo off

set SLN="..\DryIoc.sln"
set PROJECT_OUTDIR_SETTING="..\bin\Release"

echo:
echo:Building %SLN% into %PROJECT_OUTDIR_SETTING% . . .

rem MSBuild 32-bit operating systems:
rem HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions\12.0

for /f "tokens=2*" %%S in ('reg query HKLM\SOFTWARE\Wow6432Node\Microsoft\MSBuild\ToolsVersions\12.0 /v MSBuildToolsPath') do (
if exist "%%T" (

echo:
echo:Using MSBuild from "%%T"

"%%T\MSBuild.exe" %SLN% /t:Rebuild /p:OutDir=%PROJECT_OUTDIR_SETTING% /p:Configuration=Release /p:RestorePackages=false /m /p:BuildInParallel=true
))

if not "%1"=="-nopause" pause