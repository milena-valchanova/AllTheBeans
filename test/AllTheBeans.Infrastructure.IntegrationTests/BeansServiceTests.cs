using AllTheBeans.Domain;
using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Entities;
using AllTheBeans.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AllTheBeans.Infrastructure.IntegrationTests;
internal class BeansServiceTests
{
    private PostgreSqlContainer _postgresCotainer;
    private BeansContext _context;

    private IBeansService _service;

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
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                ["ConnectionStrings:BeansDbConnectionString"] = _postgresCotainer.GetConnectionString()
            })
            .Build();
        services.AddDomainServices();
        services.AddInfrastructure(configuration);

        var serviceProvider = services.BuildServiceProvider();

        _context = serviceProvider.GetRequiredService<BeansContext>();
        await _context.Database.EnsureCreatedAsync();

        _service = serviceProvider.GetRequiredService<IBeansService>();
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_context is not null)
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }
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
    [Description("Country should be stored when storing bean succeeds")]
    public async Task Country_ShouldBe_Created_When_StoringBeanSucceeds()
    {
        var beanDto = new CreateBeanDTO()
        {
            CountryName = "Peru"
        };

        var _ = await _service.InitiliseAsync(beanDto);

        var countriesInDb = await _context.Countries.ToListAsync();
        Assert.That(countriesInDb, Has.Count.EqualTo(1));
        Assert.That(countriesInDb[0].Name, Is.EqualTo(beanDto.CountryName));
    }

    [Test]
    [Description("DeleteBeanAsync should remove a bean and related country if no other beans are related to it")]
    public async Task DeleteBeanAsync_Should_RemoveBeanAndCountry_When_NoOtherBeansAreRelatedtoTheCountry()
    {
        var bean = new Bean()
        {
            Country = new Country()
            {
                Name = "Peru"
            }
        };
        _context.Beans.Add(bean);
        await _context.SaveChangesAsync();

        await _service.DeleteBeanAsync(bean.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_context.Beans.Count(), Is.EqualTo(0));
            Assert.That(_context.Countries.Count(), Is.EqualTo(0));
        }
    }

    [Test]
    [Description("DeleteBeanAsync should remove a bean and keep country if other beans are related to it")]
    public async Task DeleteBeanAsync_Should_RemoveBeanAndKeepCountry_When_OtherBeansAreRelatedtoTheCountry()
    {
        var country = new Country()
        {
            Name = "Peru"
        };
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        var bean1 = new Bean()
        {
            Name = "Bean1",
            CountryId = country.Id
        };
        _context.Beans.Add(bean1);
        var bean2 = new Bean()
        {
            Name = "Bean2",
            CountryId = country.Id
        };
        _context.Beans.Add(bean2);
        await _context.SaveChangesAsync();

        await _service.DeleteBeanAsync(bean1.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_context.Beans.Count(), Is.EqualTo(1));
            var beanInDb = await _context.Beans.SingleAsync();
            Assert.That(beanInDb, Is.EqualTo(bean2));

            Assert.That(_context.Countries.Count(), Is.EqualTo(1));
        }
    }
}
