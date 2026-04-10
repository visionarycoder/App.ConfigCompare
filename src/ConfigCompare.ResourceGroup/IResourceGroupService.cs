using ConfigCompare.ResourceGroup.Resources;

namespace ConfigCompare.ResourceGroup;

/// <summary>
/// Service for enumerating Azure resource groups and resolving App Configuration and Key Vault endpoints within them.
/// </summary>
public interface IResourceGroupService
{
    /// <summary>
    /// Retrieves all Azure resource groups accessible to the currently authenticated user across all subscriptions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A response containing the list of resource groups or an error message.</returns>
    Task<GetResourceGroupsResponse> GetResourceGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the Azure App Configuration and Key Vault endpoints within a specific resource group.
    /// </summary>
    /// <param name="resourceGroupName">The name of the resource group to query.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A response containing the discovered services or an error message.</returns>
    Task<GetServicesInGroupResponse> GetServicesInGroupAsync(string resourceGroupName, CancellationToken cancellationToken = default);
}
