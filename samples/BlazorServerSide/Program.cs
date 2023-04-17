using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using BlazorServerSide.Data;

// support for injection of required properties
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

using Serilog;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

static PropertyOrFieldServiceInfo? TryInjectRequiredProperties(MemberInfo memberInfo, Request request)
{
    if (memberInfo is not PropertyInfo p)
        return null;

    // avoid injection of properties if the selected constructor already supposed to set them 
    var ctor = request.SelectedConstructor;
    if (ctor != null && ctor.GetCustomAttribute<SetsRequiredMembersAttribute>() != null) 
    {
#if DEBUG
        var logger = request.Container.GetService<ILogger<DryIoc.IContainer>>();
        logger?.LogDebug("Skipping injection of required property `{PropertyName}` because the selected constructor `{Constructor}` already supposed to set it for service `{ServiceType}`.", p.Name, ctor, request.ServiceType);
#endif
        return null;
    }
    
    return p.GetCustomAttribute<RequiredMemberAttribute>() == null
        ? null
        : PropertyOrFieldServiceInfo.Of(p).WithDetails(ServiceDetails.Of(IfUnresolved.Throw));
}

var tryInjectRequiredProperties = PropertiesAndFields.All(withFields: false, serviceInfo: TryInjectRequiredProperties);

var container = new Container(Rules.MicrosoftDependencyInjectionRules.With(propertiesAndFields: tryInjectRequiredProperties));

// Here it goes the integration with the existing DryIoc container
var diFactory = new DryIocServiceProviderFactory(container, RegistrySharing.Share);
builder.Host.UseServiceProviderFactory(diFactory);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true;
});

// Configure Serilog Logger
var logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.File("Logs/Log_.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog(logger);


builder.Services.AddTransient<WeatherForecast>();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton<IRandomProvider, SharedRandomProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
