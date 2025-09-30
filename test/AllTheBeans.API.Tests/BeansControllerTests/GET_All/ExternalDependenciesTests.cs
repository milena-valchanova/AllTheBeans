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

namespace AllTheBeans.API.IntegrationTests.BeansControllerTests.GET_All;

[TestFixture(TestOf = typeof(BeansController))]
internal class ExternalDependenciesTests
{
    private const string Endpoint = "/beans";

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
                        ["CurrencyCulture"] = "en-GB"
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
    [Description("GetAll should return correct response")]
    public async Task GetAll_Should_ReturnCorrectResponse()
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
            Id = Guid.NewGuid(),
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
        var result = await response.Content.ReadFromJsonAsync<BeansResponse>();
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Beans.Count(), Is.EqualTo(1));
        }
        var bean = result.Beans.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(bean.Id, Is.EqualTo(seededBean.Id.ToString().Replace("-", string.Empty)));
            Assert.That(bean.Index, Is.EqualTo(seededBean.Index));
            Assert.That(bean.Name, Is.EqualTo(seededBean.Name));
            Assert.That(bean.Description, Is.EqualTo(seededBean.Description));
            Assert.That(bean.IsBOTD, Is.EqualTo(seededBean.IsBOTD));
            Assert.That(bean.Country, Is.EqualTo(seededCountry.Name));
            Assert.That(bean.Colour, Is.EqualTo("dark roast"));
            Assert.That(bean.Image, Is.EqualTo("https://some.location.com/image-1"));
            Assert.That(bean.Cost, Is.EqualTo("£5.00"));
        }
    }
}
