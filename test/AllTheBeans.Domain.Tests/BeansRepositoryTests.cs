using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Entities;
using AllTheBeans.Domain.Enums;
using AllTheBeans.Domain.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace AllTheBeans.Domain.UnitTests;

[TestFixture(TestOf = typeof(BeansRepository))]
internal class BeansRepositoryTests
{
    private const int defaultPageNumber = 1;
    private const int defaultPageSize = 5;

    private BeansContext _context;
    private BeansRepository BeansRepository => new(_context);

    private readonly Bean dummyBean = new Bean()
    {
        Country = new Country()
    };

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

    [TestCase(0)]
    [TestCase(-1)]
    [Description("GetAllAsync should throw ArgumentException when page number is negative")]
    public void GetAllAsync_Should_ThrowArgumentException_WhenPageNumberIsNegative(int invalidPageNumber)
    {
        var ex = Assert.ThrowsAsync<ArgumentException>(
            () => BeansRepository.GetAllAsync(invalidPageNumber, defaultPageSize));

        var expectedErrorMessage = "pageNumber must have positive value";
        Assert.That(ex.Message, Is.EqualTo(expectedErrorMessage));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [Description("GetAllAsync should throw ArgumentException when page size is not positive")]
    public void GetAllAsync_Should_ThrowArgumentException_WhenPageSizeIsNotPositive(int invalidPageSize)
    {
        var ex = Assert.ThrowsAsync<ArgumentException>(
            () => BeansRepository.GetAllAsync(defaultPageNumber, invalidPageSize));

        var expectedErrorMessage = "pageSize must have positive value";
        Assert.That(ex.Message, Is.EqualTo(expectedErrorMessage));
    }

    [Test]
    [Description("GetAllAsync should return an empty collection when there is no records in the database")]
    public async Task GetAllAsync_Should_ReturnEmptyCollection_When_ThereAreNoEntitiesInTheDatatbase()
    {
        var result = await BeansRepository.GetAllAsync(defaultPageNumber, defaultPageSize);

        Assert.That(result, Is.Empty);
    }

    [Test]
    [Description("GetAllAsync should return all entities when more than exisitng number of entities is requested")]
    public async Task GetAllAsync_Should_ReturnAllEntitites_When_MoreThanExistingNumberOfEntitiesIsRequested()
    {
        _context.Beans.Add(dummyBean);
        await _context.SaveChangesAsync();

        var result = await BeansRepository.GetAllAsync(defaultPageNumber, defaultPageSize);

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [TestCase(1, 2, 2)]
    [TestCase(3, 2, 1)]
    [TestCase(4, 2, 0)]
    [Description("GetAllAsync should paginate entities correctly")]
    public async Task GetAllAsync_Should_PaginateEntitiesCorrectly(int pageNumber, int pageSize, int expectedNumberOfBeans)
    {
        var totalNumberOfBeans = 5;
        var seededBeans = new List<Bean>();
        for (int i = 0; i < totalNumberOfBeans; i++)
        {
            seededBeans.Add(
                new Bean() 
                { 
                    Country = new Country()
                });
        }
        await _context.AddRangeAsync(seededBeans);
        await _context.SaveChangesAsync();

        var result = await BeansRepository.GetAllAsync(pageNumber, pageSize);
        Assert.That(result, Has.Count.EqualTo(expectedNumberOfBeans));
    }

    [TestCase(0)]
    [TestCase(2)]
    [TestCase(5)]
    [Description("Count all should return the number of all beans in the database")]
    public async Task CountAllAsync_ShouldReturn_TheTotalNumberOfBeans(int numberOfSeededEntities)
    {
        for (int i = 0; i < numberOfSeededEntities; i++)
        {
            _context.Beans.Add(new Bean());
        }
        if (numberOfSeededEntities > 0)
        {
            await _context.SaveChangesAsync();
        }

        var result = await BeansRepository.CountAllAsync();

        Assert.That(result, Is.EqualTo(numberOfSeededEntities));
    }

    [Test]
    [Description("Bean entity should be successfully populated with provided properties")]
    public async Task BeanEntity_ShouldBe_SuccessfullyPopulatedWithProvidedProperties()
    {
        var beanDto = new CreateBeanDTO()
        {
            Name = "TURNABOUT",
            Description = "Ipsum cupidatat nisi do elit veniam Lorem magna. Ullamco qui exercitation fugiat pariatur sunt dolore Lorem magna magna pariatur minim. Officia amet incididunt ad proident. Dolore est irure ex fugiat. Voluptate sunt qui ut irure commodo excepteur enim labore incididunt quis duis. Velit anim amet tempor ut labore sint deserunt.",
            IsBOTD = true,
            Cost = 39.26M,
            ImageName = "photo-1672306319681-7b6d7ef349cf",
            Colour = BeanColour.DarkRoast,
            Index = 1
        };
        var countryId = 1;

        await BeansRepository.CreateAsync(beanDto, countryId);

        var storedBean = await _context.Beans.SingleOrDefaultAsync();

        Assert.That(storedBean, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(storedBean.Name, Is.EqualTo(beanDto.Name));
            Assert.That(storedBean.Description, Is.EqualTo(beanDto.Description));
            Assert.That(storedBean.IsBOTD, Is.EqualTo(beanDto.IsBOTD));
            Assert.That(storedBean.Cost, Is.EqualTo(beanDto.Cost));
            Assert.That(storedBean.ImageName, Is.EqualTo(beanDto.ImageName));
            Assert.That(storedBean.Colour, Is.EqualTo(beanDto.Colour));
            Assert.That(storedBean.Index, Is.EqualTo(beanDto.Index));
            Assert.That(storedBean.CountryId, Is.EqualTo(countryId));
        }
    }
}
