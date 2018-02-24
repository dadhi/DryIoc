<#
//
// TODO: PLEASE ADD TO YOUR PROJECT 
// a new Text Template file "Registrations.ttinclude"
// and copy the contents of this file into it.
// 
//=============================================================================================
// TODO: Change the code below to
// 1. Configure DryIoc.container with your rules and conventions.
// 2. Register your services.
// 3. Identify the resolution roots: the services to be Resolved, rather then injected.
// 4. Save the DryIocZero/Container.Generated.tt class. Confirm the VisualStudio prompt if any.
// 5. Check the Container.Generated.cs for general compilation and listed resolution errors.
//=============================================================================================
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

    return container;
}

// TODO: Filter the root services to generate expressions for ...
ServiceInfo[] FilterResolutionRoots(ServiceRegistrationInfo reg)
{
    return reg.AsResolutionRoot ? reg.ToServiceInfo().One() : null;
}

// TODO: Additional roots to generate ...
ServiceInfo[] CustomResolutionRoots = {};
#>