using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var container = new Container(Rules.MicrosoftDependencyInjectionRules.WithConcreteTypeDynamicRegistrations(reuse: Reuse.Transient));

// register natively with DryIoc
container.Register<Bar>();

builder.Host.UseServiceProviderFactory(new DryIocServiceProviderFactory(container));

// register via Services collection
builder.Services.AddTransient<IFoo, Foo>();

var app = builder.Build();

app.MapGet("/", (IFoo foo) => $"Hello world with `{foo}`");
app.MapGet("/bar", (IFoo foo, Bar bar) => $"Hello world with `{foo}` and `{bar}`");

// enabled by adding the `WithConcreteTypeDynamicRegistrations` container rules
app.MapGet("/pooh", ([FromServices]Pooh p) => $"Hello world with concrete type `{p}`");

app.Run();

public interface IFoo {}

public class Foo : IFoo
{
    public Foo(Bar bar = null) {}
}

public class Bar {}

public class Pooh {}