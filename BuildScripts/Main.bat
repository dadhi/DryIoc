@echo off
setlocal

echo:
echo:Clean, build, run tests, etc. . .
echo:---------------------------------

set SCRIPT_DIR="%~dp0"
echo:
echo:Switching to %SCRIPT_DIR% . . .
cd /d %SCRIPT_DIR%  

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
if ERRORLEVEL 1 (
	echo:
	echo:ERROR: One of steps is failed with ERRORLEVEL==%ERRORLEVEL%!
	exit 1
) else ( 
	exit /b
)