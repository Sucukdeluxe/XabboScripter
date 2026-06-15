using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Xabbo.Scripter.Mcp.Protocol;
using Xabbo.Scripter.Mcp.Tools;

namespace Xabbo.Scripter.Mcp.Server;

public sealed class McpDispatcher
{
    private static readonly string[] SupportedProtocolVersions =
    {
        "2025-11-25",
        "2025-06-18",
        "2025-03-26",
        "2024-11-05"
    };

    private readonly McpToolRegistry _registry;
    private readonly string _version;

    public McpDispatcher(McpToolRegistry registry)
    {
        _registry = registry;
        _version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0";
    }

    public async Task<JsonRpcResponse?> DispatchAsync(JsonRpcRequest request, CancellationToken cancellationToken)
    {
        if (request.IsNotification)
            return null;

        switch (request.Method)
        {
            case McpMethods.Initialize:
                return JsonRpcResponse.Success(request.Id, Initialize(request.Params));

            case McpMethods.Ping:
                return JsonRpcResponse.Success(request.Id, new { });

            case McpMethods.ListTools:
                return JsonRpcResponse.Success(request.Id, new ListToolsResult { Tools = _registry.Tools });

            case McpMethods.CallTool:
                return await CallToolAsync(request, cancellationToken).ConfigureAwait(false);

            default:
                return JsonRpcResponse.Failure(request.Id, JsonRpcErrorCodes.MethodNotFound, $"Method not found: {request.Method}");
        }
    }

    private InitializeResult Initialize(JsonElement parameters)
    {
        string protocolVersion = McpConstants.DefaultProtocolVersion;

        if (parameters.ValueKind == JsonValueKind.Object &&
            parameters.TryGetProperty("protocolVersion", out JsonElement requested) &&
            requested.ValueKind == JsonValueKind.String)
        {
            string? value = requested.GetString();
            if (!string.IsNullOrEmpty(value))
                protocolVersion = System.Array.IndexOf(SupportedProtocolVersions, value) >= 0 ? value : McpConstants.DefaultProtocolVersion;
        }

        return new InitializeResult
        {
            ProtocolVersion = protocolVersion,
            ServerInfo = new Implementation { Name = McpConstants.ServerName, Version = _version },
            Instructions =
                "This server controls the xabbo scripter, a C# scripting interface for the Habbo client via G-Earth. " +
                "Scripts are C# script files (.csx) with top-level statements that call a rich game API (the globals class 'G'). " +
                "Start by calling 'get_started' for the workflow, 'get_scripting_guide' for how to write scripts, " +
                "'list_api' / 'get_api' to discover every callable game function, and 'list_mcp_tools' to see every tool you can use. " +
                "You can read and live-edit open editor tabs, create and run scripts in the background, inspect the current room and " +
                "game state, read error logs, and manage autostart scripts."
        };
    }

    private async Task<JsonRpcResponse> CallToolAsync(JsonRpcRequest request, CancellationToken cancellationToken)
    {
        if (request.Params.ValueKind != JsonValueKind.Object ||
            !request.Params.TryGetProperty("name", out JsonElement nameElement) ||
            nameElement.ValueKind != JsonValueKind.String)
        {
            return JsonRpcResponse.Failure(request.Id, JsonRpcErrorCodes.InvalidParams, "Missing tool name.");
        }

        string name = nameElement.GetString()!;

        JsonElement arguments = request.Params.TryGetProperty("arguments", out JsonElement argumentsElement)
            ? argumentsElement
            : default;

        CallToolResult result = await _registry.CallAsync(name, arguments, cancellationToken).ConfigureAwait(false);
        return JsonRpcResponse.Success(request.Id, result);
    }
}
