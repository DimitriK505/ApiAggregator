
using ApiAggregator.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace ApiAggregator.Domain;

/// <summary>
/// Represents an API call to retrieve current weather information for Athens.
/// </summary>
/// <remarks>This class is responsible for interacting with the weather API endpoint to fetch weather data.  It
/// includes caching functionality to minimize redundant API calls, storing results for 15 minutes.</remarks>
public class WeatherApiCall : IApiCall
{
    public string EndpointName => "WeatherEndpoint";
    private readonly EndpointSettings _settings;
    private readonly IMemoryCache _cache;
    private readonly IAggregatorStatisticsService _aggregatorStatisticsService;
    private const string WeatherCacheKey = "WeatherCacheKey";

    public WeatherApiCall(IOptions<EndpointSettings> settings, IMemoryCache cache, IAggregatorStatisticsService aggregatorStatisticsService)
    {
        _settings = settings.Value;
        _cache = cache;
        _aggregatorStatisticsService = aggregatorStatisticsService;
    }

    /// <summary>
    /// Calls the weather API endpoint to retrieve the current weather information for Athens.
    /// </summary>
    /// <remarks>The method caches the result for 15 minutes to reduce redundant API calls. If the cached
    /// result is available, it is returned immediately. Otherwise, the method makes an HTTP GET request to the weather
    /// API.</remarks>
    /// <param name="httpClient">The <see cref="HttpClient"/> instance used to make the HTTP request.</param>
    /// <param name="filterOptions">The filter options to apply to the request. Currently unused.</param>
    /// <param name="options">The sorting options to apply to the request. Currently unused.</param>
    /// <returns>An <see cref="EndpointResult"/> containing the result of the API call, including the weather description,
    /// temperature, and any error messages if the call fails.</returns>
    public async Task<EndpointResult> CallEndpointAsync(HttpClient httpClient, FilterOptions filterOptions, SortingOptions options)
    {
        if (_cache.TryGetValue(WeatherCacheKey, out EndpointResult? cachedResult) && cachedResult != null)
        {
            return cachedResult;
        }

        try
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await httpClient.GetAsync($"https://api.openweathermap.org/data/2.5/weather/?q=Athens&units=metric&appid={_settings.WeatherApiKey}");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            stopwatch.Stop();
            _aggregatorStatisticsService.RecordApiCall(EndpointName, stopwatch.ElapsedMilliseconds);
            var json = JsonDocument.Parse(body);
            var weatherDescription = json.RootElement.GetProperty("weather")[0].GetProperty("description").GetString();
            var temperature = json.RootElement.GetProperty("main").GetProperty("temp").GetDouble();
            body = $"Weather in Athens: {weatherDescription}, Temperature: {temperature:F2} °C";

            var result = new EndpointResult
            {
                Name = EndpointName,
                IsSuccess = response != null && response.IsSuccessStatusCode,
                ResponseBody = body,
                ErrorMessage = response?.ReasonPhrase?.Trim() ?? string.Empty
            };

            _cache.Set(WeatherCacheKey, result, TimeSpan.FromMinutes(15));

            return result;
        }
        catch (Exception ex) 
        {
            return new EndpointResult
            {
                Name = EndpointName,
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

}
