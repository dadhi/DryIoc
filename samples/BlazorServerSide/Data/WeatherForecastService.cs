namespace BlazorServerSide.Data;

public class WeatherForecastService
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    // injected by the DI container
    public required Func<WeatherForecast> WeatherForecastFactory { get; init; }

    public readonly IRandomProvider RandomProvider;
    public WeatherForecastService(IRandomProvider randomProvider) => RandomProvider = randomProvider;

    public Task<WeatherForecast[]> GetForecastAsync(DateOnly startDate)
    {
        var random = RandomProvider.Random;
        return Task.FromResult(Enumerable.Range(1, 5).Select(index =>
        {
            var f = WeatherForecastFactory();
            f.Date = startDate.AddDays(index);
            f.TemperatureC = random.Next(-20, 55);
            f.Summary = Summaries[random.Next(Summaries.Length)];
            return f;
        }).ToArray());
    }
}
