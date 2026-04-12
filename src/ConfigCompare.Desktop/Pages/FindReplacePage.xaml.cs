using ConfigCompare.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ConfigCompare.AppConfig;

namespace ConfigCompare.Desktop.Pages;

public sealed partial class FindReplacePage : Page
{
    private readonly FindReplaceViewModel _viewModel;

    public FindReplacePage()
    {
        this.InitializeComponent();
        var svc = App.ServiceProvider.GetService<IAppConfigService>();
        _viewModel = svc is not null
            ? new FindReplaceViewModel(svc)
            : throw new System.InvalidOperationException("IAppConfigService not registered");
    }

    private async void Execute_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ConfigEndpoint = EndpointBox.Text;
        _viewModel.FindValue = FindBox.Text;
        _viewModel.ReplaceValue = ReplaceBox.Text;

        LoadingRing.IsActive = true;
        ResultsPanel.Visibility = Visibility.Collapsed;

        await _viewModel.ExecuteFindReplaceCommand.ExecuteAsync(null);

        LoadingRing.IsActive = false;
        StatusText.Text = _viewModel.StatusMessage;

        if (_viewModel.ReplacementCount > 0 || !string.IsNullOrEmpty(_viewModel.AffectedKeysDisplay))
        {
            CountText.Text = $"Replacements: {_viewModel.ReplacementCount}";
            AffectedKeysText.Text = _viewModel.AffectedKeysDisplay;
            ResultsPanel.Visibility = Visibility.Visible;
        }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ClearFormCommand.Execute(null);
        EndpointBox.Text = string.Empty;
        FindBox.Text = string.Empty;
        ReplaceBox.Text = string.Empty;
        StatusText.Text = string.Empty;
        ResultsPanel.Visibility = Visibility.Collapsed;
    }
}
