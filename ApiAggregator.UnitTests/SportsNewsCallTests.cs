using ApiAggregator.Domain;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net;

namespace ApiAggregator.UnitTests
{
    public class SportsNewsCallTests
    {
        [Fact]
        public async Task CallEndpointAsync_ReturnsParsedResults_OnSuccess()
        {
            // Arrange
            var json = "{" +
                "\"matches\":[{" +
                "\"utcDate\":\"2025-05-01T20:00:00Z\"," +
                "\"homeTeam\":{\"name\":\"TeamA\"}," +
                "\"awayTeam\":{\"name\":\"TeamB\"}," +
                "\"score\":{\"fullTime\":{\"home\":2,\"away\":1}}" +
                "}]" +
                "}";
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var httpClient = HttpClientMockHelpers.CreateFakeHttpClient(fakeResponse);
            var cache = A.Fake<IMemoryCache>();
            object? dummy;
            A.CallTo(() => cache.TryGetValue(A<object>._, out dummy)).Returns(false);
            var stats = A.Fake<IAggregatorStatisticsService>();
            var options = Options.Create(new EndpointSettings { SportsApiKey = "fake-key" });
            var sportsNewsCall = new SportsNewsCall(options, cache, stats);
            var filter = new FilterOptions { SportNewsKeyword = "" };
            var sorting = new SortingOptions { SortBy = SortBy.Date, SortOrder = SortOrder.Asc };

            // Act
            var result = await sportsNewsCall.CallEndpointAsync(httpClient, filter, sorting);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Name.Should().Be("SportsNewsEndpoint");
            result.ResponseBody.Should().Contain("Champions League Results");
            result.ResponseBody.Should().Contain("2025-05-01 - TeamA 2 : 1 TeamB");
            A.CallTo(() => stats.RecordApiCall("SportsNewsEndpoint", A<long>._)).MustHaveHappened();
        }

        [Fact]
        public async Task CallEndpointAsync_ReturnsNoMatchesMessage_WhenNoMatches()
        {
            // Arrange
            var json = "{\"matches\":[]}";
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var httpClient = HttpClientMockHelpers.CreateFakeHttpClient(fakeResponse);
            var cache = A.Fake<IMemoryCache>();
            object? dummy;
            A.CallTo(() => cache.TryGetValue(A<object>._, out dummy)).Returns(false);
            var stats = A.Fake<IAggregatorStatisticsService>();
            var options = Options.Create(new EndpointSettings { SportsApiKey = "fake-key" });
            var sportsNewsCall = new SportsNewsCall(options, cache, stats);
            var filter = new FilterOptions { SportNewsKeyword = "" };
            var sorting = new SortingOptions { SortBy = SortBy.Date, SortOrder = SortOrder.Asc };

            // Act
            var result = await sportsNewsCall.CallEndpointAsync(httpClient, filter, sorting);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.ResponseBody.Should().Contain("No Champions League matches in the last 7 days.");
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
            var options = Options.Create(new EndpointSettings { SportsApiKey = "fake-key" });
            var sportsNewsCall = new SportsNewsCall(options, cache, stats);
            var filter = new FilterOptions { SportNewsKeyword = "" };
            var sorting = new SortingOptions { SortBy = SortBy.Date, SortOrder = SortOrder.Asc };

            // Act
            var result = await sportsNewsCall.CallEndpointAsync(httpClient, filter, sorting);

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
            var options = Options.Create(new EndpointSettings { SportsApiKey = "fake-key" });
            var sportsNewsCall = new SportsNewsCall(options, cache, stats);
            var filter = new FilterOptions { SportNewsKeyword = "" };
            var sorting = new SortingOptions { SortBy = SortBy.Date, SortOrder = SortOrder.Asc };

            // Act
            var result = await sportsNewsCall.CallEndpointAsync(httpClient, filter, sorting);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.ResponseBody.Should().Be("Fallback triggered");
        }
    }
}
