using System.Text.Json;
using static CBTW_TEST.Web.Components.Pages.Counter;

namespace CBTW_TEST.Web;

public class WeatherApiClient(HttpClient httpClient)
{
    public async Task<WeatherForecast[]> GetWeatherAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        List<WeatherForecast>? forecasts = null;

        await foreach (var forecast in httpClient.GetFromJsonAsAsyncEnumerable<WeatherForecast>("/weatherforecast", cancellationToken))
        {
            if (forecasts?.Count >= maxItems)
            {
                break;
            }
            if (forecast is not null)
            {
                forecasts ??= [];
                forecasts.Add(forecast);
            }
        }

        return forecasts?.ToArray() ?? [];
    }

    public async Task<List<BookMatchResultDto>> StartDiscoveryAsync(string messyInput)
    {
        var response = await httpClient.PostAsync($"/?messyInput={Uri.EscapeDataString(messyInput)}", null);
        response.EnsureSuccessStatusCode();
        var workflowResult = await response.Content.ReadFromJsonAsync<WorkflowResultDto>();
        if (workflowResult == null || !workflowResult.WasSuccess)
        {
            throw new Exception("The AI Discovery engine failed to process the request.");
        }
        if (workflowResult.Result == null) return new List<BookMatchResultDto>();

        var jsonString = workflowResult.Result.ToString();
        var results = JsonSerializer.Deserialize<List<BookMatchResultDto>>(jsonString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return results ?? new List<BookMatchResultDto>();
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
