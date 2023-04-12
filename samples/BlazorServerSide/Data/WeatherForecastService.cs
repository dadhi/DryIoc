namespace BlazorServerSide.Data;

public class WeatherForecastService
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public required Func<WeatherForecast> WeatherForecastFactory { protected get; init; }

    public Task<WeatherForecast[]> GetForecastAsync(DateOnly startDate)
    {
        return Task.FromResult(Enumerable.Range(1, 5).Select(index =>
        {
            var f = WeatherForecastFactory();
            f.Date = startDate.AddDays(index);
            f.TemperatureC = Random.Shared.Next(-20, 55);
            f.Summary = Summaries[Random.Shared.Next(Summaries.Length)];
            return f;
        }).ToArray());
    }
}
