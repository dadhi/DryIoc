@echo off
set OUTDIR=".\bin"

echo Cleaning..
if not exist %OUTDIR% goto:eof
pushd %OUTDIR%
del /q *.* 
for /f "tokens=*" %%D in ('dir /B') do rd /s /q "%%D"
popd 
