using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.Hosting;

using Wpf.Ui.Appearance;

using Xabbo.Extension;
using Xabbo.GEarth;
using Xabbo.Scripter.ViewModel;

namespace Xabbo.Scripter.Services;

public class ScripterLifetime : IHostLifetime
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly Application _application;
    private readonly Window _window;
    private readonly IUiContext _uiContext;
    private readonly SettingsViewManager _settings;
    private readonly IRemoteExtension _extension;
    private readonly MainViewManager _mainViewManager;
    private bool _windowInitialized;
    private bool _shownOnce;

    public ScripterLifetime(
        IHostApplicationLifetime lifetime,
        Application application,
        Window window,
        IUiContext uiContext,
        SettingsViewManager settings,
        IRemoteExtension extension,
        MainViewManager mainViewManager)
    {
        _lifetime = lifetime;
        _application = application;
        _window = window;
        _uiContext = uiContext;
        _settings = settings;
        _extension = extension;
        _mainViewManager = mainViewManager;

        _application.Exit += OnApplicationExit;
        _extension.InterceptorDisconnected += OnInterceptorDisconnected;

        if (_extension is GEarthExtension gearth)
        {
            gearth.Clicked += OnExtensionClicked;
        }

        _lifetime.ApplicationStarted.Register(OnApplicationStarted);

        _extension.Initialized += OnExtensionInitialized;
    }

    private void OnApplicationStarted()
    {
        _uiContext.Invoke(() =>
        {
            _application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        });

        Task.Run(async () =>
        {
            await _mainViewManager.InitializeAsync(CancellationToken.None);
        });
    }

    private void OnExtensionClicked(object? sender, EventArgs e) => BringToFront();

    private void OnExtensionInitialized(object? sender, ExtensionInitializedEventArgs e)
    {
        if (_shownOnce) return;
        _shownOnce = true;
        BringToFront();
    }

    public void BringToFront()
    {
        _uiContext.Invoke(() =>
        {
            InitializeWindow();
            _window.Show();
            _window.Activate();
            if (_window.WindowState == WindowState.Minimized)
                _window.WindowState = WindowState.Normal;
        });
    }

    private void InitializeWindow()
    {
        if (_windowInitialized) return;
        _windowInitialized = true;

        _application.MainWindow = _window;

        _window.Closing += OnWindowClosing;

        if (!_settings.DarkMode)
        {
            Theme.Apply(ThemeType.Light, updateAccent: false);
            if (_window is View.MainWindow mw)
                mw.ApplyFallbackBackground(false);
        }
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_extension.IsInterceptorConnected)
        {
            e.Cancel = true;
            _window.Hide();
        }
    }

    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void OnApplicationExit(object sender, ExitEventArgs e)
    {
        _lifetime.StopApplication();
    }

    private async void OnInterceptorDisconnected(object? sender, EventArgs e)
    {
        await Task.Delay(1500);
        _uiContext.Invoke(() => _application.Shutdown());
    }
}
