using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Options;

using Xabbo.Scripter.Engine;

namespace Xabbo.Scripter.Mcp.Tools;

public sealed class ApiTools : IMcpToolProvider
{
    private readonly ScripterApiCatalog _catalog;
    private readonly IOptions<ScriptEngineOptions> _options;

    public ApiTools(ScripterApiCatalog catalog, IOptions<ScriptEngineOptions> options)
    {
        _catalog = catalog;
        _options = options;
    }

    [McpTool("list_api", "List every member of the scripting API (the globals available to scripts) as a compact index of signatures.")]
    public object ListApi()
    {
        return new
        {
            count = _catalog.Members.Count,
            members = _catalog.Members.Select(m => m.Signature).ToList()
        };
    }

    [McpTool("get_api", "Get detailed scripting API members (signature plus documentation) optionally filtered by a search term that matches the name, signature or documentation.")]
    public object GetApi(
        [McpParameter("Optional case-insensitive search term. Omit to return the entire API.")] string? search = null)
    {
        List<object> members = _catalog.Search(search)
            .Select(m => (object)new { m.Name, m.Kind, m.Signature, m.Summary, m.IsAsync })
            .ToList();

        return new { count = members.Count, members };
    }

    [McpTool("get_imports", "List the default namespace imports and referenced assemblies available to every script.")]
    public object GetImports()
    {
        return new
        {
            imports = _options.Value.Imports ?? new List<string>(),
            references = _options.Value.References ?? new List<string>()
        };
    }
}
