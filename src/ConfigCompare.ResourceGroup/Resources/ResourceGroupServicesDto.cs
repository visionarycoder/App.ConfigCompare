namespace ConfigCompare.ResourceGroup.Resources;

/// <summary>
/// Represents the Azure App Configuration and Key Vault services discovered within a specific resource group.
/// </summary>
public record ResourceGroupServicesDto(
    string ResourceGroupName,
    IReadOnlyList<AppConfigEndpointDto> AppConfigEndpoints,
    IReadOnlyList<KeyVaultEndpointDto> KeyVaultEndpoints);
