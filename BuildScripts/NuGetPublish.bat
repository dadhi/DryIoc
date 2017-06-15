@echo off
pushd ".."

set NUGET=".nuget\NuGet.exe"
set PACKAGEDIR="bin\NuGetPackages"
set /p APIKEY=<ApiKey.txt

rem DryIoc
rem %NUGET% push "%PACKAGEDIR%\DryIoc.dll.2.11.3.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.2.11.3.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.Internal.2.11.3.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.Microsoft.DependencyInjection
rem %NUGET% push "%PACKAGEDIR%\DryIoc.Microsoft.DependencyInjection.1.0.2.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIocZero
rem %NUGET% push "%PACKAGEDIR%\DryIocZero.3.0.0-preview-01.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIocAttributes
rem %NUGET% push "%PACKAGEDIR%\DryIocAttributes.3.1.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIocAttributes.dll.3.1.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.MefAttributedModel
%NUGET% push "%PACKAGEDIR%\DryIoc.MefAttributedModel.3.1.1.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
%NUGET% push "%PACKAGEDIR%\DryIoc.MefAttributedModel.dll.3.1.1.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.Web
rem %NUGET% push "%PACKAGEDIR%\DryIoc.Web.2.2.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.Web.dll.2.2.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem rem DryIoc.Mvc
rem %NUGET% push "%PACKAGEDIR%\DryIoc.Mvc.2.2.2.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.Mvc.dll.2.2.2.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.WebApi
rem %NUGET% push "%PACKAGEDIR%\DryIoc.WebApi.2.2.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.WebApi.dll.2.2.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.SignalR
rem %NUGET% push "%PACKAGEDIR%\DryIoc.SignalR.2.1.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.SignalR.dll.2.1.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.Owin
rem %NUGET% push "%PACKAGEDIR%\DryIoc.Owin.2.2.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.Owin.dll.2.2.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.WebApi.Owin
rem %NUGET% push "%PACKAGEDIR%\DryIoc.WebApi.Owin.2.1.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.WebApi.Owin.dll.2.1.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.CommonServiceLocator
rem %NUGET% push "%PACKAGEDIR%\DryIoc.CommonServiceLocator.2.2.1.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.CommonServiceLocator.dll.2.2.1.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

popd
pause

echo: 
echo:Packaging succeeded.
