@echo off

echo:
echo:Running tests..
echo:

set NUNIT="packages\NUnit.Runners.2.6.3\tools\nunit-console.exe"

for %%D in ("bin\Release\*Tests.dll") do %NUNIT% %%D
for %%D in ("Net40\bin\Release\*Tests.dll") do %NUNIT% %%D

echo:
echo:Tests succeeded.
echo:

if not "%1"=="-nopause" pause