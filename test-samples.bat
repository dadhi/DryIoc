@echo off
echo:## Sample Tests (MinimalWeb)
dotnet test -v:minimal -c:Release samples/MinimalWeb.Tests/MinimalWeb.Tests.csproj
if %ERRORLEVEL% neq 0 exit /b 1
echo:## Sample Tests: OK
