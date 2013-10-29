@echo off

set source=%1
set target=%2

echo Renaming "%source%" to "%target%" ...

echo 1. Renaming folder ...
move %source% %target%

echo 2. Renaming %source%.proj file and other files to %target%.proj ...
cd %target%
ren %source%.* %target%.*

echo 3. Renaming "%source%" to "%target%" in .sln file ...