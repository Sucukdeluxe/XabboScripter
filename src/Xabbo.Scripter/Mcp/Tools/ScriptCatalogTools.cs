using System.Collections.Generic;
using System.Linq;

using Xabbo.Scripter.Services;
using Xabbo.Scripter.ViewModel;

namespace Xabbo.Scripter.Mcp.Tools;

public sealed class ScriptCatalogTools : IMcpToolProvider
{
    private readonly ScriptsViewManager _scripts;
    private readonly IUiContext _ui;

    public ScriptCatalogTools(ScriptsViewManager scripts, IUiContext ui)
    {
        _scripts = scripts;
        _ui = ui;
    }

    [McpTool("list_scripts", "List every script known to the scripter (both saved on disk and unsaved tabs) with its name, group, file name and current status.")]
    public object ListScripts()
    {
        return _ui.Invoke(() =>
        {
            List<object> scripts = _scripts.GetScripts().Select(s => ScriptInfo.Of(s)).ToList();
            return (object)new { count = scripts.Count, scripts };
        });
    }

    [McpTool("get_script", "Get the full source code and status of a single script, identified by file name (with or without .csx) or display name.")]
    public object GetScript(
        [McpParameter("The script file name (with or without .csx) or display name.")] string script)
    {
        return _ui.Invoke(() =>
        {
            ScriptViewModel? viewModel = _scripts.FindScript(script);
            if (viewModel is null)
                throw new McpToolException($"No script found matching '{script}'.");

            if (viewModel.IsSavedToDisk && !viewModel.IsLoaded)
                viewModel.Load();

            return ScriptInfo.Of(viewModel, includeCode: true);
        });
    }

    [McpTool("search_scripts", "Search scripts by a term matched against the script name, group and source code. Returns matching scripts with their code.")]
    public object SearchScripts(
        [McpParameter("Case-insensitive term to match against name, group or code.")] string query)
    {
        return _ui.Invoke(() =>
        {
            List<object> matches = new();

            foreach (ScriptViewModel viewModel in _scripts.GetScripts())
            {
                if (viewModel.IsSavedToDisk && !viewModel.IsLoaded)
                {
                    try { viewModel.Load(); } catch { }
                }

                bool matched =
                    viewModel.Name.Contains(query, System.StringComparison.OrdinalIgnoreCase) ||
                    viewModel.Group.Contains(query, System.StringComparison.OrdinalIgnoreCase) ||
                    viewModel.Code.Contains(query, System.StringComparison.OrdinalIgnoreCase);

                if (matched)
                    matches.Add(ScriptInfo.Of(viewModel, includeCode: true));
            }

            return (object)new { count = matches.Count, scripts = matches };
        });
    }
}
