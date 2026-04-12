#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ConfigCompare.AppConfig;

namespace ConfigCompare.Desktop.ViewModels;

public partial class ConfigurationsViewModel : ObservableObject
{
    private readonly IAppConfigService? _appConfigService;

    public ObservableCollection<ConfigurationTabViewModel> Configurations { get; } = new();

    [ObservableProperty]
    private ConfigurationTabViewModel? selectedConfiguration;

    [ObservableProperty]
    private string newEndpoint = string.Empty;

    public ConfigurationsViewModel()
    {
        try { _appConfigService = App.ServiceProvider.GetService<IAppConfigService>(); } catch { }
    }

    [RelayCommand]
    public async Task AddConfigurationAsync()
    {
        var endpoint = NewEndpoint.Trim();
        if (string.IsNullOrWhiteSpace(endpoint)) return;

        if (_appConfigService is null)
        {
            await ShowErrorAsync("Service Unavailable", "App Configuration service is not available.");
            return;
        }

        var tab = new ConfigurationTabViewModel
        {
            Endpoint = endpoint,
            DisplayName = TryGetHostname(endpoint),
            IsLoading = true,
            StatusMessage = "Loading..."
        };
        Configurations.Add(tab);
        SelectedConfiguration = tab;
        NewEndpoint = string.Empty;

        try
        {
            var response = await _appConfigService.GetConfigurationsAsync(endpoint);
            if (response.Success)
            {
                tab.Items.Clear();
                foreach (var item in response.Configurations)
                    tab.Items.Add(item);
                tab.StatusMessage = $"{tab.Items.Count} item(s) loaded.";
            }
            else
            {
                tab.StatusMessage = $"Error: {response.ErrorMessage}";
                await ShowErrorAsync("Load Failed", response.ErrorMessage ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            tab.StatusMessage = $"Error: {ex.Message}";
            await ShowErrorAsync("Load Failed", ex.Message);
        }
        finally
        {
            tab.IsLoading = false;
        }
    }

    [RelayCommand]
    public void RemoveConfiguration(ConfigurationTabViewModel tab)
    {
        Configurations.Remove(tab);
        if (SelectedConfiguration == tab)
            SelectedConfiguration = Configurations.Count > 0 ? Configurations[0] : null;
    }

    [RelayCommand]
    public async Task RefreshConfigurationAsync(ConfigurationTabViewModel tab)
    {
        if (_appConfigService is null || string.IsNullOrWhiteSpace(tab.Endpoint)) return;
        tab.IsLoading = true;
        tab.StatusMessage = "Refreshing...";
        try
        {
            var response = await _appConfigService.GetConfigurationsAsync(tab.Endpoint);
            if (response.Success)
            {
                tab.Items.Clear();
                foreach (var item in response.Configurations)
                    tab.Items.Add(item);
                tab.StatusMessage = $"{tab.Items.Count} item(s) loaded.";
            }
            else
            {
                tab.StatusMessage = $"Error: {response.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            tab.StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            tab.IsLoading = false;
        }
    }

    private static string TryGetHostname(string endpoint)
    {
        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
            return uri.Host;
        return endpoint.Length > 40 ? endpoint[..37] + "..." : endpoint;
    }

    private static async Task ShowErrorAsync(string title, string message)
    {
        if (App.MainWindowInstance is MainWindow mw)
            await mw.ShowErrorAsync(title, message);
    }
}
