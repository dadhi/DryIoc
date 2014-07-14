set NUGET="..\.nuget\NuGet.exe"
set /p APIKEY=<apikey-staging.txt

for %%P in ("..\bin\NuGetPackages\*.nupkg") do (    
%NUGET% push %%P -Source https://staging.nuget.org -ApiKey %APIKEY%	
)

pause