namespace Xabbo.Scripter.Mcp.Tools;

public sealed class DocsTools : IMcpToolProvider
{
    [McpTool("get_started", "Get an orientation overview of this MCP server and the recommended workflow for creating, running and debugging scripts.")]
    public string GetStarted() => McpDocs.GettingStarted;

    [McpTool("get_scripting_guide", "Get a guide explaining how to write xabbo scripter scripts (syntax, async, the cancellation token, metadata directives and examples).")]
    public string GetScriptingGuide() => McpDocs.ScriptingGuide;

    [McpTool("get_knowledgebase", "Get the full xabbo scripter knowledgebase: a dense field guide (the API by domain, packets/headers/interception, events, data models, proven recipes mined from real scripts, a debugging playbook and a cheat sheet) distilled from the entire source. Read this once to understand the whole system before writing scripts.")]
    public string GetKnowledgebase() => McpDocs.Knowledgebase;
}
