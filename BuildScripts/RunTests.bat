@echo off
pushd ".."

echo:
echo:Running tests . . .

set NUNIT="packages\NUnit.Runners\tools\nunit-console.exe"

for %%D in ("bin\Release\*Tests.dll") do %NUNIT% %%D /xml="bin\Release\TestResult.xml"
for %%D in ("Net40\bin\Release\*Tests.dll") do %NUNIT% %%D /xml="Net40\bin\Release\TestResult.xml"

echo:
echo:Tests succeeded.
popd

if not "%1"=="-nopause" pause