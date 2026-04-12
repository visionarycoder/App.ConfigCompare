using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ConfigCompare.AzureIdentity.Resources;
using ConfigCompare.Desktop.Pages;

namespace ConfigCompare.Desktop;

public sealed partial class MainWindow : Window
{
    private readonly IAuthService? _authService;
    private bool _hasLoaded;

    public MainWindow()
    {
        this.InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        try { _authService = App.ServiceProvider.GetService<IAuthService>(); } catch { }

        Activated += MainWindow_Activated;
    }

    private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (_hasLoaded) return;
        _hasLoaded = true;
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(ConfigurationsPage));
        await LoadUserInfoAsync();
    }

    private async Task LoadUserInfoAsync()
    {
        if (_authService is null) { UserNameTextBlock.Text = "Not signed in"; return; }
        try
        {
            var result = await _authService.GetCurrentUserAsync();
            UserNameTextBlock.Text = result.IsSuccess && result.Data is not null
                ? result.Data.DisplayName
                : "Not signed in";
        }
        catch { UserNameTextBlock.Text = "Not signed in"; }
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
            ContentFrame.Navigate(typeof(SettingsPage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }
        var item = args.SelectedItem as NavigationViewItem;
        var tag = item?.Tag as string;
        Type? pageType = tag switch
        {
            "Configurations" => typeof(ConfigurationsPage),
            "Compare"        => typeof(ComparePage),
            "EditConfig"     => typeof(EditConfigPage),
            "FindReplace"    => typeof(FindReplacePage),
            "CopySettings"   => typeof(CopySettingsPage),
            _                => null
        };
        if (pageType is not null)
            ContentFrame.Navigate(pageType);
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = (ThemeComboBox.SelectedItem as ComboBoxItem)?.Tag as string;
        var theme = selected switch
        {
            "Light" => ElementTheme.Light,
            "Dark"  => ElementTheme.Dark,
            _       => ElementTheme.Default
        };
        if (Content is FrameworkElement root)
            root.RequestedTheme = theme;
    }

    public async Task ShowErrorAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "Dismiss",
            XamlRoot = this.Content.XamlRoot
        };
        await dialog.ShowAsync();
    }
}
