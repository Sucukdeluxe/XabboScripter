using Xabbo.Scripter.ViewModel;

namespace Xabbo.Scripter.Mcp.Tools;

internal static class ScriptInfo
{
    public static object Of(ScriptViewModel script, bool includeCode = false) => new
    {
        name = script.Name,
        group = string.IsNullOrEmpty(script.Group) ? null : script.Group,
        fileName = script.FileName,
        savedToDisk = script.IsSavedToDisk,
        loaded = script.IsLoaded,
        modified = script.IsModified,
        running = script.IsRunning,
        working = script.IsWorking,
        autostart = script.IsAutostart,
        status = script.StatusText,
        codeLength = script.Code.Length,
        code = includeCode ? script.Code : null
    };
}
