@echo off

set PACKAGES=..\.dist\packages
set SOURCE=https://api.nuget.org/v3/index.json
set /p APIKEY=<"..\ApiKey.txt"

dotnet nuget push "%PACKAGES%\DryIoc.dll.4.0.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.4.0.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIocZero.4.1.0.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIoc.Web.3.1.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.Web.dll.3.1.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIoc.Mvc.3.1.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.Mvc.dll.3.1.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIoc.WebApi.3.1.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.WebApi.dll.3.1.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIoc.Microsoft.DependencyInjection.2.1.0.nupkg" -k %APIKEY% -s %SOURCE%
rem dotnet nuget push "%PACKAGES%\DryIoc.Microsoft.Hosting.1.0.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%

echo:
echo:Publishing completed.

pause
