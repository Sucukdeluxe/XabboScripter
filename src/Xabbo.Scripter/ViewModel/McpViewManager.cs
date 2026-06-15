using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using MaterialDesignThemes.Wpf;

using Xabbo.Scripter.Configuration;
using Xabbo.Scripter.Mcp.Integration;
using Xabbo.Scripter.Mcp.Server;

namespace Xabbo.Scripter.ViewModel;

public class McpViewManager : ObservableObject
{
    private readonly McpServer _server;
    private readonly McpConfig _config;
    private readonly McpClientConfigurator _configurator;
    private readonly ISnackbarMessageQueue _snackbar;

    public McpServer Server => _server;

    public string Endpoint => _config.Endpoint;
    public string AuthToken => _config.AuthToken;

    public string ClaudeCommand => _configurator.ClaudeCommand();
    public string GeminiCommand => _configurator.GeminiCommand();
    public string CodexToml => _configurator.CodexToml();

    public bool Enabled
    {
        get => _config.Enabled;
        set { if (_config.Enabled == value) return; _config.Enabled = value; _config.Save(); RaisePropertyChanged(); }
    }

    public bool StartOnLaunch
    {
        get => _config.StartOnLaunch;
        set { if (_config.StartOnLaunch == value) return; _config.StartOnLaunch = value; _config.Save(); RaisePropertyChanged(); }
    }

    public int Port
    {
        get => _config.Port;
        set
        {
            if (_config.Port == value || value <= 0 || value > 65535) return;
            _config.Port = value;
            _config.Save();
            RaisePropertyChanged();
            RaiseConnectionInfo();
        }
    }

    public bool RequireAuthToken
    {
        get => _config.RequireAuthToken;
        set
        {
            if (_config.RequireAuthToken == value) return;
            _config.RequireAuthToken = value;
            _config.Save();
            RaisePropertyChanged();
            RaiseConnectionInfo();
        }
    }

    public bool AllowExecute
    {
        get => _config.AllowExecute;
        set { if (_config.AllowExecute == value) return; _config.AllowExecute = value; _config.Save(); RaisePropertyChanged(); }
    }

    public bool AllowFileWrite
    {
        get => _config.AllowFileWrite;
        set { if (_config.AllowFileWrite == value) return; _config.AllowFileWrite = value; _config.Save(); RaisePropertyChanged(); }
    }

    public bool AllowEditor
    {
        get => _config.AllowEditor;
        set { if (_config.AllowEditor == value) return; _config.AllowEditor = value; _config.Save(); RaisePropertyChanged(); }
    }

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand RestartCommand { get; }
    public ICommand RegenerateTokenCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand InstallClaudeCommand { get; }
    public ICommand InstallGeminiCommand { get; }
    public ICommand InstallCodexCommand { get; }

    public McpViewManager(
        McpServer server,
        McpConfig config,
        McpClientConfigurator configurator,
        ISnackbarMessageQueue snackbar)
    {
        _server = server;
        _config = config;
        _configurator = configurator;
        _snackbar = snackbar;

        _server.PropertyChanged += OnServerPropertyChanged;

        StartCommand = new RelayCommand(async () => await _server.StartServerAsync());
        StopCommand = new RelayCommand(async () => await _server.StopServerAsync());
        RestartCommand = new RelayCommand(async () => await _server.RestartServerAsync());

        RegenerateTokenCommand = new RelayCommand(() =>
        {
            _config.RegenerateToken();
            RaiseConnectionInfo();
            _snackbar.Enqueue("A new authentication token was generated. Reconnect your clients.");
        });

        CopyCommand = new RelayCommand<string>(text =>
        {
            if (string.IsNullOrEmpty(text)) return;
            try { Clipboard.SetText(text); _snackbar.Enqueue("Copied to clipboard."); }
            catch { _snackbar.Enqueue("Failed to copy to clipboard."); }
        });

        InstallClaudeCommand = new RelayCommand(() => Install(_configurator.InstallClaude(), "Claude Code"));
        InstallGeminiCommand = new RelayCommand(() => Install(_configurator.InstallGemini(), "Gemini CLI"));
        InstallCodexCommand = new RelayCommand(() => Install(_configurator.InstallCodex(), "Codex CLI"));
    }

    private void OnServerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(McpServer.Endpoint) || e.PropertyName == nameof(McpServer.IsRunning))
            RaiseConnectionInfo();
    }

    private void Install(McpInstallResult result, string client)
    {
        _snackbar.Enqueue(result.Ok
            ? $"{client}: {result.Message}"
            : $"{client}: failed — {result.Message}");
    }

    private void RaiseConnectionInfo()
    {
        RaisePropertyChanged(nameof(Endpoint));
        RaisePropertyChanged(nameof(AuthToken));
        RaisePropertyChanged(nameof(ClaudeCommand));
        RaisePropertyChanged(nameof(GeminiCommand));
        RaisePropertyChanged(nameof(CodexToml));
    }
}
