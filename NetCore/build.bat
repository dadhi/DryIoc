@echo off

set SLN=".\DryIoc.sln"
set MSB="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\bin\MSBuild.exe"

call .nuget\nuget.exe restore %SLN%
if %ERRORLEVEL% neq 0 call :error "RESTORE"
echo:
echo:## RESTORE IS SUCCESSFUL ##
echo:

call %MSB% %SLN% /t:Rebuild /p:Configuration=Release /p:RestorePackages=false /v:minimal /fl /flp:LogFile=MSBuild.log
if %ERRORLEVEL% neq 0 call :error "BUILD"
echo:
echo:## BUILD IS SUCCESSFUL ##
echo:

set NUNIT="packages\NUnit.ConsoleRunner.3.9.0\tools\nunit3-console.exe"
set OPENCOVER="packages\OpenCover.4.6.519\tools\OpenCover.Console.exe"
set REPORTGEN="packages\ReportGenerator.3.1.2\tools\ReportGenerator.exe"
set REPORTS=.\CoverageReport
set COVERAGE=%REPORTS%\Coverage.xml
if not exist %REPORTS% md %REPORTS% 

set TESTS=.\test\DryIoc.UnitTests\bin\Release\net45\DryIoc.UnitTests.dll^
    .\test\DryIoc.IssuesTests\bin\Release\net45\DryIoc.IssuesTests.dll^
    .\test\DryIoc.MefAttributedModel.UnitTests\bin\Release\net45\DryIoc.MefAttributedModel.UnitTests.dll

echo:
echo:## RUNNING TESTS: %TESTS%
echo: 
%OPENCOVER%^
 -register:path64^
 -target:%NUNIT%^
 -targetargs:"%TESTS%"^
 -filter:"+[*]* -[*Test*]* -[*Docs*]* -[*Moq*]* -[Microsoft*]* -[xunit*]* -[NetCore*]*"^
 -excludebyattribute:*.ExcludeFromCodeCoverageAttribute^
 -hideskipped:all^
 -output:%COVERAGE%

echo:
echo:## GENERATING "%COVERAGE%" . . .
echo: 
%REPORTGEN%^
 -reports:%COVERAGE%^
 -targetdir:%REPORTS%^
 -reporttypes:Html;HtmlSummary;Badges^
 -assemblyfilters:-*Test*^
 -classfilters:-DryIoc.Arg

REM dotnet test ".\test\DryIoc.UnitTests"
REM dotnet test ".\test\DryIoc.Microsoft.DependencyInjection.Specification.Tests"
REM dotnet test ".\test\DryIoc.Microsoft.DependencyInjection.Specification.Tests.v1.1"
REM if %ERRORLEVEL% neq 0 call :error "dotnet test"

REM dotnet pack ".\src\DryIoc" -c Release -o ".\bin\NuGetPackages"
REM dotnet pack ".\src\DryIoc.Microsoft.DependencyInjection" -c Release -o "..\bin\NuGetPackages"
REM dotnet pack ".\src\DryIoc.Microsoft.Hosting"             -c Release -o "..\bin\NuGetPackages"
REM if %ERRORLEVEL% neq 0 call :error "dotnet pack"

echo:All is successful.
pause
exit 0

:error
echo:%1 failed with error: %ERRORLEVEL%
exit %ERRORLEVEL%