@echo off
pushd ".."

echo:
echo:Running tests . . .

set NUNIT="packages\NUnit.Runners\tools\nunit-console.exe"

for %%P in ("."; "Net40"; "Net45"; "PCL-Net45") do (
    
	for %%D in ("%%P\bin\Release\*Tests.dll") do (
		
		echo:
		echo:Running tests from "%%~fD"
		echo:==================================================================================================================
		
		%NUNIT% "%%D" /xml="%%P\bin\Release\TestResult.xml"
	)
)

echo:
echo:Tests succeeded.

popd

if not "%1"=="-nopause" pause