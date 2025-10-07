using ApiAggregator.Domain;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<ApiAggregatorService>()
    .AddPolicyHandler(RetryPolicy.GetRetryPolicy())
    .AddPolicyHandler(RetryPolicy.GetTimeoutPolicy())
    .AddPolicyHandler(RetryPolicy.GetFallbackPolicy());

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IApiCall, WeatherApiCall>();
builder.Services.AddScoped<IApiCall, NewsApiCall>();
builder.Services.AddScoped<IApiCall, SportsNewsCall>();
builder.Services.AddSingleton<IAggregatorStatisticsService, AggregatorStatisticsService>();
builder.Services.Configure<EndpointSettings>(
    builder.Configuration.GetSection("EndpointSettings"));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


