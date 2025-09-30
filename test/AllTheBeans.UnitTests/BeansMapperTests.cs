using AllTheBeans.API.Mappers;
using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace AllTheBeans.API.UnitTests;

[TestFixture(TestOf = typeof(BeansMapper))]
internal class BeansMapperTests
{
    private IConfiguration GetConfiguration(string currencyCulture = "en-GB") 
        => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>()
        {
            ["ImagesLocation"] = "https://some.location.com/",
            ["CurrencyCulture"] = currencyCulture
        })
        .Build();

    [TestCase("en-GB", "£5.00")]
    [TestCase("en-US", "$5.00")]
    [TestCase("ja-JP", "￥5")]
    [TestCase("da-DK", "5,00 kr.")]
    [Description("Cost currency is correctly formatted depending on settings")]
    public void CurrencyIsCorrectlyFormattedDependingOnSettings(string currencyCulture, string expectedCost)
    {
        var beanDTO = new BeanDTO()
        {
            Cost = 5
        };
        var configuration = GetConfiguration(currencyCulture);
        var mapper = new BeansMapper(configuration);

        var result = mapper.ToBeanResponse(beanDTO);

        Assert.That(result.Cost, Is.EqualTo(expectedCost));
    }

    [TestCase(default, "undefined")]
    [TestCase(BeanColour.Green, "green")]
    [TestCase(BeanColour.Golden, "golden")]
    [TestCase(BeanColour.LightRoast, "light roast")]
    [TestCase(BeanColour.MediumRoast, "medium roast")]
    [TestCase(BeanColour.DarkRoast, "dark roast")]
    [Description("Cost currency is correctly formatted depending on settings")]
    public void ColourIsCorrectlyDisplayed(BeanColour beanColour, string expectedColour)
    {
        var beanDTO = new BeanDTO()
        {
            Colour = beanColour
        };
        var mapper = new BeansMapper(GetConfiguration());

        var result = mapper.ToBeanResponse(beanDTO);

        Assert.That(result.Colour, Is.EqualTo(expectedColour));
    }
}
