using ApiAggregator.Domain;
using FakeItEasy;
using FluentAssertions;

namespace ApiAggregator.UnitTests
{
    public class ApiAggregatorServiceTests
    {
        [Fact]
        public async Task AggregateAsync_CallsAllApiCallsAndReturnsResults()
        {
            // Arrange
            var filter = new FilterOptions { NewsKeyword = "foo" };
            var sorting = new SortingOptions { SortBy = SortBy.Date, SortOrder = SortOrder.Asc };
            var httpClient = HttpClientMockHelpers.CreateFakeHttpClient(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK));

            var call1 = A.Fake<IApiCall>();
            var call2 = A.Fake<IApiCall>();
            var result1 = new EndpointResult { Name = "Call1", IsSuccess = true, ResponseBody = "Result1" };
            var result2 = new EndpointResult { Name = "Call2", IsSuccess = true, ResponseBody = "Result2" };
            A.CallTo(() => call1.CallEndpointAsync(httpClient, filter, sorting)).Returns(Task.FromResult(result1));
            A.CallTo(() => call2.CallEndpointAsync(httpClient, filter, sorting)).Returns(Task.FromResult(result2));

            var service = new ApiAggregatorService(new[] { call1, call2 }, httpClient);

            // Act
            var results = (await service.AggregateAsync(filter, sorting)).ToList();

            // Assert
            results.Should().HaveCount(2);
            results.Should().ContainEquivalentOf(result1);
            results.Should().ContainEquivalentOf(result2);
            A.CallTo(() => call1.CallEndpointAsync(httpClient, filter, sorting)).MustHaveHappenedOnceExactly();
            A.CallTo(() => call2.CallEndpointAsync(httpClient, filter, sorting)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task AggregateAsync_ReturnsEmpty_WhenNoApiCalls()
        {
            // Arrange
            var filter = new FilterOptions();
            var sorting = new SortingOptions();
            var httpClient = HttpClientMockHelpers.CreateFakeHttpClient(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK));
            var service = new ApiAggregatorService(Enumerable.Empty<IApiCall>(), httpClient);

            // Act
            var results = await service.AggregateAsync(filter, sorting);

            // Assert
            results.Should().BeEmpty();
        }
    }
}
