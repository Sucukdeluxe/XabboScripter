using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xabbo.Scripter.Mcp.Protocol;

public static class McpMethods
{
    public const string Initialize = "initialize";
    public const string Initialized = "notifications/initialized";
    public const string Ping = "ping";
    public const string ListTools = "tools/list";
    public const string CallTool = "tools/call";
}

public static class McpConstants
{
    public const string DefaultProtocolVersion = "2025-06-18";
    public const string ServerName = "xabbo-scripter";
    public const string SessionHeader = "Mcp-Session-Id";
    public const string ProtocolVersionHeader = "MCP-Protocol-Version";
}

public sealed class Implementation
{
    [JsonPropertyName("name")] public string Name { get; set; } = McpConstants.ServerName;
    [JsonPropertyName("version")] public string Version { get; set; } = "1.0.0";
}

public sealed class ToolsCapability
{
    [JsonPropertyName("listChanged")] public bool ListChanged { get; set; }
}

public sealed class ServerCapabilities
{
    [JsonPropertyName("tools")] public ToolsCapability Tools { get; set; } = new();
}

public sealed class InitializeResult
{
    [JsonPropertyName("protocolVersion")] public string ProtocolVersion { get; set; } = McpConstants.DefaultProtocolVersion;
    [JsonPropertyName("capabilities")] public ServerCapabilities Capabilities { get; set; } = new();
    [JsonPropertyName("serverInfo")] public Implementation ServerInfo { get; set; } = new();
    [JsonPropertyName("instructions")] public string? Instructions { get; set; }
}

public sealed class Tool
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("inputSchema")] public object InputSchema { get; set; } = new { type = "object" };
}

public sealed class ListToolsResult
{
    [JsonPropertyName("tools")] public IReadOnlyList<Tool> Tools { get; set; } = Array.Empty<Tool>();
}

public sealed class ContentBlock
{
    [JsonPropertyName("type")] public string Type { get; set; } = "text";
    [JsonPropertyName("text")] public string? Text { get; set; }

    public static ContentBlock FromText(string text) => new() { Type = "text", Text = text };
}

public sealed class CallToolResult
{
    [JsonPropertyName("content")] public List<ContentBlock> Content { get; set; } = new();
    [JsonPropertyName("isError")] public bool IsError { get; set; }

    public static CallToolResult Text(string text) =>
        new() { Content = { ContentBlock.FromText(text) } };

    public static CallToolResult Failure(string text) =>
        new() { Content = { ContentBlock.FromText(text) }, IsError = true };
}
