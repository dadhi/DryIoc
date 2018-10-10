@echo off

set PACKAGES=src\bin\NuGetPackages
set SOURCE=https://api.nuget.org/v3/index.json
set /p APIKEY=<"..\ApiKey.txt"

dotnet nuget push "%PACKAGES%\DryIoc.Microsoft.DependencyInjection.2.1.0.nupkg" -k %APIKEY% -s %SOURCE%

rem dotnet nuget push "%PACKAGES%\DryIoc.Microsoft.Hosting.1.0.0-preview-01.nupkg" -k %APIKEY% -s %SOURCE%

echo:
echo:Publishing completed.

pause
