using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

using MaterialDesignThemes.Wpf;

using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

using Xabbo.Scripter.ViewModel;

namespace Xabbo.Scripter.View;

public partial class MainWindow : UiWindow, INavigationWindow
{
    private readonly INavigationService _nav;
    private bool _firstActivation = true;

    public MainWindow(MainViewManager manager,
        INavigationService nav,
        IPageService pageService)
    {
        _nav = nav;
        DataContext = manager;

        InitializeComponent();

        _nav.SetNavigationControl(RootNavigation);
        SetPageService(pageService);

        Activated += MainWindow_Activated;

        RootFrame.Navigating += RootFrame_Navigating;
    }

    private void RootFrame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
    {
        if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Refresh)
            e.Cancel = true;
    }

    private void ButtonPin_Click(object sender, RoutedEventArgs e) => Topmost = !Topmost;

    private void MainWindow_Activated(object? sender, EventArgs e)
    {
        if (!_firstActivation) return;
        _firstActivation = false;

        Navigate(typeof(Pages.LogPage));

        Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Dark, updateAccent: false);
        Wpf.Ui.Appearance.Background.Apply(this, Wpf.Ui.Appearance.BackgroundType.Mica);
        ApplyFallbackBackground(true);

        Dispatcher.InvokeAsync(WarmupUi, DispatcherPriority.ApplicationIdle);
    }

    private async void WarmupUi()
    {
        try
        {
            ContextMenu warmMenu = new()
            {
                Placement = PlacementMode.Absolute,
                HorizontalOffset = -10000,
                VerticalOffset = -10000
            };
            warmMenu.Items.Add(new System.Windows.Controls.MenuItem { Header = "warmup" });
            warmMenu.Opened += (_, _) => warmMenu.IsOpen = false;
            warmMenu.IsOpen = true;
        }
        catch { }

        try
        {
            await DialogHost.Show(
                new TextInputModalViewModel { Message = string.Empty },
                "Root",
                new DialogOpenedEventHandler((_, args) => args.Session.Close(false)));
        }
        catch { }

        try
        {
            await DialogHost.Show(
                new MessageBoxViewModel { Message = string.Empty, Buttons = MessageBoxButton.YesNo },
                "Root",
                new DialogOpenedEventHandler((_, args) => args.Session.Close(false)));
        }
        catch { }
    }

    internal void ApplyFallbackBackground(bool dark)
    {
        if (Environment.OSVersion.Version.Build >= 22000) return;
        Background = new System.Windows.Media.SolidColorBrush(
            dark ? System.Windows.Media.Color.FromRgb(0x20, 0x20, 0x20)
                 : System.Windows.Media.Color.FromRgb(0xF3, 0xF3, 0xF3));
    }


    #region - INavigationWindow -
    public void CloseWindow() => Close();

    public Frame GetFrame() => RootFrame;

    public INavigation GetNavigation() => RootNavigation;

    public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

    public void SetPageService(IPageService pageService) => RootNavigation.PageService = pageService;

    public void ShowWindow() => Show();
    #endregion
}
