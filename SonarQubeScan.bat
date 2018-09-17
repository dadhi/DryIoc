@echo off

set /p sonartoken=<sonartoken.txt

SonarScanner.MSBuild.exe begin /k:"dadhi_DryIoc" /d:sonar.organization="dadhi-github" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login="%sonartoken%"

MSBuild.exe DryIoc.sln /t:Rebuild /v:minimal /m /fl /flp:LogFile=MSBuildRebuild.log

SonarScanner.MSBuild.exe end /d:sonar.login="%sonartoken%"

echo:Success!
pause