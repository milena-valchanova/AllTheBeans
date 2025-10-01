using AllTheBeans.Domain.Entities.Constants;
using AllTheBeans.Domain.Enums;
using AllTheBeans.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AllTheBeans.Domain.Entities;


[Index(nameof(Name), IsUnique = true)]
public class Bean
{
    public Guid Id { get; set; }

    [Required]
    public uint Index { get; set; }

    public bool IsBOTD { get; set; }

    [Required]
    public decimal Cost { get; set; }

    [Required]
    [StringLength(PropertyLengths.ImageName)]
    public string ImageName { get; set; } = string.Empty;

    public BeanColour Colour { get; set; }

    [Required]
    [StringLength(PropertyLengths.Name)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(PropertyLengths.Description)]
    public string Description { get; set; } = string.Empty;

    public long CountryId { get; set; }

    private Country? _country;
    public Country Country
    {
        get => _country 
            ?? throw new PropertyNotInitialisedException(nameof(Country));
        set => _country = value;
    }

    private HashSet<BeanOfTheDay>? _beansOfTheDay;
    public HashSet<BeanOfTheDay> BeansOfTheDay
    {
        get => _beansOfTheDay
            ?? throw new PropertyNotInitialisedException(nameof(BeansOfTheDay));
        set => _beansOfTheDay = value;
    }
}
