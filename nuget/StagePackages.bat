@echo off
setlocal EnableDelayedExpansion

set NUGET="..\.nuget\NuGet.exe"
set /p APIKEY=<apikey-staging.txt

for %%P in ("..\bin\NuGetPackages\*.nupkg") do (
	set PX=%%P
	if not "!PX:~-14!"==".symbols.nupkg" (
		%NUGET% push %%P -Source https://staging.nuget.org -ApiKey %APIKEY%
	)
)

pause