#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConfigCompare.AppConfig;

namespace ConfigCompare.Desktop.ViewModels;

/// <summary>
/// View model for finding and replacing values in configurations
/// </summary>
public partial class FindReplaceViewModel : ObservableObject
{
    private readonly IAppConfigService appConfigService;

    [ObservableProperty]
    private string configEndpoint = string.Empty;

    [ObservableProperty]
    private string findValue = string.Empty;

    [ObservableProperty]
    private string replaceValue = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private int replacementCount;

    [ObservableProperty]
    private string affectedKeysDisplay = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public FindReplaceViewModel(IAppConfigService appConfigService)
    {
        this.appConfigService = appConfigService;
    }

    [RelayCommand]
    public async Task ExecuteFindReplace(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ConfigEndpoint) || 
            string.IsNullOrWhiteSpace(FindValue) || 
            string.IsNullOrWhiteSpace(ReplaceValue))
        {
            StatusMessage = "❌ Endpoint, Find Value, and Replace Value are all required.";
            return;
        }

        IsLoading = true;

        try
        {
            var response = await appConfigService.FindAndReplaceAsync(
                ConfigEndpoint,
                FindValue,
                ReplaceValue,
                cancellationToken: cancellationToken);

            if (response.Success)
            {
                ReplacementCount = response.ReplacementCount;
                AffectedKeysDisplay = response.AffectedKeys.Count > 0 
                    ? string.Join(Environment.NewLine, response.AffectedKeys)
                    : "(No affected keys)";
                StatusMessage = $"✓ Found and replaced {response.ReplacementCount} occurrence(s)!";
            }
            else
            {
                StatusMessage = $"❌ Failed: {response.ErrorMessage}";
                ReplacementCount = 0;
                AffectedKeysDisplay = string.Empty;
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
        FindValue = string.Empty;
        ReplaceValue = string.Empty;
        StatusMessage = string.Empty;
        ReplacementCount = 0;
        AffectedKeysDisplay = string.Empty;
    }
}
