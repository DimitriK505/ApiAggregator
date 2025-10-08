using System.Text.Json.Serialization;

namespace ApiAggregator.Domain.Models;

public class MatchResponse
{
    [JsonPropertyName("matches")]
    public List<Match> Matches { get; set; } = new List<Match>();
}

public class Match
{
    [JsonPropertyName("utcDate")]
    public DateTime UtcDate { get; set; }

    [JsonPropertyName("homeTeam")]
    public Team HomeTeam { get; set; } = new();

    [JsonPropertyName("awayTeam")]
    public Team AwayTeam { get; set; } = new();

    [JsonPropertyName("score")]
    public Score Score { get; set; } = new();
}

public class Team
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class Score
{
    [JsonPropertyName("fullTime")]
    public FullTime FullTime { get; set; } = new();
}

public class FullTime
{
    [JsonPropertyName("home")]
    public int? Home { get; set; }

    [JsonPropertyName("away")]
    public int? Away { get; set; }
}


