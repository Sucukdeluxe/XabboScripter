using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xabbo.Scripter.Configuration;

public class McpConfig
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "xabbo", "scripter", "mcp.json"
    );

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public bool Enabled { get; set; } = true;
    public bool StartOnLaunch { get; set; } = true;
    public int Port { get; set; } = 9090;
    public bool RequireAuthToken { get; set; } = true;
    public string AuthToken { get; set; } = string.Empty;
    public bool AllowExecute { get; set; } = true;
    public bool AllowFileWrite { get; set; } = true;
    public bool AllowEditor { get; set; } = true;

    [JsonIgnore]
    public int ActivePort { get; set; }

    public int EffectivePort => ActivePort > 0 ? ActivePort : Port;

    public string Endpoint => $"http://127.0.0.1:{EffectivePort}/mcp";

    public static McpConfig Load()
    {
        McpConfig config;

        try
        {
            config = File.Exists(ConfigPath)
                ? JsonSerializer.Deserialize<McpConfig>(File.ReadAllText(ConfigPath), JsonOptions) ?? new McpConfig()
                : new McpConfig();
        }
        catch
        {
            config = new McpConfig();
        }

        if (string.IsNullOrWhiteSpace(config.AuthToken))
        {
            config.AuthToken = GenerateToken();
            config.Save();
        }

        return config;
    }

    public void Save()
    {
        try
        {
            string? dir = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this, JsonOptions));
        }
        catch { }
    }

    public string RegenerateToken()
    {
        AuthToken = GenerateToken();
        Save();
        return AuthToken;
    }

    private static string GenerateToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
}
