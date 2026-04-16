using Application.Common.Models;
using ECommerce.Server.Infrastructure;

namespace ECommerce.Server.Endpoints
{
    public class WeatherForecast : EndpointGroupBase
    {
        public override void Map(RouteGroupBuilder groupBuilder)
        {
            groupBuilder.MapGet(GetWeatherForecast)
                .WithSummary("Weather forecast")
                .WithDescription("Get the random weather forcast based on the GetWeatherForecast")
                .Produces<WeatherForecastDto>();
        }

        static IResult GetWeatherForecast()
        {
            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecastDto
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
                )
            ).ToArray();

            return Results.Ok(forecast);

        }
    }
}
