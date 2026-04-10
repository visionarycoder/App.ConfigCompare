#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConfigCompare.AppConfig;
using ConfigCompare.AppConfig.Resources;

namespace ConfigCompare.Desktop.ViewModels;

/// <summary>
/// View model for editing and committing configurations to Azure App Configuration
/// </summary>
public partial class EditConfigViewModel : ObservableObject
{
    private readonly IAppConfigService appConfigService;

    [ObservableProperty]
    private string configEndpoint = string.Empty;

    [ObservableProperty]
    private string configKey = string.Empty;

    [ObservableProperty]
    private string configValue = string.Empty;

    [ObservableProperty]
    private string configLabel = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isSaveSuccessful;

    public EditConfigViewModel(IAppConfigService appConfigService)
    {
        this.appConfigService = appConfigService;
    }

    [RelayCommand]
    public async Task SaveConfiguration(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ConfigEndpoint) || string.IsNullOrWhiteSpace(ConfigKey))
        {
            StatusMessage = "❌ Endpoint and Key are required.";
            return;
        }

        IsLoading = true;
        IsSaveSuccessful = false;

        try
        {
            var config = new ConfigurationItemDto
            {
                Key = ConfigKey,
                Value = ConfigValue,
                Label = ConfigLabel
            };

            var response = await appConfigService.UpdateConfigurationAsync(ConfigEndpoint, config, cancellationToken);

            if (response.Success)
            {
                StatusMessage = $"✓ Configuration '{ConfigKey}' saved successfully!";
                IsSaveSuccessful = true;
                ConfigValue = string.Empty;
                ConfigLabel = string.Empty;
                ConfigKey = string.Empty;
            }
            else
            {
                StatusMessage = $"❌ Failed to save: {response.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void ClearForm()
    {
        ConfigEndpoint = string.Empty;
        ConfigKey = string.Empty;
        ConfigValue = string.Empty;
        ConfigLabel = string.Empty;
        StatusMessage = string.Empty;
        IsSaveSuccessful = false;
    }
}
