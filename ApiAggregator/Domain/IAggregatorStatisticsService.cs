using ApiAggregator.Domain.Models;

namespace ApiAggregator.Domain;

public interface IAggregatorStatisticsService
{
    void RecordApiCall(string endpoint, long elapsedMs);
    IReadOnlyDictionary<string, EndpointStats> GetEndpointStatistics();
}
