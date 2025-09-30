using AllTheBeans.API.DataModels;
using AllTheBeans.Domain.DataModels;
using EnumsNET;
using System.Globalization;

namespace AllTheBeans.API.Mappers;

internal class BeansMapper(IConfiguration configuration) : IBeansMapper
{
    private const string _imagesLocationKey = "ImagesLocation";
    private const string _currencyCultureKey = "CurrencyCulture";

    private readonly string _imageLocation = configuration
        .GetValue<string>(_imagesLocationKey)
        ?? throw new ArgumentException($"{_imagesLocationKey} must be provided");
    private string _currencyCulture = configuration
        .GetValue<string>(_currencyCultureKey)
        ?? throw new ArgumentException($"{_currencyCultureKey} must be provided");

    public BeanResponse ToBeanResponse(IBeanDTO beanDTO)
        => new ()
        {
            Id = beanDTO.Id.ToString("N"),
            Name = beanDTO.Name,
            Description = beanDTO.Description,
            Country = beanDTO.CountryName,
            Index = beanDTO.Index,
            IsBOTD = beanDTO.IsBOTD,
            Colour = beanDTO.Colour.AsString(EnumFormat.Description) 
                ?? "undefined",
            Cost = beanDTO.Cost.ToString("C", new CultureInfo(_currencyCulture)),
            Image = _imageLocation + beanDTO.ImageName
        };
}
