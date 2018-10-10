Hello Sailor,

Your starting point is the Registrations.ttinclude file that should be included in project now.
If not included, please do checks below.


CHECK 1: If you don't see any new files in your project, please copy all files from
%USERPROFILE%/.nuget/packages/DryIocZero/<version>/tools/content

CHECK 2: If you see errors in Container.Generated.tt file of DryIoc and ExpressionToCodeLib 
namespaces not resolved, 
please copy "%USERPROFILE%/.nuget/packages/DryIocZero/<version>/build/DryIocZero.props" into
your project, edit the project file and add closer to top the following Import:

<Import Project="DryIocZero.props" />
