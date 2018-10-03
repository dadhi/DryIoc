@echo off

set SLN=".\DryIoc.sln"
set MSB="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\bin\MSBuild.exe"

call .nuget\nuget.exe restore %SLN%
if %ERRORLEVEL% neq 0 call :error "RESTORE"
echo:
echo:## RESTORE IS SUCCESSFUL ##
echo:

call %MSB% %SLN% /t:Rebuild /t:Pack /p:Configuration=Release /p:RestorePackages=false /v:minimal /fl /flp:LogFile=MSBuild.log
if %ERRORLEVEL% neq 0 call :error "BUILD"
echo:
echo:## RESTORE, BUILD, PACK IS SUCCESSFUL ##
echo:

set NUNIT="packages\NUnit.ConsoleRunner.3.9.0\tools\nunit3-console.exe"
set OPENCOVER="packages\OpenCover.4.6.519\tools\OpenCover.Console.exe"
set REPORTGEN="packages\ReportGenerator.3.1.2\tools\ReportGenerator.exe"
set TESTRESULTS=.\TestResults
set COVERAGE=%TESTRESULTS%\Coverage.xml
if not exist %TESTRESULTS% md %TESTRESULTS% 

set TESTS=.\test\DryIoc.UnitTests\bin\Release\net45\DryIoc.UnitTests.dll^
    .\test\DryIoc.IssuesTests\bin\Release\net45\DryIoc.IssuesTests.dll^
    .\test\DryIoc.MefAttributedModel.UnitTests\bin\Release\net45\DryIoc.MefAttributedModel.UnitTests.dll

echo:## RUNNING TESTS: %TESTS%
echo: 
%OPENCOVER%^
 -register:path64^
 -target:%NUNIT%^
 -targetargs:"%TESTS% --out=%TESTRESULTS%\TestResult.xml"^
 -filter:"+[*]* -[*Test*]* -[*Docs*]* -[*Moq*]* -[Microsoft*]* -[xunit*]* -[NetCore*]*"^
 -excludebyattribute:*.ExcludeFromCodeCoverageAttribute^
 -hideskipped:all^
 -output:%COVERAGE%

echo:
echo:## GENERATING "%COVERAGE%" . . .
echo: 
%REPORTGEN%^
 -reports:%COVERAGE%^
 -targetdir:%TESTRESULTS%^
 -reporttypes:Html;HtmlSummary;Badges^
 -assemblyfilters:-*Test*^
 -classfilters:-DryIoc.Arg

echo:
echo:## ALL IS SUCCESSFUL ##
echo:
exit /b 0

:error
echo:
echo:## %1 FAILED WITH ERROR: %ERRORLEVEL%
echo:
exit /b %ERRORLEVEL%