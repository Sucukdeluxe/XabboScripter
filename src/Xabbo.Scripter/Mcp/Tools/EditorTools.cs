using System.Collections.Generic;

using Xabbo.Scripter.Configuration;
using Xabbo.Scripter.Services;
using Xabbo.Scripter.ViewModel;

namespace Xabbo.Scripter.Mcp.Tools;

public sealed class EditorTools : IMcpToolProvider
{
    private readonly ScriptsViewManager _scripts;
    private readonly IUiContext _ui;
    private readonly McpConfig _config;

    public EditorTools(ScriptsViewManager scripts, IUiContext ui, McpConfig config)
    {
        _scripts = scripts;
        _ui = ui;
        _config = config;
    }

    [McpTool("list_tabs", "List the editor tabs currently open in the scripter window, in order, marking which one is active.")]
    public object ListTabs()
    {
        return _ui.Invoke(() =>
        {
            List<object> tabs = new();

            for (int index = 0; index < _scripts.OpenTabs.Count; index++)
            {
                ScriptViewModel viewModel = _scripts.OpenTabs[index];
                tabs.Add(new
                {
                    index,
                    active = ReferenceEquals(_scripts.SelectedTabItem, viewModel),
                    name = viewModel.Name,
                    fileName = viewModel.FileName,
                    modified = viewModel.IsModified,
                    running = viewModel.IsRunning,
                    status = viewModel.StatusText
                });
            }

            return (object)new { count = tabs.Count, tabs };
        });
    }

    [McpTool("get_active_tab", "Get the script in the currently active editor tab, including its full source code.")]
    public object GetActiveTab()
    {
        return _ui.Invoke(() =>
        {
            if (_scripts.SelectedTabItem is not ScriptViewModel viewModel)
                throw new McpToolException("No editor tab is currently active.");

            return ScriptInfo.Of(viewModel, includeCode: true);
        });
    }

    [McpTool("open_script", "Open an existing script in a visible editor tab and switch to it so the user can see it.")]
    public object OpenScript(
        [McpParameter("The script file name (with or without .csx) or display name.")] string script)
    {
        McpGuard.Require(_config.AllowEditor, "editor");

        return _ui.Invoke(() =>
        {
            ScriptViewModel? viewModel = _scripts.FindScript(script);
            if (viewModel is null)
                throw new McpToolException($"No script found matching '{script}'.");

            _scripts.SelectScript(viewModel);
            return ScriptInfo.Of(viewModel, includeCode: true);
        });
    }

    [McpTool("create_script_tab", "Create a new script with the given code and open it as a visible editor tab so the user can watch it live. The script is not yet saved to disk.")]
    public object CreateScriptTab(
        [McpParameter("The C# script source code.")] string code)
    {
        McpGuard.Require(_config.AllowEditor, "editor");

        return _ui.Invoke(() =>
        {
            ScriptViewModel viewModel = _scripts.NewScript(code, open: true);
            return ScriptInfo.Of(viewModel, includeCode: true);
        });
    }

    [McpTool("edit_tab", "Replace the code of an open editor tab live, so the change is visible in the editor immediately. Opens the script first if it is not already in a tab.")]
    public object EditTab(
        [McpParameter("The script file name (with or without .csx) or display name.")] string script,
        [McpParameter("The new full C# script source code.")] string code)
    {
        McpGuard.Require(_config.AllowEditor, "editor");

        return _ui.Invoke(() =>
        {
            ScriptViewModel? viewModel = _scripts.FindScript(script);
            if (viewModel is null)
                throw new McpToolException($"No script found matching '{script}'.");

            if (!_scripts.OpenTabs.Contains(viewModel))
                _scripts.SelectScript(viewModel);

            viewModel.ReplaceCode(code);
            _scripts.SelectedTabItem = viewModel;

            return ScriptInfo.Of(viewModel, includeCode: true);
        });
    }

    [McpTool("select_tab", "Switch the active editor tab to the given script, opening it if necessary.")]
    public object SelectTab(
        [McpParameter("The script file name (with or without .csx) or display name.")] string script)
    {
        McpGuard.Require(_config.AllowEditor, "editor");

        return _ui.Invoke(() =>
        {
            ScriptViewModel? viewModel = _scripts.FindScript(script);
            if (viewModel is null)
                throw new McpToolException($"No script found matching '{script}'.");

            _scripts.SelectScript(viewModel);
            return ScriptInfo.Of(viewModel);
        });
    }

    [McpTool("close_tab", "Close the editor tab for the given script.")]
    public object CloseTab(
        [McpParameter("The script file name (with or without .csx) or display name.")] string script)
    {
        McpGuard.Require(_config.AllowEditor, "editor");

        return _ui.Invoke(() =>
        {
            ScriptViewModel? viewModel = _scripts.FindScript(script);
            if (viewModel is null)
                throw new McpToolException($"No script found matching '{script}'.");

            _scripts.CloseScript(viewModel);
            return (object)new { closed = true, fileName = viewModel.FileName };
        });
    }
}
