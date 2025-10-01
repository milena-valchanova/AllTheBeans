using AllTheBeans.Domain.Enums;

namespace AllTheBeans.Domain.DataModels;
internal class CreateOrUpdateBeanDTO : ICreateOrUpdateBeanDTO
{
    public uint Index { get; set; }
    public bool IsBOTD { get; set; }
    public decimal Cost { get; set; }
    public string ImageName { get; set; } = string.Empty;
    public BeanColour Colour { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
}
