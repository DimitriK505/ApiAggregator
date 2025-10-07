namespace ApiAggregator.Domain.Models;

public class EndpointStats
{
    public int CallCount;
    public long TotalElapsedTimeMs;
    public double AverageTimeMs => CallCount == 0 ? 0 : (double)TotalElapsedTimeMs / CallCount;
    public string PerformanceBracket
    {
        get
        {
            if (AverageTimeMs < 100) return "fast";
            if (AverageTimeMs < 3500) return "average";
            return "slow";
        }
    }
}
