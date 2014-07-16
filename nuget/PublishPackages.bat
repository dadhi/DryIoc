@echo off
setlocal EnableDelayedExpansion

set NUGET="..\.nuget\NuGet.exe"
set /p APIKEY=<apikey.txt

for %%P in ("..\bin\NuGetPackages\*.nupkg") do (
	set PX=%%P
	if not "!PX:~-14!"==".symbols.nupkg" (
		%NUGET% push %%P -Source https://nuget.org -ApiKey %APIKEY%
	)
)

pause
endlocal