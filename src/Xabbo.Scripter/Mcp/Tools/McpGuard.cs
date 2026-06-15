namespace Xabbo.Scripter.Mcp.Tools;

internal static class McpGuard
{
    public static void Require(bool allowed, string capability)
    {
        if (!allowed)
            throw new McpToolException($"The '{capability}' capability is disabled in the scripter MCP settings.");
    }
}
