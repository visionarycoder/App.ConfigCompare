namespace ConfigCompare.ResourceGroup.Resources;

/// <summary>
/// Represents an Azure resource group.
/// </summary>
public record ResourceGroupDto(
    string Name,
    string SubscriptionId,
    string Location,
    string? DisplayName);
