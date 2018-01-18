@echo off
setlocal EnableDelayedExpansion

set SLN="..\DryIoc.sln"
set OUTDIR="..\bin\Release"
set NUGET="..\.nuget\NuGet.exe"

set NOPAUSE=%1
set MSBUILDVER=%2
if "%MSBUILDVER%"=="" set MSBUILDVER=14
echo:Default MSBuild version is 15, fallback version is: %MSBUILDVER%

echo:
echo:Restoring packages for solution %SLN% . . .
%NUGET% restore %SLN%

echo:
echo:Building %SLN% into %OUTDIR% . . .

set MSBUILD15="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\bin\MSBuild.exe" 
echo:First try to find MSBuild 15: %MSBUILD15%
if exist %MSBUILD15% (
	echo:OK, MsBuild15 is present. Start building . . .

	%MSBUILD15% %SLN% /t:Rebuild /v:minimal /m /fl /flp:LogFile=MSBuild.log ^
		/p:OutDir=%OUTDIR% ^
		/p:GenerateProjectSpecificOutputFolder=false ^
		/p:Configuration=Release ^
		/p:RestorePackages=false 

	find /C "Build succeeded." MSBuild.log
	goto :eoscript
) 

echo:MsBuild15 is not found. Try other versions found in Win Registry.
for /f "tokens=2*" %%S in ('reg query HKLM\SOFTWARE\Wow6432Node\Microsoft\MSBuild\ToolsVersions\%MSBUILDVER%.0 /v MSBuildToolsPath') do (

	if exist "%%T" (

		echo:
		echo:Using MSBuild from path "%%T\MSBuild.exe"

		"%%T\MSBuild.exe" %SLN% /t:Rebuild /v:minimal /m /fl /flp:LogFile=MSBuild.log ^
   			/p:OutDir=%OUTDIR% ^
   			/p:GenerateProjectSpecificOutputFolder=false ^
   			/p:Configuration=Release ^
   			/p:RestorePackages=false 

		find /C "Build succeeded." MSBuild.log
    )
)

endlocal
:eoscript
if not "%NOPAUSE%"=="-nopause" pause