using System.Text.Json;
using System.Text.Json.Serialization;

namespace Streaik.OpenAI.Models.Realtime;

public class OpenAiRealtimeResponse
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = null!;

    [JsonPropertyName("expires_at")]
    public int ExpiresAt { get; set; }

    [JsonPropertyName("session")]
    public JsonElement Session { get; set; }
}
