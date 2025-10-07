using ApiAggregator.Domain;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net;

namespace ApiAggregator.UnitTests
{
    public class WeatherApiCallTests
    {
        [Fact]
        public async Task CallEndpointAsync_ReturnsParsedWeatherResult_OnSuccess()
        {
            // Arrange
            var json = "{" +
                "\"weather\":[{\"description\":\"clear sky\"}]," +
                "\"main\":{\"temp\":23.45}" +
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
            var options = Options.Create(new EndpointSettings { WeatherApiKey = "fake-key" });
            var weatherApi = new WeatherApiCall(options, cache, stats);

            // Act
            var result = await weatherApi.CallEndpointAsync(httpClient, new FilterOptions(), new SortingOptions());

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Name.Should().Be("WeatherEndpoint");
            result.ResponseBody.Should().Contain("Weather in Athens: clear sky, Temperature: 23.45");
            A.CallTo(() => stats.RecordApiCall("WeatherEndpoint", A<long>._)).MustHaveHappened();
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
            var options = Options.Create(new EndpointSettings { WeatherApiKey = "fake-key" });
            var weatherApi = new WeatherApiCall(options, cache, stats);

            // Act
            var result = await weatherApi.CallEndpointAsync(httpClient, new FilterOptions(), new SortingOptions());

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        }
    }
}
