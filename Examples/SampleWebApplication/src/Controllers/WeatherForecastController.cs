namespace SampleWebApplication.Controllers;

using FunctionalDDD.Results.Asp;
using FunctionalDDD.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

[ApiController]
[Produces("application/json")]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] s_summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public ActionResult<WeatherForecast[]> Get()
    {
        long from = 0;
        long to = 4;
        var strRange = Request.Headers[HeaderNames.Range].FirstOrDefault();
        if (RangeHeaderValue.TryParse(strRange, out var range))
        {
            var firstRange = range.Ranges.First();
            from = firstRange.From ?? from;
            to = firstRange.To ?? to;
        }
        return Result.Success(() =>
            {
                var allData = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = s_summaries[Random.Shared.Next(s_summaries.Length)]
                }).ToArray();

                WeatherForecast[] data = allData.Skip((int)from).Take((int)(to - from + 1)).ToArray();
                var contentRangeHeaderValue = new ContentRangeHeaderValue(from, to, allData.Length) { Unit = "items" };
                return (RangedValue: contentRangeHeaderValue, Data: data);
            })
        .ToPartialOrOkActionResult(this, static r => r);
    }
}
