using System;
using System.Reflection;

namespace Xabbo.Scripter.Util;

internal static class GitVersionUtil
{
    public static string? GetSemanticVersion(Assembly assembly)
    {
        var fromGitVersion = assembly
            .GetType("GitVersionInformation")
            ?.GetField("SemVer")
            ?.GetValue(null) as string;
        if (!string.IsNullOrEmpty(fromGitVersion))
            return fromGitVersion;

        var info = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (string.IsNullOrEmpty(info))
            return null;

        int plus = info.IndexOf('+');
        return plus > 0 ? info.Substring(0, plus) : info;
    }
}
