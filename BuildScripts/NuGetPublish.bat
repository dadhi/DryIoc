@echo off

pushd ".."

set NUGET=".nuget\NuGet.exe"
set PACKAGEDIR="bin\NuGetPackages"

echo:
echo:Publishing NuGet packages . . .
echo:-------------------------------
if not "%1"=="-nopause" pause

set /p APIKEY=<NuGet\apikey-staging.txt

for %%P in ("%PACKAGEDIR%\*.nupkg") do (
	rem For all packages Not containing "symbols.nupkg" ...
	for /f %%N in ('echo:%%P ^| find /v "symbols.nupkg"') do (
		%NUGET% push "%%N" -Source https://staging.nuget.org -ApiKey %APIKEY%  
	) 
)

echo: 
echo:Publishing succeeded.
popd

if not "%1"=="-nopause" pause 
goto:eof
