using AllTheBeans.Domain.Entities;
using AllTheBeans.Domain.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace AllTheBeans.Domain.UnitTests;

[TestFixture(TestOf = typeof(CountriesRepository))]
internal class CountriesRepositoryTests
{
    private BeansContext _context;
    private CountriesRepository CountriesRepository => new (_context);

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BeansContext>();
        options.UseInMemoryDatabase(Guid.NewGuid().ToString());
        _context = new BeansContext(options.Options);
    }

    [TearDown]
    public void TearDown() 
    { 
        _context?.Dispose(); 
    }

    [Test]
    [Description("Country should be created successfully when it does not exist")]
    public async Task Country_ShouldBe_CreatedSuccessfully_When_DoesNotExist()
    {
        var countryName = "Peru";
        var country = await CountriesRepository.GetOrCreateAsync(countryName);

        Assert.That(country, Is.Not.Null);
        Assert.That(country.Id, Is.EqualTo(1));
        Assert.That(country.Name, Is.EqualTo(countryName));

        Assert.That(_context.Countries.Count(), Is.EqualTo(1));

        var dbEntity = await _context.Countries.SingleAsync();
        Assert.That(dbEntity.Id, Is.EqualTo(1));
        Assert.That(dbEntity.Name, Is.EqualTo(countryName));
    }

    [Test]
    [Description("Country should be returned successfully without being recreated when it exists")]
    public async Task Country_ShouldBe_ReturnedSuccessfullyWithoutBeingRecreated_When_Exists()
    {
        var seededCountry = new Country
        {
            Name = "Peru"
        };
        await _context.Countries.AddAsync(seededCountry);
        await _context.SaveChangesAsync();

        var country = await CountriesRepository.GetOrCreateAsync(seededCountry.Name);

        Assert.That(country, Is.Not.Null);
        Assert.That(country.Id, Is.EqualTo(seededCountry.Id));
        Assert.That(country.Name, Is.EqualTo(seededCountry.Name));

        Assert.That(_context.Countries.Count(), Is.EqualTo(1));
    }
}
