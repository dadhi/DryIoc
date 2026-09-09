public interface IWeatherService
{
    IReadOnlyList<WeatherForecast> GetForecasts(int count = 5);
}

public class WeatherService : IWeatherService
{
    private static readonly string[] Summaries =
        ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    public IReadOnlyList<WeatherForecast> GetForecasts(int count = 5) =>
        [.. Enumerable.Range(1, count).Select(i =>
            new WeatherForecast(
                DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                Random.Shared.Next(-20, 55),
                Summaries[Random.Shared.Next(Summaries.Length)]))];
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
