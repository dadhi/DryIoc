set /p apikey= < apikey.txt
set version=%1
call nuget push ..\bin\nuget\DryIoc.%version%.nupkg -Source https://staging.nuget.org -ApiKey %apikey%
call nuget push ..\bin\nuget\DryIoc.dll.%version%.nupkg -Source https://staging.nuget.org -ApiKey %apikey%