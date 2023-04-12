using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using BlazorServerSide.Data;

// to get the required properties
using System.Reflection;
using System.Runtime.CompilerServices;

using Serilog;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// todo: @wip take into account constructor with SetsRequiredMembers attribute
var requiredProperties = PropertiesAndFields.All(withFields: false, 
    serviceInfo: (p, _) => p.GetCustomAttribute<RequiredMemberAttribute>() != null ? PropertyOrFieldServiceInfo.Of(p) : null);

var container = new Container(Rules.MicrosoftDependencyInjectionRules.With(propertiesAndFields: requiredProperties));


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
    .WriteTo.File("Errors/Log_.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog(logger);


builder.Services.AddSingleton<WeatherForecast>(); // will be injected as required property
builder.Services.AddSingleton<WeatherForecastService>();

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
