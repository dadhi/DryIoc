@echo off
setlocal EnableDelayedExpansion

echo:
echo:# Build and Run TestRunner on .NET 8.0
echo:
rem Calculate start time
set start_t=%time%
set /a start_ms=%start_t:~0,2%*24*60*100+%start_t:~3,2%*60*100+%start_t:~6,2%*100+%start_t:~9,2%

dotnet run -v minimal -f net8.0 -c Release --project test/DryIoc.TestRunner
if %ERRORLEVEL% neq 0 goto :error

rem Calculate elapsed time
set fnish_t=%time%
set /a fnish_ms=%fnish_t:~0,2%*24*60*100+%fnish_t:~3,2%*60*100+%fnish_t:~6,2%*100+%fnish_t:~9,2%
set /a ellap_ms=%fnish_ms%*10-%start_ms%*10
echo:
echo:## ALL Tests passed in %ellap_ms% ms. 
echo:
exit /b 0

:error
echo:
echo:## Build is failed :-(
exit /b 1
