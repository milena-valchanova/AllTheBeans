using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AllTheBeans.API.DataModels;

public class UpdateBeanPayload : IUpdateBeanDTO, IValidatableObject
{
    [JsonPropertyName("index")]
    public uint? Index { get; set; }

    [JsonPropertyName("isBOTD")]
    public bool? IsBOTD { get; set; }

    [Range(0, (double)decimal.MaxValue)]
    [JsonPropertyName("cost")]
    public decimal? Cost { get; set; }

    [StringLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$")]
    [JsonPropertyName("image")]
    public string? ImageName { get; set; }

    [EnumDataType(typeof(BeanColour))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("colour")]
    public BeanColour? Colour { get; set; }

    [StringLength(100)]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [StringLength(2000)]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [StringLength(100)]
    [JsonPropertyName("country")]
    public string? CountryName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if(
            Index is null 
            && IsBOTD is null 
            && Cost is null
            && ImageName is null
            && Colour is null
            && Name is null
            && Description is null
            && CountryName is null)
        {
            yield return new ValidationResult(
                $"At least one value should be provided.",
                [
                    nameof(Index),
                    nameof(IsBOTD),
                    nameof(Cost),
                    nameof(ImageName),
                    nameof(Colour),
                    nameof(Name),
                    nameof(Description),
                    nameof(CountryName)
                    ]);
        }
    }
}
