using AllTheBeans.API.Controllers;
using AllTheBeans.API.DataModels;
using AllTheBeans.Domain;
using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Entities;
using AllTheBeans.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;

namespace AllTheBeans.API.IntegrationTests.BeansControllerTests.BeanOfTheDay;

[TestFixture(TestOf = typeof(BeansController))]
internal class ExternalDependenciesTests
{
    private const string Endpoint = "/beans/of-the-day";

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
                        ["ConnectionStrings:BeansDbConnectionString"] = _postgresCotainer.GetConnectionString(),
                        ["ImagesLocation"] = "https://some.location.com/",
                        ["CurrencyCulture"] = "en-GB",
                        ["RateLimit:PermitLimit"] = "50",
                        ["RateLimit:QueueLimit"] = "100"
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
    [Description("Get should return correctly formatted response when a bean of the day is found")]
    public async Task Get_Should_ReturnCorrectResponse_When_ABeanOfTheDayIsFound()
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
            Description = "Some Description",
            Index = 1,
            IsBOTD = true,
            ImageName = "image-1",
            Colour = BeanColour.DarkRoast,
            Cost = 5,
            CountryId = seededCountry.Id
        };
        context.Beans.Add(seededBean);
        await context.SaveChangesAsync();

        using var httpClient = _factory.CreateClient();
        using var response = await httpClient.GetAsync(Endpoint);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await response.Content.ReadFromJsonAsync<BeanResponse>();
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(seededBean.Id.ToString().Replace("-", string.Empty)));
            Assert.That(result.Index, Is.EqualTo(seededBean.Index));
            Assert.That(result.Name, Is.EqualTo(seededBean.Name));
            Assert.That(result.Description, Is.EqualTo(seededBean.Description));
            Assert.That(result.IsBOTD, Is.EqualTo(seededBean.IsBOTD));
            Assert.That(result.Country, Is.EqualTo(seededCountry.Name));
            Assert.That(result.Colour, Is.EqualTo("dark roast"));
            Assert.That(result.Image, Is.EqualTo("https://some.location.com/image-1"));
            Assert.That(result.Cost, Is.EqualTo("£5.00"));
        }
    }

    [Test]
    [Description("The bean of the day should be the same for concurrent calls")]
    public async Task TheBeanOfTheDay_ShouldBe_TheSameForConcurrentCalss()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BeansContext>();
        var previouslySelectedBean = new Bean()
        {
            Name = "Bean 1",
            Country = new Country()
            {
                Name = "Peru"
            }
        };
        var anotherBean = new Bean()
        {
            Name = "Bean 2",
            Country = new Country()
            {
                Name = "Bolivia"
            }
        };
        await context.AddRangeAsync(previouslySelectedBean, anotherBean);
        await context.SaveChangesAsync();

        var concurrentCalls = 99;
        var tasks = new List<Task<HttpResponseMessage>>();
        using var httpClient = _factory.CreateClient();
        for (int i = 0; i < concurrentCalls; i++)
        {
            tasks.Add(httpClient.GetAsync(Endpoint));
        }
        var result = await Task.WhenAll(tasks);

        var uniqueIds = new HashSet<string>();

        using (Assert.EnterMultipleScope())
        {
            foreach (var res in result)
            {
                Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                var content = await res.Content.ReadFromJsonAsync<BeanResponse>();
                Assert.That(content, Is.Not.Null);
                uniqueIds.Add(content.Id);
            }
        }
        Assert.That(uniqueIds, Has.Count.EqualTo(1));
        Assert.That(context.BeansOfTheDay.Count, Is.EqualTo(1));
    }
}
