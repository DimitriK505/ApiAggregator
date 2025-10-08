using ApiAggregator.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ApiAggregator.Domain
{
    /// <summary>
    /// Represents an API call to retrieve and process top headlines from a news API endpoint.
    /// </summary>
    /// <remarks>This class provides functionality to fetch news articles based on specified filtering and
    /// sorting options. Results are cached for 15 minutes to improve performance and reduce redundant API calls. The
    /// class is designed to be used with dependency injection and requires configuration settings, a memory cache, and
    /// an aggregator statistics service.</remarks>
    public class NewsApiCall : IApiCall
    {
        public string EndpointName => "NewsEndpoint";
        private readonly EndpointSettings _settings;
        private readonly IMemoryCache _cache;
        private readonly IAggregatorStatisticsService _aggregatorStatisticsService;

        public NewsApiCall(IOptions<EndpointSettings> settings, IMemoryCache cache, IAggregatorStatisticsService aggregatorStatisticsService)
        {
            _settings = settings.Value;
            _cache = cache;
            _aggregatorStatisticsService = aggregatorStatisticsService;
        }

        /// <summary>
        /// Calls the specified news API endpoint to retrieve and process top headlines based on the provided filter and
        /// sorting options.
        /// </summary>
        /// <remarks>This method caches the results for a duration of 15 minutes to improve performance
        /// and reduce redundant API calls. If a cached result exists for the given filter and sorting options, it is
        /// returned immediately without making an API call.</remarks>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance used to make the HTTP request. Must not be null.</param>
        /// <param name="filterOptions">The filtering options to apply to the news articles, such as keywords to search for.</param>
        /// <param name="sortingOptions">The sorting options to apply to the news articles, such as sorting by date or title in ascending or
        /// descending order.</param>
        /// <returns>An <see cref="EndpointResult"/> containing the processed response from the API, including the filtered and
        /// sorted news articles. If the API call fails, the result will indicate the failure with an appropriate error
        /// message.</returns>
        public async Task<EndpointResult> CallEndpointAsync(HttpClient httpClient, FilterOptions filterOptions, SortingOptions sortingOptions)
        {
            if (_cache.TryGetValue(GenerateCacheKey(filterOptions, sortingOptions), out EndpointResult? cachedResult) && cachedResult != null)
            {
                return cachedResult;
            }

            var url = $"https://newsapi.org/v2/top-headlines?country=us&category=business&apiKey={_settings.NewsApiKey}";
            try
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "C# App");
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

                var newsResponse = JsonSerializer.Deserialize<NewsApiResponse>(json, options);
                StringBuilder allArticlesText = new StringBuilder();
                if (newsResponse?.Articles != null)
                {
                    switch (sortingOptions.SortBy)
                    {
                        case SortBy.Date:
                            newsResponse.Articles = sortingOptions.SortOrder == SortOrder.Asc
                                ? newsResponse.Articles.OrderBy(a => a.PublishedAt).ToList()
                                : newsResponse.Articles.OrderByDescending(a => a.PublishedAt).ToList();
                            break;
                        case SortBy.Name:
                            newsResponse.Articles = sortingOptions.SortOrder == SortOrder.Asc
                                ? newsResponse.Articles.OrderBy(a => a.Title).ToList()
                                : newsResponse.Articles.OrderByDescending(a => a.Title).ToList();
                            break;
                    }

                    foreach (var article in newsResponse.Articles)
                    {
                        if (string.IsNullOrEmpty(filterOptions.NewsKeyword) || article.Title.Contains(filterOptions.NewsKeyword, StringComparison.CurrentCultureIgnoreCase))
                        {
                            allArticlesText.AppendLine($"Published At: {article.PublishedAt}");
                            allArticlesText.AppendLine($"Title: {article.Title}");
                            allArticlesText.AppendLine($"Description: {article.Description}");
                        }
                    }
                }
                else
                {
                    allArticlesText.AppendLine("News articles not found!");
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
            return $"NewsCacheKey_{filterOptions.NewsKeyword}_{sortingOptions.SortBy}_{sortingOptions.SortOrder}";
        }
    }

    public class NewsApiResponse
    {
        public string Status { get; set; }
        public int TotalResults { get; set; }
        public List<Article> Articles { get; set; }
    }

    public class Article
    {
        public Source Source { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Content { get; set; }
        public DateTime PublishedAt { get; set; }
    }

    public class Source
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
