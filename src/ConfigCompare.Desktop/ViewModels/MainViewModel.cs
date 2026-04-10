#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ConfigCompare.Desktop.ViewModels;

/// <summary>
/// Main view model for the application shell
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private int selectedTabIndex;

    [ObservableProperty]
    private string applicationTitle = "Configuration Comparison Tool";

    public MainViewModel()
    {
        SelectedTabIndex = 0;
    }

    [RelayCommand]
    public void NavigateToTab(int tabIndex)
    {
        SelectedTabIndex = tabIndex;
    }
}
