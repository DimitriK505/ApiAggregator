namespace ApiAggregator.Domain;

public class EndpointResult
{
    public string Name { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string ResponseBody { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
