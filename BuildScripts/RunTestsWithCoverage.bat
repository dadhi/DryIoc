@echo off
pushd ".."
setlocal EnableDelayedExpansion

set NUNIT="packages\NUnit.Runners\tools\nunit-console.exe"
set OPENCOVER="packages\OpenCover\OpenCover.Console.exe"
set REPORTGEN="packages\ReportGenerator.2.3.2.0\tools\ReportGenerator.exe"
set REPORTS=bin\Reports
set COVERAGE="%REPORTS%\Coverage.xml"

if not exist %REPORTS% md %REPORTS% 

for %%P in ("."; "Net40"; "Net45"; "PCL-Net45"; "Extensions") do (
	for %%T in ("%%P\bin\Release\*Tests.dll") do (
		set TESTLIBS=!TESTLIBS! %%T
))

echo:
echo:Running tests with coverage into %COVERAGE% . . .
echo:
echo:from assemblies: %TESTLIBS%
echo: 

%OPENCOVER%^
 -register:user^
 -target:%NUNIT%^
 -targetargs:"%TESTLIBS% /nologo /noshadow"^
 -filter:"+[*]* -[*Test*]* -[protobuf*]* -[Microsoft*]* -[xunit*]* -[Dnx*]*"^
 -excludebyattribute:*.ExcludeFromCodeCoverageAttribute^
 -hideskipped:all^
 -output:%COVERAGE%

echo:
echo:Generating HTML coverage report in "%REPORTS%" . . .
echo: 

%REPORTGEN%^
 -reports:%COVERAGE%^
 -targetdir:%REPORTS%^
 -reporttypes:Html;HtmlSummary;Badges^
 -assemblyfilters:-*Test*^
 -classfilters:-DryIoc.Arg

rem start %REPORTS%\index.htm

echo:
echo:Succeeded.
endlocal
popd

if not "%1"=="-nopause" pause