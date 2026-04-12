using ConfigCompare.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ConfigCompare.AppConfig;

namespace ConfigCompare.Desktop.Pages;

public sealed partial class EditConfigPage : Page
{
    private readonly EditConfigViewModel _viewModel;

    public EditConfigPage()
    {
        this.InitializeComponent();
        var svc = App.ServiceProvider.GetService<IAppConfigService>();
        _viewModel = svc is not null
            ? new EditConfigViewModel(svc)
            : throw new System.InvalidOperationException("IAppConfigService not registered");
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ConfigEndpoint = EndpointBox.Text;
        _viewModel.ConfigKey = KeyBox.Text;
        _viewModel.ConfigValue = ValueBox.Text;
        _viewModel.ConfigLabel = LabelBox.Text;

        SaveBtn.IsEnabled = false;
        LoadingRing.IsActive = true;

        await _viewModel.SaveConfigurationCommand.ExecuteAsync(null);

        LoadingRing.IsActive = false;
        SaveBtn.IsEnabled = true;
        StatusText.Text = _viewModel.StatusMessage;

        if (_viewModel.IsSaveSuccessful)
        {
            KeyBox.Text = string.Empty;
            ValueBox.Text = string.Empty;
            LabelBox.Text = string.Empty;
        }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ClearFormCommand.Execute(null);
        EndpointBox.Text = string.Empty;
        KeyBox.Text = string.Empty;
        ValueBox.Text = string.Empty;
        LabelBox.Text = string.Empty;
        StatusText.Text = string.Empty;
    }
}
