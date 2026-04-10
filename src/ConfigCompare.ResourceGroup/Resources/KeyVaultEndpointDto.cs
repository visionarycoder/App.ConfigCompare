namespace ConfigCompare.ResourceGroup.Resources;

/// <summary>
/// Represents an Azure Key Vault endpoint discovered within a resource group.
/// </summary>
public record KeyVaultEndpointDto(
    string Name,
    string VaultUri);
