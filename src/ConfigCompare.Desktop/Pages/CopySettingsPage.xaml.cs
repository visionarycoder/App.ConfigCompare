using ConfigCompare.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ConfigCompare.AppConfig;

namespace ConfigCompare.Desktop.Pages;

public sealed partial class CopySettingsPage : Page
{
    private readonly CopySettingsViewModel _viewModel;

    public CopySettingsPage()
    {
        this.InitializeComponent();
        var svc = App.ServiceProvider.GetService<IAppConfigService>();
        _viewModel = svc is not null
            ? new CopySettingsViewModel(svc)
            : throw new System.InvalidOperationException("IAppConfigService not registered");
    }

    private async void Execute_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SourceEndpoint = SourceBox.Text;
        _viewModel.TargetEndpoint = TargetBox.Text;

        LoadingRing.IsActive = true;
        ResultsPanel.Visibility = Visibility.Collapsed;

        await _viewModel.ExecuteCopySettingsCommand.ExecuteAsync(null);

        LoadingRing.IsActive = false;
        StatusText.Text = _viewModel.StatusMessage;

        if (_viewModel.CopiedCount > 0 || !string.IsNullOrEmpty(_viewModel.CopiedKeysDisplay))
        {
            CountText.Text = $"Copied: {_viewModel.CopiedCount} setting(s)";
            CopiedKeysText.Text = _viewModel.CopiedKeysDisplay;
            ResultsPanel.Visibility = Visibility.Visible;
        }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ClearFormCommand.Execute(null);
        SourceBox.Text = string.Empty;
        TargetBox.Text = string.Empty;
        StatusText.Text = string.Empty;
        ResultsPanel.Visibility = Visibility.Collapsed;
    }
}
