using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace MinimalWeb.Tests;

[TestFixture]
public class ApiTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Root_returns_200()
    {
        var response = await _client.GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Weather_returns_5_forecasts()
    {
        var response = await _client.GetAsync("/weather");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();
        Assert.That(forecasts, Has.Length.EqualTo(5));
    }

    [Test]
    public void IWeatherService_resolved_from_compile_time_container()
    {
        using var scope = _factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IWeatherService>();
        // Compile-time container always creates a new WeatherService instance (transient).
        Assert.That(svc, Is.InstanceOf<WeatherService>());
    }
}
