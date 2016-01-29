@echo off
echo:BUILD AND RUN TESTS:

dnx -p "DryIoc.Dnx.UnitTests" test
dnx -p "DryIoc.MefAttributedModel.Dnx.UnitTests" test
dnx -p "DryIoc.Dnx.DependencyInjection.Specification.Tests" test

echo:BUILD AND PACKAGE:
call dnu pack DryIoc.Dnx --configuration Release
call dnu pack DryIoc.Dnx.DependencyInjection --configuration Release
call dnu pack DryIocAttributes.Dnx --configuration Release
call dnu pack DryIoc.MefAttributedModel.Dnx --configuration Release

pause