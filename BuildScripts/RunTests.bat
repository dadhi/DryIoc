@echo off
pushd ".."
setlocal EnableDelayedExpansion

echo:
echo:Running tests . . .

set NUNIT="packages\NUnit.Runners\tools\nunit-console.exe"

for %%P in ("."; "Net40"; "Net45"; "PCL-Net45"; "Extensions"; "Issues") do (
	for %%T in ("%%P\bin\Release\*Tests.dll") do (	
		set TESTS=!TESTS! "%%~T"
))

echo:
echo:Tests: %TESTS%
echo: 

%NUNIT% %TESTS% /xml="bin\Release\TestResult.xml"

echo:
echo:Tests succeeded.
endlocal
popd

if not "%1"=="-nopause" pause