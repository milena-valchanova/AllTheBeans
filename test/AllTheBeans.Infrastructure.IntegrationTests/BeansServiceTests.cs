using AllTheBeans.Domain;
using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Entities;
using AllTheBeans.Domain.Services;
using AllTheBeans.Domain.Services.Implementation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AllTheBeans.Infrastructure.IntegrationTests;

[TestFixture(TestOf = typeof(BeansService))]
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
        var beanDto = new CreateOrUpdateBeanDTO()
        {
            CountryName = "Peru"
        };

        var _ = await _service.CreateAsync(beanDto);

        var countriesInDb = await _context.Countries.ToListAsync();
        Assert.That(countriesInDb, Has.Count.EqualTo(1));
        Assert.That(countriesInDb[0].Name, Is.EqualTo(beanDto.CountryName));
    }

    [Test]
    [Description("Bean should be created when it does not exist")]
    public async Task Bean_ShouldBe_Created_When_ItDoesNotExist()
    {
        var beanId = Guid.NewGuid();
        var beanDto = new CreateOrUpdateBeanDTO()
        {
            Name = "Bean 1",
            CountryName = "Peru"
        };

        var _ = await _service.CreateOrUpdateAsync(beanId, beanDto);

        var beansInDb = await _context.Beans.ToListAsync();
        Assert.That(beansInDb, Has.Count.EqualTo(1));
        Assert.That(beansInDb[0].Name, Is.EqualTo(beanDto.Name));
        Assert.That(beansInDb[0].Id, Is.Not.EqualTo(beanId));

        var countriesInDb = await _context.Countries.ToListAsync();
        Assert.That(countriesInDb, Has.Count.EqualTo(1));
        Assert.That(countriesInDb[0].Name, Is.EqualTo(beanDto.CountryName));
    }

    [Test]
    [Description("Bean should be updated when it exists")]
    public async Task Bean_ShouldBe_Updated_When_ItExists()
    {
        var seededBean = new Bean()
        {
            Name = "Old Name",
            Country = new Country()
            {
                Name = "Old Country"
            }
        };
        await _context.AddAsync(seededBean);
        await _context.SaveChangesAsync();

        var beanDto = new CreateOrUpdateBeanDTO()
        {
            Name = "New Name",
            CountryName = "New Country"
        };

        var result = await _service.CreateOrUpdateAsync(seededBean.Id, beanDto);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Name, Is.EqualTo(beanDto.Name));
            Assert.That(result.CountryName, Is.EqualTo(beanDto.CountryName));
        }

        var beansInDb = await _context.Beans.ToListAsync();
        Assert.That(beansInDb, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(beansInDb[0].Id, Is.EqualTo(seededBean.Id));
            Assert.That(beansInDb[0].Name, Is.EqualTo(beanDto.Name));
        }

        var countriesInDb = await _context.Countries.ToListAsync();
        Assert.That(countriesInDb, Has.Count.EqualTo(2));
    }

    [Test]
    [Description("GetByIdAsync should return correct entity")]
    public async Task GetByIdAsync_Should_ReturnCorrectEntity()
    {
        var seededBean = new Bean()
        {
            Country = new Country()
            {
                Name = "Peru"
            }
        };
        await _context.AddAsync(seededBean);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(seededBean.Id);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(seededBean.Id));
            Assert.That(result.Name, Is.EqualTo(seededBean.Name));
            Assert.That(result.Description, Is.EqualTo(seededBean.Description));
            Assert.That(result.CountryName, Is.EqualTo(seededBean.Country.Name));
            Assert.That(result.Index, Is.EqualTo(seededBean.Index));
            Assert.That(result.ImageName, Is.EqualTo(seededBean.ImageName));
            Assert.That(result.Colour, Is.EqualTo(seededBean.Colour));
            Assert.That(result.Cost, Is.EqualTo(seededBean.Cost));
        }
    }

    [Test]
    [Description("GetByIdAsync should throw KeyNotFoundException when bean is not found")]
    public void GetByIdAsync_Should_ThrowKeyNotFoundException_When_BeanIsNotFound()
    {
        var notExistingId = Guid.NewGuid();

        var exception = Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetByIdAsync(notExistingId));

        var expectedError = $"Bean with id {notExistingId} was not found";
        Assert.That(exception.Message, Is.EqualTo(expectedError));
    }

    [Test]
    [Description("Bean of the day should throw KeyNotFoundException when there are no beans in the database")]
    public void KeyNotFoundException_ShouldBe_Thrown_WhenThereAreNoBeans()
    {
        var exception = Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetOrCreateBeanOfTheDayAsync(DateOnly.MaxValue));

        var expectedError = "There are not enough beans to select Bean Of The Day";
        Assert.That(exception.Message, Is.EqualTo(expectedError));
    }

    [Test]
    [Description("Bean of the day should throw KeyNotFoundException when the only bean in the database was previously selected")]
    public async Task KeyNotFoundException_ShouldBe_Thrown_When_TheOnlyBeanInTheDatabaseWasPreviouslySelected()
    {
        var seededBean = new Bean()
        {
            Country = new Country()
            {
                Name = "Peru"
            }
        };
        await _context.AddAsync(seededBean);
        await _context.SaveChangesAsync();

        await _context.BeansOfTheDay.AddAsync(new BeanOfTheDay
        {
            BeanId = seededBean.Id,
            Date = DateOnly.MaxValue.AddDays(-1)
        });
        await _context.SaveChangesAsync();

        var exception = Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetOrCreateBeanOfTheDayAsync(DateOnly.MaxValue));

        var expectedError = "There are not enough beans to select Bean Of The Day";
        Assert.That(exception.Message, Is.EqualTo(expectedError));
    }

    [Test]
    [Description("The bean of the day should be the only bean in the database when there is no previously selected ones")]
    public async Task TheBeanOfTheDay_ShouldBe_TheOnlyBean_When_TheIsNoPreviouslySelectedOnes()
    {
        var seededBean = new Bean()
        {
            Country = new Country()
            {
                Name = "Peru"
            }
        };
        await _context.AddAsync(seededBean);
        await _context.SaveChangesAsync();

        var bean = await _service.GetOrCreateBeanOfTheDayAsync(DateOnly.MaxValue);

        Assert.That(bean.Id, Is.EqualTo(seededBean.Id));
    }

    [Test]
    [Description("The bean of the day should not be previously selected one")]
    public async Task TheBeanOfTheDay_ShouldNotBe_PreviouslySelectedOne()
    {
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
        await _context.AddRangeAsync(previouslySelectedBean, anotherBean);
        await _context.SaveChangesAsync();
        await _context.BeansOfTheDay.AddAsync(new BeanOfTheDay
        {
            BeanId = previouslySelectedBean.Id,
            Date = DateOnly.MaxValue.AddDays(-1)
        });
        await _context.SaveChangesAsync();

        var bean = await _service.GetOrCreateBeanOfTheDayAsync(DateOnly.MaxValue);

        Assert.That(bean.Id, Is.EqualTo(anotherBean.Id));
    }

    [Test]
    [Description("The bean of the day should be the same for the same date")]
    public async Task TheBeanOfTheDay_ShouldBe_TheSameForTheSameDate()
    {
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
        await _context.AddRangeAsync(previouslySelectedBean, anotherBean);
        await _context.SaveChangesAsync();
        await _context.BeansOfTheDay.AddAsync(new BeanOfTheDay
        {
            BeanId = previouslySelectedBean.Id,
            Date = DateOnly.MaxValue
        });
        await _context.SaveChangesAsync();

        var bean = await _service.GetOrCreateBeanOfTheDayAsync(DateOnly.MaxValue);

        Assert.That(bean.Id, Is.EqualTo(previouslySelectedBean.Id));
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
