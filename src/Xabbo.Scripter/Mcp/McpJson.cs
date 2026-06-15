using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xabbo.Scripter.Mcp;

internal static class McpJson
{
    public static readonly JsonElement Null = JsonDocument.Parse("null").RootElement;

    public static readonly JsonSerializerOptions Wire = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public static readonly JsonSerializerOptions Result = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };
}
