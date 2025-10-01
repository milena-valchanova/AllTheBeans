using AllTheBeans.Domain.Enums;

namespace AllTheBeans.Domain.DataModels;
public interface ICreateOrUpdateBeanDTO : IUpdateBeanDTO
{
    new uint Index { get; }
    new bool IsBOTD { get; }
    new decimal Cost { get; }
    new string ImageName { get; }
    new BeanColour Colour { get; }
    new string Name { get; }
    new string Description { get; }
    new string CountryName { get; }
}
