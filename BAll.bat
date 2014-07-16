@echo off
setlocal

echo:
echo:Clean, build, pack nuget packages..
echo:-----------------------------------

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
echo:

if not "%1"=="-nopause" pause 
goto:eof

:Check
if ERRORLEVEL 1 (
echo:Failed with ERRORLEVEL==%ERRORLEVEL%!
if not "%1"=="-nopause" pause
exit
) else exit /b
