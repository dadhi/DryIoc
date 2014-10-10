@echo off
setlocal

echo:
echo:Clean, build, run tests, etc. . .
echo:---------------------------------

set SCRIPTDIR="%~dp0"
echo:
echo:Switching to %SCRIPT_DIR% . . .
cd /d %SCRIPTDIR%  

call Clean -nopause
call :Check

call Build -nopause
call :Check

call RunTests -nopause
call :Check

call NuGetPack -nopause
call :Check

echo:------------
echo:All Success.

if not "%1"=="-nopause" pause 
goto:eof

:Check

if %ERRORLEVEL% EQU 123 (
	echo:"* Strange MSBuild error 123 that could be ignored."
	exit /b
)

if ERRORLEVEL 1 (
	echo:
	echo:ERROR: One of steps is failed with ERRORLEVEL==%ERRORLEVEL%!
	if not "%1"=="-nopause" pause	
	exit 1
) else ( 
	exit /b
)