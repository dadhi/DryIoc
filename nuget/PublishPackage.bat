set /p apikey=<apikey.txt
set dryiocv=1.2.2
call nuget push ..\bin\nuget\DryIoc.%dryiocv%.nupkg -Source https://nuget.org -ApiKey %apikey%
call nuget push ..\bin\nuget\DryIoc.dll.%dryiocv%.nupkg -Source https://nuget.org -ApiKey %apikey%

REM set attrmodelv=1.2.0
REM call nuget push ..\bin\nuget\DryIoc.MefAttributedModel.%attrmodelv%.nupkg -Source https://nuget.org -ApiKey %apikey%
REM call nuget push ..\bin\nuget\DryIoc.MefAttributedModel.dll.%attrmodelv%.nupkg -Source https://nuget.org -ApiKey %apikey%

pause