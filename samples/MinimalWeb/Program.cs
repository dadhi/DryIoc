using DryIoc;
using DryIoc.Microsoft.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var container = new Container(Rules.MicrosoftDependencyInjectionRules);

// register natively with DryIoc
container.Register<Bar>();

builder.Host.UseServiceProviderFactory(new DryIocServiceProviderFactory(container));

// register via Services collection
builder.Services.AddTransient<Foo>();

var app = builder.Build();

app.MapGet("/", (Foo foo) => $"Hello world with `{foo}`");
app.MapGet("/bar", (Foo foo, Bar bar) => $"Hello world with `{foo}` and `{bar}`");

app.Run();

public class Foo
{
    public Foo(Bar bar = null) {}
}

public class Bar {}