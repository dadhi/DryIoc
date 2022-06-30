REM Restore the project using the custom config file, restoring packages to a local folder
dotnet restore --packages ../../.packages --configfile "nuget.comptime-tests.config" 

REM Build the project (no restore), using the packages restored to the local folder
dotnet build -c Debug --packages ../../.packages --no-restore
