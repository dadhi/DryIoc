@echo off
echo:BUILD AND RUN TESTS:

rem Restores packages for the first time
rem call dnu restore

rem dnx -p "DryIoc.Dnx.UnitTests" test
rem dnx -p "DryIoc.MefAttributedModel.Dnx.UnitTests" test
rem dnx -p "DryIoc.Dnx.DependencyInjection.Specification.Tests" test

echo:BUILD AND PACKAGE:
call dnu pack DryIoc.Dnx --configuration Release
call dnu pack DryIocAttributes.Dnx --configuration Release
call dnu pack DryIoc.MefAttributedModel.Dnx --configuration Release
call dnu pack DryIoc.Dnx.DependencyInjection --configuration Release

pause