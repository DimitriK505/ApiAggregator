using FakeItEasy;

namespace ApiAggregator.UnitTests
{
    public static class HttpClientMockHelpers
    {
        public static HttpClient CreateFakeHttpClient(HttpResponseMessage response)
        {
            var handler = A.Fake<HttpMessageHandler>();
            A.CallTo(handler)
                .Where(call => call.Method.Name == "SendAsync")
                .WithReturnType<Task<HttpResponseMessage>>()
                .Returns(Task.FromResult(response));

            return new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };
        }
    }
}
