using System.Text.Json.Serialization;

namespace AllTheBeans.API.DataModels;

public class BeansResponse
{
    [JsonPropertyName("beans")]
    public IEnumerable<BeanResponse> Beans { get; set; } = [];

    [JsonPropertyName("total")]
    public int Total { get; set; }
}
