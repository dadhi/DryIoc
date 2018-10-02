@echo off
setlocal EnableDelayedExpansion

set NUNIT="packages\NUnit.ConsoleRunner.3.9.0\tools\nunit3-console.exe"
set OPENCOVER="packages\OpenCover.4.6.519\tools\OpenCover.Console.exe"
set REPORTGEN="packages\ReportGenerator.3.1.2\tools\ReportGenerator.exe"
set REPORTS=CoverageReport
set COVERAGE="%REPORTS%\Coverage.xml"

if not exist %REPORTS% md %REPORTS% 

set TESTS= ^
.\test\DryIoc.UnitTests\Release\net45\DryIoc.UnitTests.dll ^
.\test\DryIoc.IssuesTests\Release\net45\DryIoc.UnitIssues.dll ^
.\test\DryIoc.MefAttributedModel.UnitTests\Release\net45\DryIoc.MefAttributedModel.UnitTests.dll

echo:
echo:Running tests with coverage. Results are collected in %COVERAGE% . . .
echo:
echo:from assemblies: %TESTLIBS%
echo: 

%OPENCOVER%^
 -register:path64^
 -target:%NUNIT%^
 -targetargs:"%TESTLIBS%"^
 -filter:"+[*]* -[*Test*]* -[*Docs*]* -[*Moq*]* -[Microsoft*]* -[xunit*]* -[NetCore*]*"^
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
