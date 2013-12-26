set /p apikey= < apikey.txt
set version=%1
call nuget push ..\bin\nuget\DryIoc.%version%.nupkg -Source https://nuget.org -ApiKey %apikey%
call nuget push ..\bin\nuget\DryIoc.dll.%version%.nupkg -Source https://nuget.org -ApiKey %apikey%

call nuget push ..\bin\nuget\DryIoc.MefAttributedModel.%version%.nupkg -Source https://nuget.org -ApiKey %apikey%
call nuget push ..\bin\nuget\DryIoc.MefAttributedModel.dll.%version%.nupkg -Source https://nuget.org -ApiKey %apikey%