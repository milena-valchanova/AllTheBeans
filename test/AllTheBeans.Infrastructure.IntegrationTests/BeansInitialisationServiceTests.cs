using AllTheBeans.Domain;
using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Repositories;
using AllTheBeans.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Testcontainers.PostgreSql;

namespace AllTheBeans.Infrastructure.IntegrationTests;
internal class BeansInitialisationServiceTests
{
    private PostgreSqlContainer _postgresCotainer;
    private BeansContext _context;

    private readonly IBeansRepository _beansRepository = Substitute.For<IBeansRepository>();
    private IBeansInitialisationService _service;

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

        services.Replace(ServiceDescriptor.Scoped(_ => _beansRepository));
        var serviceProvider = services.BuildServiceProvider();

        _context = serviceProvider.GetRequiredService<BeansContext>();
        await _context.Database.EnsureCreatedAsync();

        _service = serviceProvider.GetRequiredService<IBeansInitialisationService>();
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
    [Description("Country should not be stored when storing bean fails")]
    public async Task Country_ShouldNotBe_Created_When_StoringBeanFails()
    {
        var thrownException = new Exception("Something went wrong");
        _beansRepository
            .CreateAsync(Arg.Any<BeanDTO>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
            .ThrowsAsyncForAnyArgs(thrownException);

        var beanDto = new BeanDTO()
        {
            CountryName = "Peru"
        };

        var exception = Assert.ThrowsAsync<Exception>(() => _service.InitiliseAsync(beanDto));

        Assert.That(exception, Is.EqualTo(thrownException));
        var numberOfCountriesInDb = await _context.Countries.CountAsync();
        Assert.That(numberOfCountriesInDb, Is.Zero);
    }

    [Test]
    [Description("Country should be stored when storing bean succeeds")]
    public async Task Country_ShouldBe_Created_When_StoringBeanSucceeds()
    {
        var beanId = Guid.NewGuid();
        _beansRepository
            .CreateAsync(Arg.Any<BeanDTO>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Task.FromResult(beanId));

        var beanDto = new BeanDTO()
        {
            CountryName = "Peru"
        };

        var result = await _service.InitiliseAsync(beanDto);

        Assert.That(result, Is.EqualTo(beanId));
        var countriesInDb = await _context.Countries.ToListAsync();
        Assert.That(countriesInDb, Has.Count.EqualTo(1));
        Assert.That(countriesInDb[0].Name, Is.EqualTo(beanDto.CountryName));
    }
}
