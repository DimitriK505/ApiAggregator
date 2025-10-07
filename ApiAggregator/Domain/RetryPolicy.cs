using Polly;
using Polly.Extensions.Http;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ApiAggregator.Domain;

public static class RetryPolicy
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions.HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy() =>
        Policy.TimeoutAsync<HttpResponseMessage>(10);

    public static IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        var fallbackResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    message = "Service is currently unavailable. Please try again later.",
                    timestamp = DateTime.UtcNow,
                    source = "PollyFallback"
                }),
                Encoding.UTF8,
                "application/json"
            )
        };

        return Policy<HttpResponseMessage>
            .Handle<Exception>()
            .FallbackAsync(fallbackResponse, onFallbackAsync: async (exception, context) =>
            {
                Console.WriteLine($"Fallback executed due to: {exception.Exception.Message}"); 
            });
    }
}
