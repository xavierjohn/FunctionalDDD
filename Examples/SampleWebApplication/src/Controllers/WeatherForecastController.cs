namespace SampleWebApplication.Controllers;

using FunctionalDdd;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

[ApiController]
[Produces("application/json")]
[Route("[controller]")]
public class WeatherForecastController(ILogger<WeatherForecastController> logger) : ControllerBase
{
    private static readonly string[] s_summaries =
    [
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching"
    ];

    private readonly ILogger<WeatherForecastController> _logger = logger;

    [HttpGet(Name = "GetWeatherForecast")]
    public ActionResult<WeatherForecast[]> Get()
    {
        long from = 0;
        long to = 4;
        var strRange = Request.Headers[Microsoft.Net.Http.Headers.HeaderNames.Range].FirstOrDefault();
        if (RangeHeaderValue.TryParse(strRange, out var range))
        {
            var firstRange = range.Ranges.First();
            from = firstRange.From ?? from;
            to = firstRange.To ?? to;
        }
        return Result.Success<(ContentRangeHeaderValue, WeatherForecast[])>(() =>
            {
                var allData = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = s_summaries[Random.Shared.Next(s_summaries.Length)]
                }).ToArray();

                WeatherForecast[] data = allData.Skip((int)from).Take((int)(to - from + 1)).ToArray();
                var contentRangeHeaderValue = new ContentRangeHeaderValue(from, to, allData.Length) { Unit = "items" };
                return new(contentRangeHeaderValue, data);
            })
        .ToPartialOrOkActionResult(this, static r => r.Item1, static r => r.Item2);
    }
}
