@echo off

set PACKAGES=..\.dist\packages
set SOURCE=https://api.nuget.org/v3/index.json
set /p APIKEY=<"..\ApiKey.txt"

dotnet nuget push "%PACKAGES%\DryIoc.dll.4.0.1.nupkg" -k %APIKEY% -s %SOURCE%
dotnet nuget push "%PACKAGES%\DryIoc.4.0.1.nupkg" -k %APIKEY% -s %SOURCE%
dotnet nuget push "%PACKAGES%\DryIoc.Internal.4.0.1.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIocZero.4.1.0.nupkg" -k %APIKEY% -s %SOURCE%

dotnet nuget push "%PACKAGES%\DryIoc.Microsoft.DependencyInjection.3.0.1.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.Microsoft.Hosting.1.0.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIocAttributes.dll.5.0.0.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIocAttributes.5.0.0.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIoc.MefAttributedModel.dll.5.0.0.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.MefAttributedModel.5.0.0.nupkg" -k %APIKEY% -s %SOURCE%

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
