#nullable enable

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigCompare.AppConfig.Resources;

namespace ConfigCompare.Desktop.ViewModels;

public partial class ConfigurationTabViewModel : ObservableObject
{
    [ObservableProperty]
    private string endpoint = string.Empty;

    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public ObservableCollection<ConfigurationItemDto> Items { get; } = new();
}
