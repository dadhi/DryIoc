namespace UsingExample;

using DryIoc;
using DryIoc.ImTools;
using Example;

public static class MyCompileTimeDI
{
    // TODO: Specify the container and registrations ...
    public static IContainer RegisterInContainer()
    {
        var container = new Container();

        // TODO: Register services in compile-time using the same DryIoc API
        // or move her the part (or all) of already written runtime registrations.

        // These are example registrations to build the IService resolution root and its dependencies
        container.Register<IService, MyService>();
        container.Register<IDependencyA, DependencyA>();
        container.Register(typeof(DependencyB<>), setup: Setup.With(asResolutionCall: true));

        // Note that `RegisterDelegate`, `RegisterInstance`, `Use` methods are not supported because of runtime state use.
        // Instead you may `RegisterPlaceholder` to put a hole in the generated object graph,
        // then you can fill it with the runtime registration, e.g. `runtimeContainer.Register<RuntimeDependency>();`
        //container.RegisterPlaceholder<RuntimeDependencyC>();

        container.Register<BaseA, KeyedA>(serviceKey: "keyed");
        container.Register<BaseA, NonKeyedA>();
        container.Register<BaseAConsumer>(setup: Setup.With(asResolutionCall: true));

        // You may batch register assemblies as well
        // container.RegisterMany(new[] { MyAssembly });

        return container;
    }

    // TODO: For each passed registration specify what resolution roots it provides, null if none
    public static ServiceInfo[] SpecifyResolutionRoots(ServiceRegistrationInfo reg) =>
        reg.AsResolutionRoot ? reg.ToServiceInfo().One() : null;

    // TODO: Alternatively, specify the resolution roots explicitly
    public static ServiceInfo[] CustomResolutionRoots =
    {
        ServiceInfo.Of<Example.IService>(),
        ServiceInfo.Of<BaseAConsumer>(),
    };

    // TODO: Specify the service namespaces to be imported via `using` instead of qualifying their types with the full names.
    // You may generate the Container.Generated.cs first, then look what you want to move to `using`
    public static string[] NamespaceUsings =
    {
        nameof(Example),
        //"Foo.Bar.Buzz",
    };
}
