using System;
using System.Windows.Controls;

using Xabbo.Scripter.ViewModel;

namespace Xabbo.Scripter.View.Pages;

public partial class SettingsPage : Page
{
    private readonly SettingsViewManager _manager;

    public SettingsPage(SettingsViewManager manager, McpViewManager mcp)
    {
        DataContext = _manager = manager;

        InitializeComponent();

        McpSection.DataContext = mcp;
    }
}
