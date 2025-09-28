using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Enums;
using AllTheBeans.Domain.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace AllTheBeans.Domain.Tests;

[TestFixture(TestOf = typeof(BeansRepository))]
internal class BeansRepositoryTests
{
    private BeansContext _context;
    private BeansRepository BeansRepository => new(_context);

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
    [Description("Bean entity should be successfully populated with provided properties")]
    public async Task BeanEntity_ShouldBe_SuccessfullyPopulatedWithProvidedProperties()
    {
        var beanDto = new BeanDTO()
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
