@echo off
setlocal

echo:
echo:Clean, build, pack nuget packages..
echo:-----------------------------------

call Clean -nopause
call :Check

call Build -nopause
call :Check

rem call RunTests -nopause
rem call :Check

call NuGetPack -nopause
call :Check

echo:------------
echo:All Success.
echo:

if not "%1"=="-nopause" pause 
goto:eof

:Check
if ERRORLEVEL 1 (
echo:Failed with ERRORLEVEL==%ERRORLEVEL%!
if not "%1"=="-nopause" pause
) else exit /b
