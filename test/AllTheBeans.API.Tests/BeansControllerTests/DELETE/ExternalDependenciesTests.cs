using AllTheBeans.API.Controllers;
using AllTheBeans.API.DataModels;
using AllTheBeans.Domain;
using AllTheBeans.Domain.Entities;
using AllTheBeans.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;

namespace AllTheBeans.API.IntegrationTests.BeansControllerTests.DELETE;

[TestFixture(TestOf = typeof(BeansController))]
internal class ExternalDependenciesTests
{
    private PostgreSqlContainer _postgresCotainer;
    private WebApplicationFactory<Program> _factory;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _postgresCotainer = new PostgreSqlBuilder()
            .WithDatabase("test-beans-db")
            .WithCleanUp(true)
            .Build();
        await _postgresCotainer.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");

                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>()
                    {
                        ["ConnectionStrings:BeansDbConnectionString"] = _postgresCotainer.GetConnectionString()
                    })
                    .Build();
                builder.UseConfiguration(configuration);
            });
    }

    [SetUp]
    public async Task SetUp()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BeansContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BeansContext>();
        await dbContext.Database.EnsureDeletedAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_postgresCotainer is not null)
        {
            await _postgresCotainer.StopAsync();
            await _postgresCotainer.DisposeAsync();
        }
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }

    [Test]
    [Description("Delete should remove a bean")]
    public async Task GetAll_Should_RemoveABean()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BeansContext>();

        var seededCountry = new Country()
        {
            Name = "Peru"
        };
        context.Countries.Add(seededCountry);
        await context.SaveChangesAsync();

        var seededBean = new Bean()
        {
            Name = "Bean 1",
            CountryId = seededCountry.Id
        };
        context.Beans.Add(seededBean);
        await context.SaveChangesAsync();        

        using var httpClient = _factory.CreateClient();
        using var response = await httpClient.DeleteAsync($"/beans/{seededBean.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(context.Beans.Count, Is.Zero);
        Assert.That(context.Countries.Count, Is.Zero);
    }
}
