// Sample from the official https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using static System.Console;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddTransient<TransientDisposable>();
builder.Services.AddScoped<ScopedDisposable>();
builder.Services.AddSingleton<SingletonDisposable>();

// Integrate DryIoc
builder.ConfigureContainer(new DryIocServiceProviderFactory(),
    // optional configuration action
    serviceProvider =>
    {
        // Factory returns the DryIocServiceProvider, but you may access the DryIoc container via the Container field.
        // Note: avoid storing the container instance in your app, because it depends on the current scope you're in.
        // Instead, if required, resolve it from the services as shown below.
        var c = serviceProvider.Container;
        c.Register<Bazz>();
    });


using IHost host = builder.Build();

// Resolve the actual DryIoc.IContainer from the services.
var container = host.Services.GetRequiredService<IContainer>();
var serviceProvider = container.Resolve<IServiceProvider>();

Console.WriteLine($"The actual container is {container.GetType().FullName}, and the service provider is {serviceProvider.GetType().FullName}");

ExemplifyDisposableScoping(host.Services, "Scope 1");
Console.WriteLine();

ExemplifyDisposableScoping(host.Services, "Scope 2");
Console.WriteLine();

await host.RunAsync();

static void ExemplifyDisposableScoping(IServiceProvider services, string scope)
{
    Console.WriteLine($"{scope}...");

    using IServiceScope serviceScope = services.CreateScope();
    Console.WriteLine($"The actual serviceScope implementation is {serviceScope.GetType().Name}");

    IServiceProvider provider = serviceScope.ServiceProvider;

    _ = provider.GetRequiredService<TransientDisposable>();
    _ = provider.GetRequiredService<ScopedDisposable>();
    _ = provider.GetRequiredService<SingletonDisposable>();
    _ = provider.GetRequiredService<Bazz>();
}

public sealed class TransientDisposable : IDisposable
{
    public void Dispose() => Console.WriteLine($"{nameof(TransientDisposable)}.Dispose()");
}

public sealed class ScopedDisposable : IDisposable
{
    public void Dispose() => Console.WriteLine($"{nameof(ScopedDisposable)}.Dispose()");
}

public sealed class SingletonDisposable : IDisposable
{
    public void Dispose() => Console.WriteLine($"{nameof(SingletonDisposable)}.Dispose()");
}

public record Bazz(TransientDisposable TransientDep, ScopedDisposable ScopedDep, SingletonDisposable SingletonDep);