using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;

namespace AllTheBeans.API.DataModels;

public class CreateBeanPayload : ICreateBeanDTO, IValidatableObject
{
    [Required]
    [JsonPropertyName("index")]
    public uint? Index { get; set; }

    [JsonPropertyName("isBOTD")]
    public bool IsBOTD { get; set; }

    [Required]
    [Range(0, (double)decimal.MaxValue)]
    [JsonPropertyName("cost")]
    public decimal? Cost { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$")]
    [JsonPropertyName("image")]
    public string ImageName { get; set; } = string.Empty;

    [EnumDataType(typeof(BeanColour))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("colour")]
    public BeanColour Colour { get; set; }

    [Required]
    [StringLength(100)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [JsonPropertyName("country")]
    public string CountryName { get; set; } = string.Empty;

    uint ICreateBeanDTO.Index => Index!.Value;

    decimal ICreateBeanDTO.Cost => Cost!.Value;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var regions = CultureInfo
            .GetCultures(CultureTypes.SpecificCultures)
            .Select(x => new RegionInfo(x.Name));
        if (!regions.Any(p => p.EnglishName == CountryName))
        {
            yield return new ValidationResult(
                $"Full name of the country should be used.",
                [nameof(CountryName)]);
        }
    }
}
