using AllTheBeans.API.Mappers;
using AllTheBeans.API.Middleware;
using AllTheBeans.API.Settings;
using AllTheBeans.Domain;
using AllTheBeans.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration
        .AddUserSecrets<Program>();
}

builder.Services.AddControllers();

builder.Services.AddSingleton<IBeansMapper, BeansMapper>();
builder.Services.AddDomainServices();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var rateLimitOptions = new RateLimitOptions();
builder
    .Configuration
    .GetSection(RateLimitOptions.SectionName)
    .Bind(rateLimitOptions);
var concurrencyPolicy = "concurrencyPolicy";
builder.Services.AddRateLimiter(rateLimiterOptions => 
    {
        rateLimiterOptions
            .AddConcurrencyLimiter(policyName: concurrencyPolicy, options =>
            {
                options.PermitLimit = rateLimitOptions.PermitLimit;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = rateLimitOptions.QueueLimit;
            });
        rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

var app = builder.Build();

app.UseRateLimiter();

app.UseMiddleware<CustomExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app
    .MapControllers()
    .RequireRateLimiting(concurrencyPolicy);

await app.RunAsync();

public partial class Program
{
    protected Program()
    {
        // Exposed to enable running tests using WebApplicationFactory
    }
}