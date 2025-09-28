using AllTheBeans.Domain;
using AllTheBeans.Domain.Entities;
using AllTheBeans.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AllTheBeans.Infrastructure.IntegrationTests;

[TestFixture(TestOf = typeof(BeansContext))]
internal class NavigationPropertiesTests
{
    private PostgreSqlContainer _postgresCotainer;
    private IServiceCollection _serviceCollection;
    private Bean _seededBean;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _postgresCotainer = new PostgreSqlBuilder()
            .WithDatabase("test-beans-db")
            .WithCleanUp(true)
            .Build();
        await _postgresCotainer.StartAsync();

    }

    [SetUp]
    public async Task SetUp()
    {
        _serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                ["ConnectionStrings:BeansDbConnectionString"] = _postgresCotainer.GetConnectionString()
            })
            .Build();
        _serviceCollection.AddInfrastructure(configuration);
        using var serviceProvider = _serviceCollection.BuildServiceProvider();

        using var context = serviceProvider.GetRequiredService<BeansContext>();
        await context.Database.EnsureCreatedAsync();
        await InitDatabase(context);
    }

    private async Task InitDatabase(BeansContext context)
    {
        _seededBean = new Bean()
        {
            Id = Guid.NewGuid(),
            Country = new Country()
            {
                Name = "Peru"
            }
        };
        await context.Beans.AddAsync(_seededBean);
        await context.SaveChangesAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        using var serviceProvider = _serviceCollection.BuildServiceProvider();
        using var context = serviceProvider.GetRequiredService<BeansContext>();
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_postgresCotainer is not null)
        {
            await _postgresCotainer.StopAsync();
            await _postgresCotainer.DisposeAsync();
        }
    }

    [Test]
    [Description("PropertyNotInitialisedException should be thrown when accessing country without loading it")]
    public async Task PropertyNotInitialisedException_ShouldBe_Thrown_When_AccessingCountryWithoutLoadingIt()
    {
        using var serviceProvider = _serviceCollection.BuildServiceProvider();

        using var context = serviceProvider.GetRequiredService<BeansContext>();
        var bean = await context.Beans.FirstAsync();
        var exception = Assert.Throws<PropertyNotInitialisedException>(
            () => _ = bean.Country);

        Assert.That(exception.Message, Is.EqualTo("Country is not initialised"));
    }

    [Test]
    [Description("Country property should be available when it is inluded while loading a bean entity")]
    public async Task CountryProperty_ShouldBe_Available_When_ItIsIncludedWhileLoadingABeanEntity()
    {
        using var serviceProvider = _serviceCollection.BuildServiceProvider();

        using var context = serviceProvider.GetRequiredService<BeansContext>();
        var bean = await context.Beans
            .Include(p => p.Country)
            .FirstAsync();
        var country = bean.Country;

        Assert.That(country.Name, Is.EqualTo(_seededBean.Country.Name));
    }

    [Test]
    [Description("PropertyNotInitialisedException should be thrown when accessing beans without loading it")]
    public async Task PropertyNotInitialisedException_ShouldBe_Thrown_When_AccessingBeansWithoutLoadingIt()
    {
        using var serviceProvider = _serviceCollection.BuildServiceProvider();

        using var context = serviceProvider.GetRequiredService<BeansContext>();
        var country = await context.Countries.FirstAsync();
        var exception = Assert.Throws<PropertyNotInitialisedException>(
            () => _ = country.Beans);

        Assert.That(exception.Message, Is.EqualTo("Beans is not initialised"));
    }

    [Test]
    [Description("Beans property should be available when it is inluded while loading a country entity")]
    public async Task BeansProperty_ShouldBe_Available_When_ItIsIncludedWhileLoadingACountryEntity()
    {
        using var serviceProvider = _serviceCollection.BuildServiceProvider();

        using var context = serviceProvider.GetRequiredService<BeansContext>();
        var country = await context.Countries
            .Include(p => p.Beans)
            .FirstAsync();
        var bean = country.Beans.First();

        Assert.That(bean.Id, Is.EqualTo(_seededBean.Id));
    }
}
