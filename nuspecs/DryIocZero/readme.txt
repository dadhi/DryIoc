Hello Sailor,

The starting point is the Registrations.ttinclude file which should be included into your project.
If it is not included, please do the checks below.


Check 1: If you don't see any new files in your project, please copy all the files from the
%USERPROFILE%/.nuget/packages/DryIocZero/<version>/tools/content

Checj 2: If you see errors in Container.Generated.tt file with DryIoc and/or ExpressionToCodeLib 
namespaces not resolved, 
please copy the "%USERPROFILE%/.nuget/packages/DryIocZero/<version>/build/DryIocZero.props" into
your project, edit the project file and add to the top the following Import:

<Import Project="DryIocZero.props" />
