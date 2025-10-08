using ApiAggregator.Contracts;

namespace ApiAggregator.Domain;

public interface IApiCall
{
    string EndpointName { get; }
    Task<EndpointResult> CallEndpointAsync(HttpClient httpClient, FilterOptions filterOptions, SortingOptions sortingOptions);
}

