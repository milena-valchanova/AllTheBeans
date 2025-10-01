using AllTheBeans.API.Controllers;
using AllTheBeans.API.Mappers;
using AllTheBeans.Domain.Services;
using NSubstitute;
using NSubstitute.ClearExtensions;

namespace AllTheBeans.API.UnitTests;

[TestFixture(TestOf = typeof(BeansController))]
internal class BeansControllerTests
{
    private readonly IBeansMapper _beansMapper = Substitute.For<IBeansMapper>();
    private readonly IBeansService _beansService = Substitute.For<IBeansService>();
    private BeansController BeansController => new (_beansMapper, _beansService);

    [TearDown]
    public void TearDown()
    {
        _beansMapper.ClearSubstitute();
        _beansService.ClearSubstitute();
    }

    [Test]
    [Description("Current date should be used when determining the bean of the day")]
    public async Task CurrentDate_ShouldBe_Used_When_DeterminingTheBeanOfTheDay()
    {
        var _ = await BeansController.BeanOfTheDay(default);

        DateOnly expectedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        await _beansService
            .Received(1)
            .GetOrCreateBeanOfTheDayAsync(expectedDate, Arg.Any<CancellationToken>());
    }
}
