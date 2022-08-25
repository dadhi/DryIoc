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

Services (94):
000 - IOptions<>,
001 - IOptionsSnapshot<>,
002 - IOptionsMonitor<>,
003 - IOptionsFactory<>,
004 - IOptionsMonitorCache<>,
005 - ILoggerFactory,
006 - ILogger<>,
007 - IConfigureOptions<LoggerFilterOptions>,
008 - IConfigureOptions<LoggerFilterOptions>,
009 - ILoggerProviderConfigurationFactory,
010 - ILoggerProviderConfiguration<>,
011 - IConfigureOptions<LoggerFilterOptions>,
012 - IOptionsChangeTokenSource<LoggerFilterOptions>,
013 - LoggingConfiguration,
014 - ConsoleFormatter,
015 - IConfigureOptions<JsonConsoleFormatterOptions>,
016 - IOptionsChangeTokenSource<JsonConsoleFormatterOptions>,
017 - ConsoleFormatter,
018 - IConfigureOptions<ConsoleFormatterOptions>,
019 - IOptionsChangeTokenSource<ConsoleFormatterOptions>,
020 - ConsoleFormatter,
021 - IConfigureOptions<SimpleConsoleFormatterOptions>,
022 - IOptionsChangeTokenSource<SimpleConsoleFormatterOptions>,
023 - ILoggerProvider,
024 - IConfigureOptions<ConsoleLoggerOptions>,
025 - IOptionsChangeTokenSource<ConsoleLoggerOptions>,
026 - ILoggerProvider,
027 - LoggingEventSource,
028 - ILoggerProvider,
029 - IConfigureOptions<LoggerFilterOptions>,
030 - IOptionsChangeTokenSource<LoggerFilterOptions>,
031 - ILoggerProvider,
032 - IConfigureOptions<LoggerFactoryOptions>,
033 - IWebHostEnvironment,
034 - IHostingEnvironment,
035 - IApplicationLifetime,
036 - IConfigureOptions<GenericWebHostServiceOptions>,
037 - DiagnosticListener,
038 - DiagnosticSource,
039 - ActivitySource,
040 - DistributedContextPropagator,
041 - IHttpContextFactory,
042 - IMiddlewareFactory,
043 - IApplicationBuilderFactory,
044 - IConnectionListenerFactory,
045 - IConfigureOptions<KestrelServerOptions>,
046 - IServer,
047 - IConfigureOptions<KestrelServerOptions>,
048 - IPostConfigureOptions<HostFilteringOptions>,
049 - IOptionsChangeTokenSource<HostFilteringOptions>,
050 - IStartupFilter,
051 - IStartupFilter,
052 - IConfigureOptions<ForwardedHeadersOptions>,
053 - IInlineConstraintResolver,
054 - ObjectPoolProvider,
055 - ObjectPool<UriBuildingContext>,
056 - TreeRouteBuilder,
057 - RoutingMarkerService,
058 - IConfigureOptions<RouteOptions>,
059 - EndpointDataSource,
060 - ParameterPolicyFactory,
061 - MatcherFactory,
062 - DfaMatcherBuilder,
063 - DfaGraphWriter,
064 - Lifetime,
065 - EndpointMetadataComparer,
066 - LinkGenerator,
067 - IEndpointAddressScheme<string>,
068 - IEndpointAddressScheme<RouteValuesAddress>,
069 - LinkParser,
070 - EndpointSelector,
071 - MatcherPolicy,
072 - MatcherPolicy,
073 - MatcherPolicy,
074 - TemplateBinderFactory,
075 - RoutePatternTransformer,
076 - IConfigureOptions<RouteHandlerOptions>,
077 - IConfigureOptions<GenericWebHostServiceOptions>,
078 - IConfiguration,
079 - IOptionsChangeTokenSource<TransientFaultHandlingOptions>,
080 - IConfigureOptions<TransientFaultHandlingOptions>,
081 - ExampleService,
082 - HttpMessageHandlerBuilder,
083 - DefaultHttpClientFactory,
084 - IHttpClientFactory,
085 - IHttpMessageHandlerFactory,
086 - ITypedHttpClientFactory<>,
087 - Cache,
088 - IHttpMessageHandlerBuilderFilter,
089 - HttpClientMappingRegistry,
090 - HttpClient,
091 - IConfigureOptions<HttpClientFactoryOptions>,
092 - TypedClient,
093 - IConfigureOptions<HttpClientFactoryOptions>


Http factory: DefaultHttpClientFactory
Never called, as it was replaced
Created client: HttpClient
Options: `{"Enabled":true,"AutoRetryDelay":"00:00:07"}`
*/