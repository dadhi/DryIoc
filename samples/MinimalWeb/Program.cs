using DryIoc;
using DryIoc.Microsoft.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// DryIoc with compile-time container: IWeatherService is resolved statically from
// CompileTimeDI/Container.Generated.cs without reflection — AOT-publish-friendly.
// All other services (ASP.NET, logging, etc.) fall through to the standard runtime DI.
var container = new Container(
    DryIocAdapter.MicrosoftDependencyInjectionRules
        .WithCompileTimeContainer(CompileTimeContainer.Instance));

builder.Host.UseServiceProviderFactory(new DryIocServiceProviderFactory(container));

var app = builder.Build();

app.MapGet("/", () => "MinimalWeb + DryIoc compile-time DI");
app.MapGet("/weather", (IWeatherService svc) => svc.GetForecasts());

app.Run();

// Expose the implicit Program class for WebApplicationFactory in integration tests.
public partial class Program { }