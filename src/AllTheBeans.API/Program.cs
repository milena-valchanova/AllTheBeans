using AllTheBeans.API.Mappers;
using AllTheBeans.API.Middleware;
using AllTheBeans.Domain;
using AllTheBeans.Infrastructure;

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

var app = builder.Build();

app.UseMiddleware<CustomExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

public partial class Program
{
    protected Program()
    {
        // Exposed to enable running tests using WebApplicationFactory
    }
}