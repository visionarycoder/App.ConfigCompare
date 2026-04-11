#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using ConfigCompare.Settings.Resources;

namespace ConfigCompare.Desktop.ViewModels;

/// <summary>
/// View model for the Azure App Configuration connection dialog.
/// </summary>
public partial class ConnectionViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private string endpoint = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowServicePrincipalFields))]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private AuthMethod authMethod = AuthMethod.DefaultAzureCredential;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private string tenantId = string.Empty;

    [ObservableProperty]
    private string subscriptionId = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private string clientId = string.Empty;

    [ObservableProperty]
    private string clientSecret = string.Empty;

    /// <summary>Whether the Service Principal credential fields should be shown.</summary>
    public bool ShowServicePrincipalFields => AuthMethod == AuthMethod.ServicePrincipal;

    /// <summary>Whether the current form values constitute a valid connection.</summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Endpoint) &&
        (AuthMethod != AuthMethod.ServicePrincipal ||
         (!string.IsNullOrWhiteSpace(TenantId) && !string.IsNullOrWhiteSpace(ClientId)));

    /// <summary>Populates the view model fields from a saved <see cref="AzureConnectionDto"/>.</summary>
    public void LoadFrom(AzureConnectionDto dto)
    {
        Endpoint = dto.Endpoint;
        AuthMethod = dto.AuthMethod;
        TenantId = dto.TenantId ?? string.Empty;
        SubscriptionId = dto.SubscriptionId ?? string.Empty;
        ClientId = dto.ClientId ?? string.Empty;
        ClientSecret = dto.ClientSecret ?? string.Empty;
    }

    /// <summary>Converts current view model values into a <see cref="AzureConnectionDto"/>.</summary>
    public AzureConnectionDto ToDto() => new(
        Endpoint.Trim(),
        AuthMethod,
        string.IsNullOrWhiteSpace(TenantId) ? null : TenantId.Trim(),
        string.IsNullOrWhiteSpace(SubscriptionId) ? null : SubscriptionId.Trim(),
        string.IsNullOrWhiteSpace(ClientId) ? null : ClientId.Trim(),
        string.IsNullOrWhiteSpace(ClientSecret) ? null : ClientSecret.Trim());
}
