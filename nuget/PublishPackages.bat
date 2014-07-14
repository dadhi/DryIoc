set NUGET="..\.nuget\NuGet.exe"
set /p APIKEY=<apikey.txt

for %%P in ("..\bin\NuGetPackages\*.nupkg") do (
%NUGET% push %%P -Source https://nuget.org -ApiKey %APIKEY%
)

pause