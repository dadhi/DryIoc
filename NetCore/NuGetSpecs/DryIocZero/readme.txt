Hello Sailor,

The starting point is `Registrations.ttinclude` file that should be included in project now.
If not included, please do checks below.


Check 1: If you don't see any new files in the project, please copy all files from

%USERPROFILE%/.nuget/packages/DryIocZero/4.0.0/tools/content


Check 2: If you see errors in Container.Generated.tt file of DryIoc and ExpressionToCodeLib 
namespaces not resolved, 
please copy %USERPROFILE%/.nuget/packages/DryIocZero/4.0.0/build/DryIocZero.props into
the project, edit the project file and add at top (where you have a Import tags) another Import:  

<Import Project="DryIocZero.props" />
