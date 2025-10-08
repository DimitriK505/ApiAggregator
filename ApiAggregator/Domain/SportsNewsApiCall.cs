
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiAggregator.Contracts;
using ApiAggregator.Domain.Models;

namespace ApiAggregator.Domain;

/// <summary>
/// Represents an API call for retrieving and processing Champions League match results.
/// </summary>
/// <remarks>This class is responsible for interacting with an external API to fetch match data, applying
/// filtering and sorting options, and caching the results for improved performance. The cache duration is 15 minutes.
/// Use this class to retrieve formatted match results based on specific criteria.</remarks>
public class SportsNewsApiCall : IApiCall
{
    public string EndpointName => "SportsNewsEndpoint";
    private readonly EndpointSettings _settings;
    private readonly IMemoryCache _cache;
    private readonly IAggregatorStatisticsService _aggregatorStatisticsService;
    public SportsNewsApiCall(IOptions<EndpointSettings> settings, IMemoryCache cache, IAggregatorStatisticsService aggregatorStatisticsService)
    {
        _settings = settings.Value;
        _cache = cache;
        _aggregatorStatisticsService = aggregatorStatisticsService;
    }

    /// <summary>
    /// Calls the specified API endpoint to retrieve and process Champions League match results.
    /// </summary>
    /// <remarks>This method retrieves match data from an external API, applies filtering and sorting based on
    /// the provided options, and caches the result for subsequent calls with the same parameters.  The cache duration
    /// is 15 minutes.</remarks>
    /// <param name="httpClient">The <see cref="HttpClient"/> instance used to make the API request. Must not be null.</param>
    /// <param name="filterOptions">The filtering options to apply to the match results. For example, use <see
    /// cref="FilterOptions.SportNewsKeyword"/> to filter by team names.</param>
    /// <param name="sortingOptions">The sorting options to apply to the match results, such as sorting by date in ascending or descending order.</param>
    /// <returns>An <see cref="EndpointResult"/> containing the processed match results. The <see
    /// cref="EndpointResult.ResponseBody"/> will include a formatted string of match details, or a message indicating
    /// no matches were found.</returns>
    public async Task<EndpointResult> CallEndpointAsync(HttpClient httpClient, FilterOptions filterOptions, SortingOptions sortingOptions)
    {
        if(_cache.TryGetValue(GenerateCacheKey(filterOptions, sortingOptions), out EndpointResult? cachedResult) && cachedResult != null)
        {
            return cachedResult;
        }

        try
        {
            var url = "https://api.football-data.org/v4/competitions/CL/matches?season=2025&status=FINISHED";
            httpClient.DefaultRequestHeaders.Add("X-Auth-Token", _settings.SportsApiKey);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var stopwatch = Stopwatch.StartNew();
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            stopwatch.Stop();
            _aggregatorStatisticsService.RecordApiCall(EndpointName, stopwatch.ElapsedMilliseconds);

            var json = JsonDocument.Parse(content);
            if (content.Contains("\"source\":\"PollyFallback\""))
            {
                return new EndpointResult
                {
                    Name = EndpointName,
                    IsSuccess = response.IsSuccessStatusCode,
                    ResponseBody = json?.RootElement.GetProperty("message").GetString() ?? string.Empty,
                    ErrorMessage = response?.ReasonPhrase?.Trim() ?? string.Empty
                };
            }
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true 
            };

            var data = JsonSerializer.Deserialize<MatchResponse>(json, options);

            if (data == null || data.Matches == null || data.Matches.Count == 0)
            {
                return new EndpointResult
                {
                    Name = EndpointName,
                    IsSuccess = true,
                    ResponseBody = "No Champions League matches in the last 7 days.",
                    ErrorMessage = string.Empty
                };
            }

            if(sortingOptions.SortBy == SortBy.Date)
            {
                data.Matches = sortingOptions.SortOrder == SortOrder.Asc ? 
                    data.Matches.OrderBy(m => m.UtcDate).ToList() : 
                    data.Matches.OrderByDescending(m => m.UtcDate).ToList();
            }

            var allArticlesText = new StringBuilder("Champions League Results (Last 7 Days):\n");
            foreach (var match in data.Matches)
            {
                if(string.IsNullOrEmpty(filterOptions.SportNewsKeyword) || 
                    match.HomeTeam.Name.Contains(filterOptions.SportNewsKeyword, StringComparison.CurrentCultureIgnoreCase) || 
                    match.AwayTeam.Name.Contains(filterOptions.SportNewsKeyword, StringComparison.CurrentCultureIgnoreCase))
                {
                    allArticlesText.AppendLine($"{match.UtcDate:yyyy-MM-dd} - {match.HomeTeam.Name} {match.Score.FullTime.Home} : {match.Score.FullTime.Away} {match.AwayTeam.Name}");
                }
            }

            var result = new EndpointResult
            {
                Name = EndpointName,
                IsSuccess = response != null && response.IsSuccessStatusCode,
                ResponseBody = allArticlesText.ToString(),
                ErrorMessage = response?.ReasonPhrase?.Trim() ?? string.Empty
            };

            _cache.Set(GenerateCacheKey(filterOptions, sortingOptions), result, TimeSpan.FromMinutes(15)); //ToDo add it to settings

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

    private string GenerateCacheKey(FilterOptions filterOptions, SortingOptions sortingOptions)
    {
        return $"SportsNewsCacheKey_{filterOptions.SportNewsKeyword}_{sortingOptions.SortBy}_{sortingOptions.SortOrder}";
    }
}

 