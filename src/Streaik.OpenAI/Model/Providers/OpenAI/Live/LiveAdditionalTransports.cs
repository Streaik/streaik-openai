using System.Text.Json;
using System.Text.Json.Serialization;

namespace Streaik.OpenAI.Model.Providers.OpenAI.Live;

/// <summary>Opaque additional transport properties. Validation belongs to the selected provider.</summary>
public sealed class LiveAdditionalRequestTransport(string type) : LiveRequestTransport(type)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Opaque additional transport properties; native OpenAI transports retain their exact shapes.</summary>
public sealed class LiveAdditionalResponseTransport(string type) : LiveResponseTransport(type)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
