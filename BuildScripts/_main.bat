@echo off
setlocal

rem Parse arguments:
for %%A in (%*) do (
	if /i "%%A"=="-NOPAUSE" (set NOPAUSE=1) else (
	if /i "%%A"=="-PUBLISH" (set PUBLISH=1) else (
	if /i "%%A"=="-MSB15"   (set MSB15=1)   else (
	set UNKNOWNARG=1 & echo:Unknown script argument: "%%A"
	)))
)
if defined UNKNOWNARG (
	echo:ERROR: Unknown script arguments, allowed arguments: "-nopause", "-publish" 
	exit 1
)

echo:
echo:Clean, build, run tests, etc. . .
if defined PUBLISH echo:. . . and publish NuGet packages! 
echo:---------------------------------

set SCRIPTDIR="%~dp0"
echo:
echo:Switching to %SCRIPTDIR% . . .
cd /d %SCRIPTDIR%  

call Clean -nopause
call :Check

if defined MSB15 (call Build15 -nopause) else (call Build -nopause)
rem call :Check

call RunTestsWithCoverage -nopause
call :Check

call NuGetPack -nopause
call :Check

if defined PUBLISH (
	call NuGetPublish -nopause
	call :Check
)

echo:------------
echo:All Success.

if not defined NOPAUSE pause 
goto:eof

:Check

if %ERRORLEVEL% EQU 123 (
	echo:"* Strange MSBuild error 123 that could be ignored."
	exit /b
)

if ERRORLEVEL 1 (
	echo:
	echo:ERROR: One of steps is failed with ERRORLEVEL==%ERRORLEVEL%!
	if not defined NOPAUSE pause	
	exit 1
) else ( 
	exit /b
)