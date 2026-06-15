using System;

namespace Xabbo.Scripter.Mcp.Tools;

[AttributeUsage(AttributeTargets.Method)]
public sealed class McpToolAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public McpToolAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class McpParameterAttribute : Attribute
{
    public string Description { get; }

    public McpParameterAttribute(string description)
    {
        Description = description;
    }
}

public interface IMcpToolProvider { }
