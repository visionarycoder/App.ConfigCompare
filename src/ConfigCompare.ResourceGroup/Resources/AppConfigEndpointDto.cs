namespace ConfigCompare.ResourceGroup.Resources;

/// <summary>
/// Represents an Azure App Configuration endpoint discovered within a resource group.
/// </summary>
public record AppConfigEndpointDto(
    string Name,
    string Endpoint);
