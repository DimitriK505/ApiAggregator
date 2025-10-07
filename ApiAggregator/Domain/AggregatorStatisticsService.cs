using ApiAggregator.Domain.Models;
using System.Collections.Concurrent;

namespace ApiAggregator.Domain;

/// <summary>
/// service for recording and retrieving statistics about API endpoint usage.
/// </summary>
public class AggregatorStatisticsService : IAggregatorStatisticsService
{
    private readonly ConcurrentDictionary<string, EndpointStats> _stats = new();

    /// <summary>
    /// Records an API call to the specified endpoint, updating the call count and total elapsed time.
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="elapsedMilliseconds"></param>
    public void RecordApiCall(string endpoint, long elapsedMilliseconds)
    {
        var stats = _stats.GetOrAdd(endpoint, _ => new EndpointStats());
        Interlocked.Increment(ref stats.CallCount);
        stats.TotalElapsedTimeMs += elapsedMilliseconds;
    }

    /// <summary>
    /// Retrieves the statistics for all recorded endpoints.
    /// </summary>
    /// <returns>Dictionary of statistics</returns>
    public IReadOnlyDictionary<string, EndpointStats> GetEndpointStatistics()
    {
        return _stats;
    }
}
