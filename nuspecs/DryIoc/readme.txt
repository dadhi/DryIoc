Hello Sailor,

DryIoc now has an *optional* COMPILE-TIME dependency injection with the source package.
(previously it was available as a separate DryIocZero package)

You may ignore this information if you don't want to use the compile-time DI.
Everything will work without it!

How to use:

1. Copy contents of "%USERPROFILE%\.nuget\packages\DryIoc\<version>\CompileTimeDI\" folder
to your project - e.g. "Container.Generated.tt", "CompileTimeRegistrations.ttinclude", and "CompileTimeRegistrations.Example.cs".
2. Add your registrations into the "CompileTimeRegistrations.ttinclude" file - the file already contains
the registrations from the "CompileTimeGenerate.Example.cs", you may remove them later.
3. Save (or re-save) the "Container.Generated.tt" file in the Visual Studio or JetBrains Rider 
(you may get a prompt - accept it). If everything is fine you will see the generated "Container.Generated.cs" 
file under the "Container.Generated.tt" in Solution Explorer. The "Container.Generated.cs" will contain 
the generated methods to create the services registered in "CompileTimeRegistrations.ttinclude"


Troubleshooting:

1. If you see the errors in "Container.Generated.tt" file with the namespaces not being resolved, 
please ensure that "DryIoc.props" is copied to your project from the DryIoc package installation, 
e.g. from the "%USERPROFILE%\.nuget\packages\DryIoc\<version>\build\DryIoc.props" 
2. Edit the target ".csproj" file and add closer to the top the following Import:

<Import Project="DryIoc.props" />

3. Edit the "DryIoc.props" to ensure the path to "ExpressionToCodeLib.dll" points to the correct location in 
the DryIoc package installation.
4. If some of System assemblies are not loading try the accepted answer from the 
https://stackoverflow.com/questions/51550265/t4-template-could-not-load-file-or-assembly-system-runtime-version-4-2-0-0


For editing and viewing the T4 text template files you may use ForTea plugin for JetBrains ReSharper
https://plugins.jetbrains.com/plugin/11634-fortea (or JetBrains Rider with the native T4 support)
