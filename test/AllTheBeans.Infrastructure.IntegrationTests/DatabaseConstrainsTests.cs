using AllTheBeans.Domain;
using AllTheBeans.Domain.Entities;
using AllTheBeans.Domain.Entities.Constants;
using AllTheBeans.Domain.Enums;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NUnit.Framework.Internal;
using Testcontainers.PostgreSql;

namespace AllTheBeans.Infrastructure.IntegrationTests;

[TestFixture(TestOf = typeof(BeansContext))]
internal class DatabaseConstrainsTests
{
    private PostgreSqlContainer _postgresCotainer;
    private BeansContext _context;

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
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        _context = serviceProvider.GetRequiredService<BeansContext>();
        await _context.Database.EnsureCreatedAsync();
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
    [Description("An exception should be thrown when bean image name is longer than specified")]
    public void AnException_ShouldBe_Thrown_When_BeanImageNameIsLongerThanSpecified()
    {
        var bean = new Bean()
        {
            ImageName = new string('a', PropertyLengths.ImageName + 1)
        };
        _context.Beans.Add(bean);
        VerifyStringLengthConstrainIsApplied(PropertyLengths.ImageName);
    }

    [Test]
    [Description("An exception should be thrown when bean name is longer than specified")]
    public void AnException_ShouldBe_Thrown_When_BeanNameIsLongerThanSpecified()
    {
        var bean = new Bean()
        {
            Name = new string('a', PropertyLengths.Name + 1)
        };
        _context.Beans.Add(bean);
        VerifyStringLengthConstrainIsApplied(PropertyLengths.Name);
    }

    [Test]
    [Description("An exception should be thrown when bean description is longer than specified")]
    public void AnException_ShouldBe_Thrown_When_BeanDescriptionIsLongerThanSpecified()
    {
        var bean = new Bean()
        {
            Description = new string('a', PropertyLengths.Description + 1)
        };
        _context.Beans.Add(bean);
        VerifyStringLengthConstrainIsApplied(PropertyLengths.Description);
    }

    [Test]
    [Description("An exception should be thrown when country name is longer than specified")]
    public void AnException_ShouldBe_Thrown_When_CountryNameIsLongerThanSpecified()
    {
        var country = new Country()
        {
            Name = new string('a', PropertyLengths.Name + 1)
        };
        _context.Countries.Add(country);
        VerifyStringLengthConstrainIsApplied(PropertyLengths.Name);
    }

    private void VerifyStringLengthConstrainIsApplied(long expectedLength)
    {
        var exception = Assert.ThrowsAsync<MaxLengthExceededException>(
                    () => _context.SaveChangesAsync());

        using var _ = Assert.EnterMultipleScope();
        Assert.That(exception.InnerException, Is.Not.Null);
        Assert.That(exception.InnerException, Is.InstanceOf<PostgresException>());
        var expectedErrorMessage = $"22001: value too long for type character varying({expectedLength})";
        Assert.That(exception.InnerException.Message, Does.Contain(expectedErrorMessage));
    }

    [Test]
    [Description("Entities should be created successfully when all properties are within their limits")]
    public void Entities_ShouldBe_CreatedSuccessfully_When_AllPropertiesAreWithinTheirLimits()
    {
        var bean = new Bean()
        {
            Index = 1,
            IsBOTD = true,
            Cost = 55.55M,
            ImageName = new string('a', PropertyLengths.ImageName),
            Colour = BeanColour.Green,
            Name = new string('a', PropertyLengths.Name),
            Description = new string('a', PropertyLengths.Description),
            Country = new Country()
            {
                Name = new string('a', PropertyLengths.Name)
            }
        };
        _context.Beans.Add(bean);
        Assert.DoesNotThrowAsync(
            () => _context.SaveChangesAsync());
    }
}
