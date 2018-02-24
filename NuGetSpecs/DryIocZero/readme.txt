<#
//
// TODO: ADD TO THE PROJECT A NEW FILE "Registrations.ttinclude" WITH THE CONTENTS OF THIS WHOLE FILE
//
// "*.ttinclude" is T4 Text Template include file.
// Given you have ReSharper with ForTea extension installed you will get 
// intellisense and most of the refactorings working.
//
// TODO: Next steps
// 1. Fill-in the methods below with your registrations and resolution roots.
// 4. Re-save the "Container.Generated.tt" file. Confirm the Visual Studio prompt if any.
// 5. Check the "Container.Generated.cs" for the resolution errors.
//
#>
<#@ assembly Name="System.Core" #>
<#@ assembly Name="$(DryIocAssembly)" #>
<#@ import Namespace="DryIoc" #>
<#@ import Namespace="ImTools" #>
<#// TODO: Insert assemblies and namespaces of your services to be registered in container #>
<#+
// TODO: Specify the container and registrations ...
IContainer GetContainerWithRegistrations()
{
    var container = new Container();

    // NOTE: `RegisterDelegate` and `UseInstance` are not supported because of runtime state usage. 
    // Instead you can use `RegisterPlaceholder` to fix object graph generation, then fill in
    // placeholder using run-time `DryIocZero.RegisterDelegate` and `DryIocZero.UseInstance`

    // TODO: Add registrations ...
    // container.Register<IMyService, MyService>();
    // container.RegisterMany(new[] { MyAssembly });

    return container;
}

// TODO: For each passed registration specify what resolution roots it provides, null if none
ServiceInfo[] SpecifyResolutionRoots(ServiceRegistrationInfo reg)
{
    return reg.AsResolutionRoot ? reg.ToServiceInfo().One() : null;
}

// TODO: Additional roots to generate ...
ServiceInfo[] CustomResolutionRoots = {};
#>