using System.Text.Json;
using System.Text.Json.Serialization;

using Xabbo.Scripter.Mcp;

namespace Xabbo.Scripter.Mcp.Protocol;

public static class JsonRpcErrorCodes
{
    public const int ParseError = -32700;
    public const int InvalidRequest = -32600;
    public const int MethodNotFound = -32601;
    public const int InvalidParams = -32602;
    public const int InternalError = -32603;
}

public sealed class JsonRpcRequest
{
    [JsonPropertyName("jsonrpc")] public string JsonRpc { get; set; } = "2.0";
    [JsonPropertyName("id")] public JsonElement Id { get; set; }
    [JsonPropertyName("method")] public string? Method { get; set; }
    [JsonPropertyName("params")] public JsonElement Params { get; set; }

    [JsonIgnore]
    public bool IsNotification => Id.ValueKind == JsonValueKind.Undefined;
}

public sealed class JsonRpcError
{
    [JsonPropertyName("code")] public int Code { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("data")] public object? Data { get; set; }
}

public sealed class JsonRpcResponse
{
    [JsonPropertyName("jsonrpc")] public string JsonRpc { get; set; } = "2.0";
    [JsonPropertyName("id")] public JsonElement Id { get; set; }
    [JsonPropertyName("result")] public object? Result { get; set; }
    [JsonPropertyName("error")] public JsonRpcError? Error { get; set; }

    private static readonly object EmptyResult = new();

    public static JsonRpcResponse Success(JsonElement id, object? result) =>
        new() { Id = Normalize(id), Result = result ?? EmptyResult };

    public static JsonRpcResponse Failure(JsonElement id, int code, string message, object? data = null) =>
        new() { Id = Normalize(id), Error = new JsonRpcError { Code = code, Message = message, Data = data } };

    private static JsonElement Normalize(JsonElement id) =>
        id.ValueKind == JsonValueKind.Undefined ? McpJson.Null : id;
}
