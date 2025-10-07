namespace ApiAggregator.Domain;

/// <summary>
/// Provides functionality to aggregate data from multiple API endpoints, applying filtering and sorting options.
/// </summary>
/// <remarks>This service is designed to handle concurrent calls to multiple API endpoints, aggregate their
/// results,  and apply filtering and sorting criteria. It is intended for scenarios where data needs to be retrieved 
/// and combined from multiple sources in a consistent and efficient manner.</remarks>
public class ApiAggregatorService
{
    private readonly IEnumerable<IApiCall> _apiAggregatorCalls;
    private readonly HttpClient _httpClient;

    public const string ApiAggregatorClientName = "ApiAggregatorClient";
    public ApiAggregatorService(IEnumerable<IApiCall> apiAggregatorCalls, HttpClient httpClient)
    {
        _apiAggregatorCalls = apiAggregatorCalls;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Aggregates results from multiple API endpoints based on the specified filter and sorting options.
    /// </summary>
    /// <remarks>This method concurrently calls multiple API endpoints and aggregates their results. The
    /// filtering  and sorting options are applied to each endpoint call individually.</remarks>
    /// <param name="filterOptions">The filtering criteria to apply when retrieving data from the endpoints.</param>
    /// <param name="sortingOptions">The sorting criteria to apply to the aggregated results.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection  of <see
    /// cref="EndpointResult"/> objects representing the aggregated results from all endpoints.</returns>
    public async Task<IEnumerable<EndpointResult>> AggregateAsync(FilterOptions filterOptions, SortingOptions sortingOptions)
    {
        var tasks = _apiAggregatorCalls.Select(x => x.CallEndpointAsync(_httpClient, filterOptions, sortingOptions));
        var results = await Task.WhenAll(tasks);
        return results;
    }
}
