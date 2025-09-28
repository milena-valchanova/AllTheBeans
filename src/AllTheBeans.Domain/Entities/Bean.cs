using AllTheBeans.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AllTheBeans.Domain.Entities;
internal class Bean
{
    [StringLength(24)]
    public string Id { get; set; } = string.Empty;

    [Required]
    public uint Index { get; set; }

    public bool IsBOTD { get; set; }

    [Required]
    public decimal Cost { get; set; }

    [Required]
    [StringLength(50)]
    public string ImageName { get; set; } = string.Empty;

    public BeanColour Colour { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public long CountryId { get; set; }
}
