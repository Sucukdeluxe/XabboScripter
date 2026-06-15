using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

using Xabbo.Scripter.Services;
using Xabbo.Scripter.ViewModel;

namespace Xabbo.Scripter.Mcp.Tools;

public sealed class LogTools : IMcpToolProvider
{
    private readonly ObservableLoggerProvider _logger;
    private readonly ScriptsViewManager _scripts;
    private readonly IUiContext _ui;

    public LogTools(IEnumerable<ILoggerProvider> loggers, ScriptsViewManager scripts, IUiContext ui)
    {
        _logger = loggers.OfType<ObservableLoggerProvider>().First();
        _scripts = scripts;
        _ui = ui;
    }

    [McpTool("get_app_log", "Get the scripter's application log (engine, connection and error messages).")]
    public object GetAppLog(
        [McpParameter("Maximum number of characters to return from the end of the log. Defaults to 8000.")] int maxChars = 8000)
    {
        string text = _logger.Text;
        if (maxChars > 0 && text.Length > maxChars)
            text = text[^maxChars..];

        return new { log = text };
    }

    [McpTool("get_script_log", "Get the status/log output of a single script (the text shown under its editor).")]
    public object GetScriptLog(
        [McpParameter("The script file name (with or without .csx) or display name.")] string script)
    {
        return _ui.Invoke(() =>
        {
            ScriptViewModel viewModel = _scripts.FindScript(script)
                ?? throw new McpToolException($"No script found matching '{script}'.");

            return (object)new
            {
                fileName = viewModel.FileName,
                status = viewModel.StatusText,
                faulted = viewModel.IsFaulted,
                output = viewModel.ResultText
            };
        });
    }

    [McpTool("get_errors", "Get the last error for a specific script, or, when no script is given, every script currently in an error or faulted state.")]
    public object GetErrors(
        [McpParameter("Optional script file name or display name. Omit to list all errored scripts.")] string? script = null)
    {
        return _ui.Invoke(() =>
        {
            if (!string.IsNullOrWhiteSpace(script))
            {
                ScriptViewModel viewModel = _scripts.FindScript(script!)
                    ?? throw new McpToolException($"No script found matching '{script}'.");

                return (object)new
                {
                    fileName = viewModel.FileName,
                    status = viewModel.StatusText,
                    faulted = viewModel.IsFaulted,
                    error = viewModel.ErrorText,
                    output = viewModel.ResultText
                };
            }

            List<object> errors = _scripts.GetScripts()
                .Where(s => s.IsFaulted || s.IsRed)
                .Select(s => (object)new
                {
                    fileName = s.FileName,
                    name = s.Name,
                    status = s.StatusText,
                    error = s.ErrorText,
                    output = s.ResultText
                })
                .ToList();

            return (object)new { count = errors.Count, errors };
        });
    }
}
