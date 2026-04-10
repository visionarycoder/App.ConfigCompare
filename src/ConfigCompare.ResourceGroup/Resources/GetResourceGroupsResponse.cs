namespace ConfigCompare.ResourceGroup.Resources;

/// <summary>
/// Response from GetResourceGroupsAsync containing a list of resource groups or an error.
/// </summary>
public record GetResourceGroupsResponse(
    bool IsSuccess,
    string? ErrorMessage,
    IReadOnlyList<ResourceGroupDto>? Data);
