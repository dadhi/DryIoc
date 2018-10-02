@echo off

echo:
echo:Deleting Release and Debug folders . . .

echo:
echo:Switch to solution folder first . . .
pushd ".."
if not exist *.sln (
	echo:
	echo:ERROR: Cleaned folder does not contain solution files, that means it is probably wrong folder. So no cleaning.
	popd & exit 1
)  

for /d /r %%D IN (b?n;o?j) do (
	if exist "%%D\Release" echo "%%D\Release" & rd /s /q "%%D\Release" 
	if exist "%%D\Debug" echo "%%D\Debug" & rd /s /q "%%D\Debug" 
)

echo:
echo:Cleaning succeeded.
popd

if not "%1"=="-nopause" pause