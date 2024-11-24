@echo off

set PACKAGES=..\.dist\packages
set SOURCE=https://api.nuget.org/v3/index.json
set /p APIKEY=<"..\..\ApiKey.txt"

@REM dotnet nuget push "%PACKAGES%\DryIoc.dll.6.0.0-preview-08.nupkg" -k %APIKEY% -s %SOURCE%
@REM dotnet nuget push "%PACKAGES%\DryIoc.6.0.0-preview-08.nupkg" -k %APIKEY% -s %SOURCE%
@REM dotnet nuget push "%PACKAGES%\DryIoc.Internal.6.0.0-preview-08.nupkg" -k %APIKEY% -s %SOURCE%

@REM dotnet nuget push "%PACKAGES%\DryIoc.Microsoft.DependencyInjection.8.0.0-preview-03.nupkg" -k %APIKEY% -s %SOURCE%
@REM dotnet nuget push "%PACKAGES%\DryIoc.Microsoft.DependencyInjection.src.6.2.0.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.Microsoft.DependencyInjection.AspNetCore2_1.3.0.3.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIoc.Microsoft.Hosting.1.0.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%

dotnet nuget push "%PACKAGES%\DryIocAttributes.dll.8.0.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%
@REM rem dotnet nuget push "%PACKAGES%\DryIocAttributes.8.0.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%

dotnet nuget push "%PACKAGES%\DryIoc.MefAttributedModel.dll.8.0.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%
@REM dotnet nuget push "%PACKAGES%\DryIoc.MefAttributedModel.8.0.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIoc.CommonServiceLocator.dll.4.0.0.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.CommonServiceLocator.4.0.0.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIoc.Web.dll.4.0.0.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.Web.4.0.0.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIoc.Mvc.dll.4.0.0.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.Mvc.4.0.0.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIoc.Owin.dll.4.0.0.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.Owin.4.0.0.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIoc.SignalR.dll.4.0.0.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.SignalR.4.0.0.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIoc.Syntax.Autofac.dll.1.0.0.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.Syntax.Autofac.1.0.0.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIoc.WebApi.dll.4.0.0.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.WebApi.4.0.0.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIoc.WebApi.Owin.dll.4.0.0.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.WebApi.Owin.4.0.0.nupkg" -k %APIKEY% -s %SOURCE%

echo:
echo:Publishing completed.

pause
