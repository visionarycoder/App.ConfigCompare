using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ConfigCompare.Desktop.Pages;

public sealed partial class SettingsPage : Page
{
    public string AppVersion { get; } =
        typeof(SettingsPage).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
            ?? typeof(SettingsPage).Assembly.GetName().Version?.ToString()
            ?? "1.0.0";

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = (ThemeSelector.SelectedItem as RadioButton)?.Tag as string;
        var theme = selected switch
        {
            "Light" => ElementTheme.Light,
            "Dark"  => ElementTheme.Dark,
            _       => ElementTheme.Default
        };
        if (App.MainWindowInstance?.Content is FrameworkElement root)
            root.RequestedTheme = theme;
    }
}
