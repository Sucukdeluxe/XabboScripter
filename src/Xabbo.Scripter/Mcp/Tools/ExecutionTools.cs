using System;
using System.Threading;
using System.Threading.Tasks;

using Xabbo.Scripter.Configuration;
using Xabbo.Scripter.Engine;
using Xabbo.Scripter.Model;
using Xabbo.Scripter.Services;
using Xabbo.Scripter.ViewModel;

namespace Xabbo.Scripter.Mcp.Tools;

public sealed class ExecutionTools : IMcpToolProvider
{
    private readonly ScriptsViewManager _scripts;
    private readonly ScriptEngine _engine;
    private readonly IScriptHost _host;
    private readonly IUiContext _ui;
    private readonly McpConfig _config;

    public ExecutionTools(ScriptsViewManager scripts, ScriptEngine engine, IScriptHost host, IUiContext ui, McpConfig config)
    {
        _scripts = scripts;
        _engine = engine;
        _host = host;
        _ui = ui;
        _config = config;
    }

    [McpTool("run_script", "Compile and run an existing script in the background. By default waits for completion and returns its log output and any error; set wait=false to start it and return immediately.")]
    public async Task<object> RunScript(
        [McpParameter("The script file name (with or without .csx) or display name.")] string script,
        [McpParameter("Wait for the script to finish before returning. Defaults to true.")] bool wait = true,
        [McpParameter("Maximum time to wait in milliseconds when wait=true. Defaults to 30000.")] int timeoutMs = 30000,
        CancellationToken cancellationToken = default)
    {
        McpGuard.Require(_config.AllowExecute, "execute");
        RequireConnected();

        ScriptViewModel viewModel = _ui.Invoke(() =>
        {
            ScriptViewModel? found = _scripts.FindScript(script)
                ?? throw new McpToolException($"No script found matching '{script}'.");

            if (found.IsSavedToDisk && !found.IsLoaded)
                found.Load();

            return found;
        });

        if (viewModel.IsWorking)
            return Snapshot(viewModel, note: "already running");

        return await RunAsync(viewModel, wait, timeoutMs, cancellationToken).ConfigureAwait(false);
    }

    [McpTool("run_code", "Compile and run ad-hoc C# script code in the background and return its log output and any errors. Set visible=true to show it as a live editor tab the user can watch; otherwise it runs hidden, which is ideal for inspecting game state.")]
    public async Task<object> RunCode(
        [McpParameter("The C# script source code to run.")] string code,
        [McpParameter("Show the code as a visible editor tab. Defaults to false (runs hidden).")] bool visible = false,
        [McpParameter("Wait for the script to finish before returning. Defaults to true.")] bool wait = true,
        [McpParameter("Maximum time to wait in milliseconds when wait=true. Defaults to 30000.")] int timeoutMs = 30000,
        CancellationToken cancellationToken = default)
    {
        McpGuard.Require(_config.AllowExecute, "execute");
        RequireConnected();

        ScriptViewModel viewModel;

        if (visible)
        {
            viewModel = _ui.Invoke(() => _scripts.NewScript(code, open: true));
        }
        else
        {
            viewModel = _ui.Invoke(() =>
            {
                ScriptViewModel hidden = new(_engine, new ScriptModel { FileName = $"mcp-{Guid.NewGuid():N}.csx" }) { IsLoaded = true };
                hidden.Code = code;
                return hidden;
            });
        }

        return await RunAsync(viewModel, wait, timeoutMs, cancellationToken).ConfigureAwait(false);
    }

    [McpTool("cancel_script", "Cancel a running or compiling script.")]
    public object CancelScript(
        [McpParameter("The script file name (with or without .csx) or display name.")] string script)
    {
        return _ui.Invoke(() =>
        {
            ScriptViewModel viewModel = _scripts.FindScript(script)
                ?? throw new McpToolException($"No script found matching '{script}'.");

            viewModel.CancelCommand.Execute(null);
            return (object)new { cancelling = true, script = ScriptInfo.Of(viewModel) };
        });
    }

    [McpTool("get_script_status", "Get the current run status of a script: state, runtime, full log output and the last compile or runtime error.")]
    public object GetScriptStatus(
        [McpParameter("The script file name (with or without .csx) or display name.")] string script)
    {
        ScriptViewModel viewModel = _ui.Invoke(() => _scripts.FindScript(script))
            ?? throw new McpToolException($"No script found matching '{script}'.");

        return Snapshot(viewModel);
    }

    private async Task<object> RunAsync(ScriptViewModel viewModel, bool wait, int timeoutMs, CancellationToken cancellationToken)
    {
        Task runTask = Task.Run(() => _engine.Run(viewModel));

        if (!wait)
            return Snapshot(viewModel, note: "started");

        if (timeoutMs > 0)
        {
            Task finished = await Task.WhenAny(runTask, Task.Delay(timeoutMs, cancellationToken)).ConfigureAwait(false);
            if (finished != runTask)
                return Snapshot(viewModel, note: "timed out waiting; still running");
        }

        await runTask.ConfigureAwait(false);
        return Snapshot(viewModel);
    }

    private object Snapshot(ScriptViewModel viewModel, string? note = null)
    {
        return _ui.Invoke(() => (object)new
        {
            note,
            status = viewModel.StatusText,
            running = viewModel.IsRunning,
            working = viewModel.IsWorking,
            faulted = viewModel.IsFaulted,
            runtimeMs = viewModel.Runtime?.TotalMilliseconds,
            output = viewModel.ResultText,
            error = viewModel.ErrorText
        });
    }

    private void RequireConnected()
    {
        if (!_host.CanExecute)
            throw new McpToolException("The scripter is not connected to the game yet, so scripts cannot run. Check get_server_info.");
    }
}
