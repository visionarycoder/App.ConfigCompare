using ConfigCompare.Desktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ConfigCompare.Desktop.Pages;

public sealed partial class ConfigurationsPage : Page
{
    public ConfigurationsViewModel ViewModel => SharedState.Configurations;

    public ConfigurationsPage()
    {
        this.InitializeComponent();
    }

    private async void AddConfig_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NewEndpoint = EndpointBox.Text;
        await ViewModel.AddConfigurationAsync();
        EndpointBox.Text = string.Empty;
    }

    private async void EndpointBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            ViewModel.NewEndpoint = EndpointBox.Text;
            await ViewModel.AddConfigurationAsync();
            EndpointBox.Text = string.Empty;
        }
    }

    private void RemoveConfig_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is ConfigurationTabViewModel tab)
        {
            ViewModel.RemoveConfiguration(tab);
            KeyValueListView.ItemsSource = null;
        }
    }

    private void ConfigListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ConfigListView.SelectedItem is ConfigurationTabViewModel tab)
        {
            ViewModel.SelectedConfiguration = tab;
            KeyValueListView.ItemsSource = tab.Items;
        }
        else
        {
            KeyValueListView.ItemsSource = null;
        }
    }
}
