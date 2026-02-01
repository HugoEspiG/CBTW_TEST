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

    public async Task<string> StartDiscoveryAsync(string messyInput)
    {
        var response = await httpClient.PostAsync($"/?messyInput={Uri.EscapeDataString(messyInput)}", null);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<JsonElement>();
        return data.GetProperty("trackId").GetString() ?? throw new Exception("No TrackId received");
    }

    public async Task<bool> GetStatusAsync(string trackId)
    {
        var response = await httpClient.GetFromJsonAsync<JsonElement>($"/status/{trackId}");
        var isCompleted = response.GetProperty("isWorkflowCompleted").GetBoolean();
        return isCompleted;
    }

    public async Task<List<BookMatchResultDto>> GetResultsAsync(string trackId)
    {
        var response = await httpClient.GetFromJsonAsync<WorkflowResultDto>($"/WorkflowResult/{trackId}");

        if (response?.Result == null) return new List<BookMatchResultDto>();

        // Deserialización segura de JsonElement a nuestra lista
        var jsonString = response.Result.ToString();
        return JsonSerializer.Deserialize<List<BookMatchResultDto>>(jsonString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
