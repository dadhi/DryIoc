Hello Sailor,

Welcome to the compile-time generated dependency injection.


First, a small technical information:

- The DryIoc source code package uses T4 template system for the code generation - the file extensions are ".tt" and ".ttinclude"
- When first saving (or re-saving) the "Container.Generated.tt" file in Visual Studio (or JetBrains Rider) you may get a prompt - accept it. 
- If everything is fine you will see the generated "Container.Generated.cs" file under the "Container.Generated.tt" in Solution Explorer
- The "Container.Generated.cs" will contain the generated methods to create the services registered in "CompileTimeRegistrations.ttinclude"

You start by adding the registrations into "CompileTimeRegistrations.ttinclude" file.
The file contains `TODO:` comments to guide with the process.
Additionally it contains the example registrations for the types from "CompileTimeGenerate.Example.cs" - you may remove them when you know how things work.


Troubleshooting:

1. If you see errors in "Container.Generated.tt" file with the namespaces not being resolved, 
please ensure that "CompileTimeDependencies.props" is copied to your project from the DryIoc package installation, 
e.g. from the "%USERPROFILE%\.nuget\packages\DryIoc\<version>\build\CompileTimeDependencies.props" 

2. Edit the target ".csproj" file and add closer to the top the following Import:

<Import Project="CompileTimeDependencies.props" />

3. Check the "CompileTimeDependencies.props" to ensure the path to "ExpressionToCodeLib.dll" points to the correct location in DryIoc package installation.
Modify the file accordingly.
