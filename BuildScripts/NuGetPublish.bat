@echo off
pushd ".."

set NUGET=".nuget\NuGet.exe"
set PACKAGEDIR="bin\NuGetPackages"
set /p APIKEY=<ApiKey.txt

rem DryIoc
rem %NUGET% push "%PACKAGEDIR%\DryIoc.dll.3.0.2.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.3.0.2.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.Internal.3.0.2.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.Microsoft.DependencyInjection
rem %NUGET% push "%PACKAGEDIR%\DryIoc.Microsoft.DependencyInjection.1.0.2.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIocZero
%NUGET% push "%PACKAGEDIR%\DryIocZero.4.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIocAttributes
rem %NUGET% push "%PACKAGEDIR%\DryIocAttributes.4.0.2.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIocAttributes.dll.4.0.2.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.MefAttributedModel
rem %NUGET% push "%PACKAGEDIR%\DryIoc.MefAttributedModel.4.0.4.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
rem %NUGET% push "%PACKAGEDIR%\DryIoc.MefAttributedModel.dll.4.0.4.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.Web
%NUGET% push "%PACKAGEDIR%\DryIoc.Web.3.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
%NUGET% push "%PACKAGEDIR%\DryIoc.Web.dll.3.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.Mvc
%NUGET% push "%PACKAGEDIR%\DryIoc.Mvc.3.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
%NUGET% push "%PACKAGEDIR%\DryIoc.Mvc.dll.3.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.WebApi
%NUGET% push "%PACKAGEDIR%\DryIoc.WebApi.3.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
%NUGET% push "%PACKAGEDIR%\DryIoc.WebApi.dll.3.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.SignalR
%NUGET% push "%PACKAGEDIR%\DryIoc.SignalR.3.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
%NUGET% push "%PACKAGEDIR%\DryIoc.SignalR.dll.3.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.Owin
%NUGET% push "%PACKAGEDIR%\DryIoc.Owin.3.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
%NUGET% push "%PACKAGEDIR%\DryIoc.Owin.dll.3.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.WebApi.Owin
%NUGET% push "%PACKAGEDIR%\DryIoc.WebApi.Owin.3.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
%NUGET% push "%PACKAGEDIR%\DryIoc.WebApi.Owin.dll.3.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

rem DryIoc.CommonServiceLocator
%NUGET% push "%PACKAGEDIR%\DryIoc.CommonServiceLocator.3.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%
%NUGET% push "%PACKAGEDIR%\DryIoc.CommonServiceLocator.dll.3.0.0.nupkg" -Source https://nuget.org -ApiKey %APIKEY%

echo: 
echo:Publishing completed.

popd
pause
