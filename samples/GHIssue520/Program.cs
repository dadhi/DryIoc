using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.Configure<TransientFaultHandlingOptions>(
    builder.Configuration.GetSection(key: nameof(TransientFaultHandlingOptions)));

services.AddSingleton<ExampleService>();

services.AddHttpClient(Options.DefaultName)
    .ConfigurePrimaryHttpMessageHandler(c =>
    {
        Console.WriteLine("Never called, as it was replaced");
        return new HttpClientHandler();
    });

services.AddHttpClient<TypedClient>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        Console.WriteLine("TypedClient called");
        return new HttpClientHandler();
    });


Console.WriteLine($"Services ({services.Count}):\n{(string.Join(",\n", services.Select((x, i) => $"{i:000} - {x.ServiceType.Print()}").ToList()))}\n\n");

var container = new Container(r =>
    r.WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Replace))
.WithDependencyInjectionAdapter(services);

var app = builder.Build();

var factory = container.Resolve<IHttpClientFactory>();
Console.WriteLine("Http factory: " + factory.GetType().Name);

using var client = factory.CreateClient();
Console.WriteLine("Created client: " + client.GetType().Name);

container.Resolve<ExampleService>();

public class TypedClient {}

public class ExampleService 
{ 
    public ExampleService(IOptions<TransientFaultHandlingOptions> options) 
    {
        var json = JsonSerializer.Serialize(options.Value);
        Console.WriteLine($"Options: `{json}`");
    }
}



public class TransientFaultHandlingOptions
{
    public bool Enabled { get; set; }
    public TimeSpan AutoRetryDelay { get; set; }
}

/*
Program run output:


Services (20):
IOptions`1,
IOptionsSnapshot`1,
IOptionsMonitor`1,
IOptionsFactory`1,
IOptionsMonitorCache`1,
ILoggerFactory,
ILogger`1,
IConfigureOptions`1,
HttpMessageHandlerBuilder,
DefaultHttpClientFactory,
IHttpClientFactory,
IHttpMessageHandlerFactory,
ITypedHttpClientFactory`1,
Cache,
IHttpMessageHandlerBuilderFilter,
HttpClientMappingRegistry,
HttpClient,
IConfigureOptions`1,
TypedClient,
IConfigureOptions`1
Http factory: DefaultHttpClientFactory
Never called, as it was replaced
Created client: HttpClient
*/