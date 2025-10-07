using ApiAggregator.Domain;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net;

namespace ApiAggregator.UnitTests
{
    public class NewsApiCallTests
    {
        [Fact]
        public async Task CallEndpointAsync_ReturnsNoArticlesMessage_WhenNoArticles()
        {
            // Arrange
            var json = "{\"articles\":null}";
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var httpClient = HttpClientMockHelpers.CreateFakeHttpClient(fakeResponse);
            var cache = A.Fake<IMemoryCache>();
            object? dummy;
            A.CallTo(() => cache.TryGetValue(A<object>._, out dummy)).Returns(false);
            var stats = A.Fake<IAggregatorStatisticsService>();
            var options = Options.Create(new EndpointSettings { NewsApiKey = "fake-key" });
            var newsApiCall = new NewsApiCall(options, cache, stats);
            var filter = new FilterOptions { NewsKeyword = "" };
            var sorting = new SortingOptions { SortBy = SortBy.Date, SortOrder = SortOrder.Asc };

            // Act
            var result = await newsApiCall.CallEndpointAsync(httpClient, filter, sorting);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.ResponseBody.Should().Contain("News articles not found!");
        }

        [Fact]
        public async Task CallEndpointAsync_ReturnsErrorResult_OnException()
        {
            // Arrange
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var httpClient = HttpClientMockHelpers.CreateFakeHttpClient(fakeResponse);
            var cache = A.Fake<IMemoryCache>();
            object? dummy;
            A.CallTo(() => cache.TryGetValue(A<object>._, out dummy)).Returns(false);
            var stats = A.Fake<IAggregatorStatisticsService>();
            var options = Options.Create(new EndpointSettings { NewsApiKey = "fake-key" });
            var newsApiCall = new NewsApiCall(options, cache, stats);
            var filter = new FilterOptions { NewsKeyword = "" };
            var sorting = new SortingOptions { SortBy = SortBy.Date, SortOrder = SortOrder.Asc };

            // Act
            var result = await newsApiCall.CallEndpointAsync(httpClient, filter, sorting);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CallEndpointAsync_ReturnsPollyFallbackMessage_WhenPollyFallbackSource()
        {
            // Arrange
            var json = "{\"source\":\"PollyFallback\",\"message\":\"Fallback triggered\"}";
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var httpClient = HttpClientMockHelpers.CreateFakeHttpClient(fakeResponse);
            var cache = A.Fake<IMemoryCache>();
            object? dummy;
            A.CallTo(() => cache.TryGetValue(A<object>._, out dummy)).Returns(false);
            var stats = A.Fake<IAggregatorStatisticsService>();
            var options = Options.Create(new EndpointSettings { NewsApiKey = "fake-key" });
            var newsApiCall = new NewsApiCall(options, cache, stats);
            var filter = new FilterOptions { NewsKeyword = "" };
            var sorting = new SortingOptions { SortBy = SortBy.Date, SortOrder = SortOrder.Asc };

            // Act
            var result = await newsApiCall.CallEndpointAsync(httpClient, filter, sorting);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.ResponseBody.Should().Be("Fallback triggered");
        }
    }
}
