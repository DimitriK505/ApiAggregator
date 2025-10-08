using System.Text.Json.Serialization;

namespace ApiAggregator.Contracts;

public class SortingOptions
{
    public SortBy SortBy { get; set; } = SortBy.Date;
    public SortOrder SortOrder { get; set; } = SortOrder.Asc;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortBy
{
    Name,
    Date
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortOrder
{
    Asc,
    Desc
}

