using AllTheBeans.Domain.Enums;

namespace AllTheBeans.Domain.DataModels;
public interface ICreateOrUpdateBeanDTO
{
    uint Index { get; }
    bool IsBOTD { get; }
    decimal Cost { get; }
    string ImageName { get; }
    BeanColour Colour { get; }
    string Name { get; }
    string Description { get; }
    string CountryName { get; }
}
