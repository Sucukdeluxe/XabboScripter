using System.IO;
using System.Reflection;

namespace Xabbo.Scripter.Mcp;

internal static class McpDocs
{
    public static string GettingStarted { get; } = Read("getting_started.md");
    public static string ScriptingGuide { get; } = Read("scripting_guide.md");
    public static string Knowledgebase { get; } = Read("knowledgebase.md");

    private static string Read(string fileName)
    {
        string resource = $"Xabbo.Scripter.Mcp.Resources.{fileName}";

        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
        if (stream is null)
            return $"Resource '{resource}' was not found.";

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
