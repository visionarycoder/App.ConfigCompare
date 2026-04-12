#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using ConfigCompare.Settings.Resources;

namespace ConfigCompare.Desktop.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private string connectionDisplay = "Not connected";

    [ObservableProperty]
    private int configSettingsCount = 0;

    [ObservableProperty]
    private double zoomLevel = 100.0;

    [ObservableProperty]
    private string versionInfo = string.Empty;

    [ObservableProperty]
    private string userDisplayName = "Signing in...";

    [ObservableProperty]
    private string currentTheme = "System";

    public void UpdateConnection(AzureConnectionDto? connection)
    {
        if (connection is null || string.IsNullOrWhiteSpace(connection.Endpoint))
        {
            ConnectionDisplay = "Not connected";
            return;
        }

        if (Uri.TryCreate(connection.Endpoint, UriKind.Absolute, out var uri))
            ConnectionDisplay = uri.Host;
        else
        {
            var ep = connection.Endpoint.Trim();
            ConnectionDisplay = ep.Length > 50 ? string.Concat(ep.AsSpan(0, 47), "...") : ep;
        }
    }

    public string ZoomDisplayText => $"{ZoomLevel:0}%";
}
