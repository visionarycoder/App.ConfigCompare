#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConfigCompare.AppConfig;

namespace ConfigCompare.Desktop.ViewModels;

/// <summary>
/// View model for copying settings between App Configuration instances
/// </summary>
public partial class CopySettingsViewModel : ObservableObject
{
    private readonly IAppConfigService appConfigService;

    [ObservableProperty]
    private string sourceEndpoint = string.Empty;

    [ObservableProperty]
    private string targetEndpoint = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private int copiedCount;

    [ObservableProperty]
    private string copiedKeysDisplay = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public CopySettingsViewModel(IAppConfigService appConfigService)
    {
        this.appConfigService = appConfigService;
    }

    [RelayCommand]
    public async Task ExecuteCopySettings(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(SourceEndpoint) || string.IsNullOrWhiteSpace(TargetEndpoint))
        {
            StatusMessage = "❌ Both Source and Target endpoints are required.";
            return;
        }

        if (SourceEndpoint == TargetEndpoint)
        {
            StatusMessage = "❌ Source and Target endpoints must be different.";
            return;
        }

        IsLoading = true;

        try
        {
            var response = await appConfigService.CopySettingsAsync(
                SourceEndpoint,
                TargetEndpoint,
                cancellationToken);

            if (response.Success)
            {
                CopiedCount = response.CopiedCount;
                CopiedKeysDisplay = response.CopiedKeys.Count > 0
                    ? string.Join(Environment.NewLine, response.CopiedKeys.Take(20))
                    : "(No settings copied)";
                
                if (response.CopiedKeys.Count > 20)
                {
                    CopiedKeysDisplay += $"\n... and {response.CopiedKeys.Count - 20} more";
                }

                StatusMessage = $"✓ Successfully copied {response.CopiedCount} setting(s)!";
            }
            else
            {
                StatusMessage = $"❌ Failed: {response.ErrorMessage}";
                CopiedCount = 0;
                CopiedKeysDisplay = string.Empty;
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
        SourceEndpoint = string.Empty;
        TargetEndpoint = string.Empty;
        StatusMessage = string.Empty;
        CopiedCount = 0;
        CopiedKeysDisplay = string.Empty;
    }
}
