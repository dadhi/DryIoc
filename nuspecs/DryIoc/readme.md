## Hello Sailor,

### Compile-time Dependency Injection

DryIoc has an *optional* compile-time Dependency Injection with the source package
(previously it was available as a separate DryIocZero package).

You may ignore this information if you don't want to use the compile-time DI.
Everything will work without it, but wait... you will like some compile-time DI!


### How to use [WIP]

1. Copy contents of "%USERPROFILE%\.nuget\packages\DryIoc\<version>\CompileTimeDI\" folder
to your project, e.g. "Container.Generated.tt", "CompileTimeRegistrations.ttinclude", and "CompileTimeRegistrations.Example.cs".
2. Add your registrations into the "CompileTimeRegistrations.ttinclude" file, the file already contains the registrations from the "CompileTimeGenerate.Example.cs", you may remove them or use it as a guideline.
3. [WIP] Save (or re-save) the "Container.Generated.tt" file in the Visual Studio or JetBrains Rider 
(you may get a prompt - accept it). If everything is fine you will see the generated "Container.Generated.cs" 
file under the "Container.Generated.tt" in Solution Explorer. The "Container.Generated.cs" will contain 
the generated methods to create the services registered in "CompileTimeRegistrations.ttinclude"


### T4 Text Template Transformation Tooling

First you may install the [dotnet-t4](https://www.nuget.org/packages/dotnet-t4/) CLI tool from NuGet to play and experiment with templates.

For editing and viewing the T4 text template files you may use:
-  [T4 Support extension](https://marketplace.visualstudio.com/items?itemName=zbecknell.t4-support) plugin in VS Code
-  [ForTea plugin](https://plugins.jetbrains.com/plugin/11634-fortea) for JetBrains ReSharper in Visual Studio
-  The native T4 support in JetBrains Rider :-)
