#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using ConfigCompare.Settings.Resources;

namespace ConfigCompare.Desktop.ViewModels;

/// <summary>
/// View model that drives the application shell status bar and title information.
/// </summary>
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

    /// <summary>
    /// Updates the connection display text based on the saved connection DTO.
    /// </summary>
    public void UpdateConnection(AzureConnectionDto? connection)
    {
        if (connection is null || string.IsNullOrWhiteSpace(connection.Endpoint))
        {
            ConnectionDisplay = "Not connected";
            return;
        }

        // Show just the hostname for brevity
        if (Uri.TryCreate(connection.Endpoint, UriKind.Absolute, out var uri))
        {
            ConnectionDisplay = uri.Host;
        }
        else
        {
            var ep = connection.Endpoint.Trim();
            ConnectionDisplay = ep.Length > 50 ? string.Concat(ep.AsSpan(0, 47), "...") : ep;
        }
    }

    /// <summary>
    /// Returns the zoom level formatted as a percentage string (e.g. "100%").
    /// </summary>
    public string ZoomDisplayText => $"{ZoomLevel:0}%";
}
