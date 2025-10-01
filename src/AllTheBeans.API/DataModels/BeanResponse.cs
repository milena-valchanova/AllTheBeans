using System.Text.Json.Serialization;

namespace AllTheBeans.API.DataModels;

public class BeanResponse
{
    [JsonPropertyName("_id")]
    public Guid Id { get; set; }

    [JsonPropertyName("index")]
    public uint Index { get; set; }

    [JsonPropertyName("isBOTD")]
    public bool IsBOTD { get; set; }

    [JsonPropertyName("Cost")]
    public string Cost { get; set; } = string.Empty;

    [JsonPropertyName("Image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("colour")]
    public string Colour { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("Country")]
    public string Country { get; set; } = string.Empty;
}
