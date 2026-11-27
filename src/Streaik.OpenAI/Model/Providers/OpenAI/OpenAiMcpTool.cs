using System.Text.Json;
using System.Text.Json.Serialization;

namespace Streaik.OpenAI.Model.Providers.OpenAI;

public sealed class OpenAiMcpTool
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "mcp";

    [JsonPropertyName("server_label")]
    public string ServerLabel { get; set; } = default!;

    [JsonPropertyName("server_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ServerUrl { get; set; }

    [JsonPropertyName("server_description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ServerDescription { get; set; }

    [JsonPropertyName("allowed_callers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<OpenAiMcpCaller>? AllowedCallers { get; set; }

    [JsonPropertyName("defer_loading")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? DeferLoading { get; set; }

    /// <summary>When omitted, the provider defaults to requiring approval for every call.</summary>
    [JsonPropertyName("require_approval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OpenAiMcpApprovalPolicy? RequireApproval { get; set; }

    [JsonPropertyName("allowed_tools")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OpenAiMcpAllowedTools? AllowedTools { get; set; }

    [JsonPropertyName("authorization")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Authorization { get; set; }

    [JsonPropertyName("headers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Headers { get; set; }
}

[JsonConverter(typeof(OpenAiMcpCallerConverter))]
public enum OpenAiMcpCaller
{
    [JsonStringEnumMemberName("direct")]
    Direct,
    [JsonStringEnumMemberName("programmatic")]
    Programmatic
}

internal sealed class OpenAiMcpCallerConverter() : JsonStringEnumConverter<OpenAiMcpCaller>(namingPolicy: null, allowIntegerValues: false);

public sealed class OpenAiMcpToolFilter
{
    [JsonPropertyName("read_only")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ReadOnly { get; set; }

    [JsonPropertyName("tool_names")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? ToolNames { get; set; }
}

public sealed class OpenAiMcpApprovalFilter
{
    [JsonPropertyName("always")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OpenAiMcpToolFilter? Always { get; set; }

    [JsonPropertyName("never")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OpenAiMcpToolFilter? Never { get; set; }
}

[JsonConverter(typeof(OpenAiMcpAllowedToolsConverter))]
public abstract record OpenAiMcpAllowedTools
{
    private OpenAiMcpAllowedTools() { }
    public sealed record Names(IReadOnlyList<string> Value) : OpenAiMcpAllowedTools;
    public sealed record Filter(OpenAiMcpToolFilter Value) : OpenAiMcpAllowedTools;
}

[JsonConverter(typeof(OpenAiMcpApprovalPolicyConverter))]
public abstract record OpenAiMcpApprovalPolicy
{
    private OpenAiMcpApprovalPolicy() { }
    public sealed record Always : OpenAiMcpApprovalPolicy;
    public sealed record Never : OpenAiMcpApprovalPolicy;
    public sealed record Filter(OpenAiMcpApprovalFilter Value) : OpenAiMcpApprovalPolicy;
}

internal sealed class OpenAiMcpAllowedToolsConverter : JsonConverter<OpenAiMcpAllowedTools>
{
    public override OpenAiMcpAllowedTools Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) => reader.TokenType switch
    {
        JsonTokenType.StartArray => new OpenAiMcpAllowedTools.Names(JsonSerializer.Deserialize<string[]>(ref reader, options)!),
        JsonTokenType.StartObject => new OpenAiMcpAllowedTools.Filter(JsonSerializer.Deserialize<OpenAiMcpToolFilter>(ref reader, options)!),
        _ => throw new JsonException("allowed_tools must be an array or a filter object.")
    };

    public override void Write(Utf8JsonWriter writer, OpenAiMcpAllowedTools value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case OpenAiMcpAllowedTools.Names names: JsonSerializer.Serialize(writer, names.Value, options); break;
            case OpenAiMcpAllowedTools.Filter filter: JsonSerializer.Serialize(writer, filter.Value, options); break;
            default: throw new JsonException("Unsupported allowed_tools value.");
        }
    }
}

internal sealed class OpenAiMcpApprovalPolicyConverter : JsonConverter<OpenAiMcpApprovalPolicy>
{
    public override OpenAiMcpApprovalPolicy Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) => reader.TokenType switch
    {
        JsonTokenType.String when reader.ValueTextEquals("always") => new OpenAiMcpApprovalPolicy.Always(),
        JsonTokenType.String when reader.ValueTextEquals("never") => new OpenAiMcpApprovalPolicy.Never(),
        JsonTokenType.StartObject => new OpenAiMcpApprovalPolicy.Filter(JsonSerializer.Deserialize<OpenAiMcpApprovalFilter>(ref reader, options)!),
        _ => throw new JsonException("require_approval must be 'always', 'never', or a filter object.")
    };

    public override void Write(Utf8JsonWriter writer, OpenAiMcpApprovalPolicy value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case OpenAiMcpApprovalPolicy.Always: writer.WriteStringValue("always"); break;
            case OpenAiMcpApprovalPolicy.Never: writer.WriteStringValue("never"); break;
            case OpenAiMcpApprovalPolicy.Filter filter: JsonSerializer.Serialize(writer, filter.Value, options); break;
            default: throw new JsonException("Unsupported require_approval value.");
        }
    }
}

