@echo off
pushd ".."

set NUGET=".nuget\NuGet.exe"
set PACKAGEDIR="bin\NuGetPackages"
set /p APIKEY=<ApiKey.txt

rem DryIoc
%NUGET% push "%PACKAGEDIR%\DryIoc.2.8.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
%NUGET% push "%PACKAGEDIR%\DryIoc.dll.2.8.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.Microsoft.DependencyInjection
rem %NUGET% push "%PACKAGEDIR%\DryIoc.Microsoft.DependencyInjection.1.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIocZero
rem %NUGET% push "%PACKAGEDIR%\DryIocZero.2.5.1.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIocAttributes
rem %NUGET% push "%PACKAGEDIR%\DryIocAttributes.2.5.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIocAttributes.dll.2.5.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.MefAttributedModel
rem %NUGET% push "%PACKAGEDIR%\DryIoc.MefAttributedModel.2.5.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.MefAttributedModel.dll.2.5.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.Owin
rem %NUGET% push "%PACKAGEDIR%\DryIoc.Owin.2.0.2.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.Owin.dll.2.0.2.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.WebApi
rem %NUGET% push "%PACKAGEDIR%\DryIoc.WebApi.2.1.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.WebApi.dll.2.1.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.Mvc
rem %NUGET% push "%PACKAGEDIR%\DryIoc.Mvc.2.0.1.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.Mvc.dll.2.0.1.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.SignalR
rem %NUGET% push "%PACKAGEDIR%\DryIoc.SignalR.2.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.SignalR.dll.2.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.Mvc
rem %NUGET% push "%PACKAGEDIR%\DryIoc.CommonServiceLocator.2.1.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.CommonServiceLocator.dll.2.1.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

popd
pause

echo: 
echo:Packaging succeeded.
