using ApiAggregator.Contracts;
using ApiAggregator.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ApiAggregator.Controllers;

/// <summary>
/// Provides endpoints for retrieving aggregated data and statistics about API usage.
/// </summary>
/// <remarks>This controller serves as the entry point for interacting with the API aggregator service. It
/// includes operations for retrieving aggregated data based on filtering and sorting criteria, as well as statistics
/// about endpoint usage. Use the provided endpoints to query data or monitor API performance.</remarks>
[ApiController]
[Route("[controller]")]
public class ApiAggregatorController : ControllerBase
{
    private readonly ApiAggregatorService _aggregatorService;
    private readonly IAggregatorStatisticsService _aggregatorStatisticsService;
    public ApiAggregatorController(ApiAggregatorService aggregatorService, IAggregatorStatisticsService aggregatorStatisticsService)
    {
        _aggregatorService = aggregatorService;
        _aggregatorStatisticsService = aggregatorStatisticsService;
    }

    /// <summary>
    /// Retrieves aggregated data based on the specified filter and sorting options.
    /// </summary>
    /// <remarks>The method processes the provided filter and sorting options to retrieve and return the
    /// aggregated data. Ensure that both <paramref name="filterOptions"/> and <paramref name="sortingOptions"/> are
    /// properly populated to avoid unexpected results.</remarks>
    /// <param name="filterOptions">The filtering criteria to apply to the data. This parameter cannot be null.</param>
    /// <param name="sortingOptions">The sorting criteria to apply to the data. This parameter cannot be null.</param>
    /// <returns>An <see cref="IActionResult"/> containing the aggregated data.</returns>
    [HttpGet(Name = "GetAggregatorData")]
    public async Task<IActionResult> Get([FromQuery] FilterOptions filterOptions, [FromQuery] SortingOptions sortingOptions)
    {
        var result = await _aggregatorService.AggregateAsync(filterOptions, sortingOptions);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves statistics about the API aggregator's endpoint usage.     
    /// </summary>
    /// <returns></returns>
    [HttpGet("/Statistics", Name = "GetAggregatorStatisticsData")]
    public IActionResult GetStatistics()
    {
        var stats = _aggregatorStatisticsService.GetEndpointStatistics();

        var result = stats.ToDictionary(
            kvp => kvp.Key,
            kvp => new
            {
                CallCount = kvp.Value.CallCount,
                Performance = kvp.Value.PerformanceBracket
            });

        return Ok(result);
    }
}
