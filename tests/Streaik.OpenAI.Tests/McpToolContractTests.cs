using System.Text.Json;
using Streaik.OpenAI.Model.Providers.OpenAI;
using Xunit;

namespace Streaik.OpenAI.Tests;

public sealed class McpToolContractTests
{
    [Theory]
    [InlineData("[\"lookup\"]", "\"always\"")]
    [InlineData("[]", "\"never\"")]
    [InlineData("{\"read_only\":false,\"tool_names\":[\"lookup\"]}", "{\"always\":{\"tool_names\":[\"write\"]},\"never\":{\"read_only\":true}}")]
    public void Supported_MCP_configuration_round_trips(string allowed, string approval)
    {
        var wire = $$"""
            {"type":"mcp","server_label":"inventory","server_url":"https://inventory.example.test/mcp","server_description":"Inventory",
             "allowed_callers":["direct","programmatic"],"defer_loading":false,"allowed_tools":{{allowed}},"require_approval":{{approval}},
             "authorization":"isolated-fixture-token","headers":{"X-Region":"eu"}}
            """;
        var tool = JsonSerializer.Deserialize<OpenAiMcpTool>(wire)!;
        Assert.Equal([OpenAiMcpCaller.Direct, OpenAiMcpCaller.Programmatic], tool.AllowedCallers);
        Assert.Equal("eu", tool.Headers!["X-Region"]);
        Assert.True(JsonElement.DeepEquals(JsonSerializer.Deserialize<JsonElement>(wire), JsonSerializer.SerializeToElement(tool)));
    }

    [Fact]
    public void Optional_and_deprecated_fields_are_not_emitted()
    {
        var tool = JsonSerializer.Deserialize<OpenAiMcpTool>("""
            {"server_label":"inventory","connector_id":"connector_dropbox","tunnel_id":"ignored","require_approval":null,"allowed_tools":null}
            """)!;
        var fields = JsonSerializer.SerializeToElement(tool);
        Assert.Equal(2, fields.GetPropertyCount());
        Assert.Equal("mcp", fields.GetProperty("type").GetString());
        Assert.Equal("inventory", fields.GetProperty("server_label").GetString());
    }
}
