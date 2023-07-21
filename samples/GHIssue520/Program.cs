using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http;

var services = new ServiceCollection();

// test gh issue 584
services.AddOptions();

// Will not work because the container is not conformed to the MS.DI rules!
// var containerIssue584 = new Container();

// Instead use this:
var containerIssue584_1 = new Container(Rules.MicrosoftDependencyInjectionRules);
containerIssue584_1.Populate(services);

// or better this:
var containerIssue584_2 = new Container().WithDependencyInjectionAdapter(services);

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

Console.WriteLine($"Services ({services.Count}):\n" + string.Join(",\n", services.Select(x => x.ServiceType.Name).ToList()));

var container = new Container(r =>
    r.WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Replace))
     .WithDependencyInjectionAdapter(services);


var factory = container.Resolve<IHttpClientFactory>();
Console.WriteLine("Http factory: " + factory.GetType().Name);

using var client = factory.CreateClient();
Console.WriteLine("Created client: " + client.GetType().Name);


public class TypedClient { }

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