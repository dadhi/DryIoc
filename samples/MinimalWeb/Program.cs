using DryIoc;
using DryIoc.Microsoft.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var container = new MyContainer(DryIocAdapter.MicrosoftDependencyInjectionRules);

// register natively with DryIoc
container.Register<Bar>();

builder.Host.UseServiceProviderFactory(new DryIocServiceProviderFactory(container));

// register via Services collection
builder.Services.AddTransient<Foo>();

// some fun with container extensibility for #539
builder.Services.AddScoped<ScopedAutomaticallyResolved>();
builder.Services.AddSingleton<SingletonAutomaticallyResolved>();

var app = builder.Build();

app.MapGet("/", (Foo foo) => $"Hello world with `{foo}`, try /bar to get bar.");
app.MapGet("/bar", (Foo foo, Bar bar) => $"Hello world with `{foo}` and `{bar}`");

app.Run();

public class Foo
{
    public Foo(Bar bar = null) { }
}

public class Bar { }

public class ScopedAutomaticallyResolved
{
    public readonly SingletonAutomaticallyResolved Singleton;
    public ScopedAutomaticallyResolved(SingletonAutomaticallyResolved singleton)
    {
        Singleton = singleton;
        Console.WriteLine("ScopedAutomaticallyResolved created");
    }
}

public class SingletonAutomaticallyResolved
{
    public SingletonAutomaticallyResolved()
    {
        Console.WriteLine("SingletonAutomaticallyResolved created");
    }
}

public sealed class MyContainer : Container
{
    public MyContainer(Rules rules) : base(rules) { }

    public override IContainer WithNewOpenScope()
    {
        var scope = base.WithNewOpenScope();
        scope.Resolve<ScopedAutomaticallyResolved>();
        return scope;
    }
}

public class MyDryIocServiceProviderFactory : DryIocServiceProviderFactory
{
    public MyDryIocServiceProviderFactory(IContainer container) : base(container) { }

    public override IServiceProvider CreateServiceProvider(DryIocServiceProvider provider)
    {
        provider.Container.Resolve<SingletonAutomaticallyResolved>();
        return provider;
    }
}