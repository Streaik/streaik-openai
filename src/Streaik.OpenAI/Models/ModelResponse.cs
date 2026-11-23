using System.Text.Json.Serialization;

namespace Streaik.OpenAI.Models;

public class ModelResponse
{
    [JsonPropertyName("object")]
    public string Object { get; } = "list";

    [JsonPropertyName("data")]
    public IEnumerable<Model> Data { get; set; } = [];
}
