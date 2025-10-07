using ApiAggregator.Domain;
using FluentAssertions;

namespace ApiAggregator.UnitTests
{
    public class AggregatorStatisticsServiceTests
    {
        [Fact]
        public void RecordApiCall_AddsAndUpdatesStatsCorrectly()
        {
            // Arrange
            var service = new AggregatorStatisticsService();

            // Act
            service.RecordApiCall("Weather", 100);
            service.RecordApiCall("Weather", 200);
            service.RecordApiCall("News", 50);

            var stats = service.GetEndpointStatistics();

            // Assert
            stats.Should().ContainKey("Weather");
            stats["Weather"].CallCount.Should().Be(2);
            stats["Weather"].TotalElapsedTimeMs.Should().Be(300);
            stats.Should().ContainKey("News");
            stats["News"].CallCount.Should().Be(1);
            stats["News"].TotalElapsedTimeMs.Should().Be(50);
        }
    }
}
