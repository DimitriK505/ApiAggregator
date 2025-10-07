## Overview

**API Aggregator** is an ASP.NET Core (.NET 8, C# 12) web API that aggregates data from multiple external sources (such as weather, news, and sports APIs) and provides unified endpoints for querying this data. It also tracks and exposes statistics about API usage and performance.

## Features

- **Aggregated Data Endpoints:** Retrieve combined data from multiple APIs with filtering and sorting options.
- **Statistics Endpoint:** Monitor API usage and performance metrics for each endpoint.
- **Caching:** Results are cached to improve performance and reduce external API calls.
- **Extensible:** Easily add new API sources by implementing the `IApiCall` interface.
- **Unit Tested:** Includes comprehensive unit tests using xUnit, FakeItEasy, and FluentAssertions.

## Project Structure

- `Controllers/ApiAggregatorController.cs` – Main API controller exposing aggregation and statistics endpoints.
- `Domain/` – Core logic, API call implementations, caching, and statistics services.
- `UnitTests/` – Unit tests for all major components.

## Endpoints

### `GET /ApiAggregator`
Retrieve aggregated data from all configured sources.

**Query Parameters:**
- `filterOptions` – Filtering criteria (e.g., keywords).
- `sortingOptions` – Sorting criteria (e.g., by date or name, ascending/descending).

### `GET /Statistics`
Retrieve usage and performance statistics for each aggregated endpoint.

## NewsApiCall

 NewsApiCall efficiently fetches news articles, applies filtering and sorting, caches the result, and handles errors and fallback scenarios, returning a standardized `EndpointResult` for use by the aggregator service.

## SportsNewsApiCall

 SportsNewsCall efficiently fetches sports match data, applies filtering and sorting, caches the result, and handles errors and fallback scenarios, returning a standardized `EndpointResult` for use by the aggregator service.

## WeatherApiCall

 WeatherApiCall fetches current weather data for Athens, formats it, caches the result, and handles errors, returning a standardized `EndpointResult` for use by the aggregator service.

