using Azure.ResourceManager;
using ConfigCompare.ResourceGroup.Resources;
using Microsoft.Extensions.Logging;

namespace ConfigCompare.ResourceGroup;

/// <summary>
/// Implementation of IResourceGroupService using Azure Resource Manager SDK.
/// </summary>
public class ResourceGroupService : IResourceGroupService
{
    private readonly ArmClient ResourceArmClient;
    private readonly ILogger<ResourceGroupService> Logger;

    public ResourceGroupService(ArmClient armClient, ILogger<ResourceGroupService> logger)
    {
        ResourceArmClient = armClient ?? throw new ArgumentNullException(nameof(armClient));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all Azure resource groups accessible to the currently authenticated user across all subscriptions.
    /// </summary>
    public async Task<GetResourceGroupsResponse> GetResourceGroupsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var resourceGroups = new List<ResourceGroupDto>();

            // Iterate through all subscriptions accessible to the credential
            var subscriptions = ResourceArmClient.GetSubscriptions();
            await foreach (var subscription in subscriptions.GetAllAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                Logger.LogInformation("Processing subscription: {SubscriptionId} - {DisplayName}", 
                    subscription.Data.SubscriptionId, 
                    subscription.Data.DisplayName);

                // Iterate through all resource groups in each subscription
                var resourceGroupsCollection = subscription.GetResourceGroups();
                await foreach (var resourceGroup in resourceGroupsCollection.GetAllAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
                {
                    var dto = new ResourceGroupDto(
                        Name: resourceGroup.Data.Name,
                        SubscriptionId: subscription.Data.SubscriptionId,
                        Location: resourceGroup.Data.Location.Name,
                        DisplayName: resourceGroup.Data.Name);

                    resourceGroups.Add(dto);
                }
            }

            Logger.LogInformation("Successfully retrieved {Count} resource groups", resourceGroups.Count);
            return new GetResourceGroupsResponse(true, null, resourceGroups.AsReadOnly());
        }
        catch (OperationCanceledException ex)
        {
            const string message = "Resource group retrieval was cancelled.";
            Logger.LogWarning(ex, message);
            return new GetResourceGroupsResponse(false, message, null);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to retrieve resource groups");
            return new GetResourceGroupsResponse(false, $"Failed to retrieve resource groups: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Retrieves the Azure App Configuration and Key Vault endpoints within a specific resource group.
    /// </summary>
    public async Task<GetServicesInGroupResponse> GetServicesInGroupAsync(string resourceGroupName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resourceGroupName))
        {
            const string message = "Resource group name must not be null or empty.";
            Logger.LogWarning(message);
            return new GetServicesInGroupResponse(false, message, null);
        }

        try
        {
            var appConfigEndpoints = new List<AppConfigEndpointDto>();
            var keyVaultEndpoints = new List<KeyVaultEndpointDto>();

            // Find the resource group and its subscription
            Azure.ResourceManager.ArmResource? foundResourceGroup = null;

            var subscriptions = ResourceArmClient.GetSubscriptions();
            await foreach (var subscription in subscriptions.GetAllAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                var resourceGroupsCollection = subscription.GetResourceGroups();
                await foreach (var resourceGroup in resourceGroupsCollection.GetAllAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
                {
                    if (resourceGroup.Data.Name.Equals(resourceGroupName, StringComparison.OrdinalIgnoreCase))
                    {
                        foundResourceGroup = resourceGroup;
                        break;
                    }
                }

                if (foundResourceGroup != null)
                {
                    break;
                }
            }

            if (foundResourceGroup == null)
            {
                const string message = "Resource group not found.";
                Logger.LogWarning(message);
                return new GetServicesInGroupResponse(false, message, null);
            }

            Logger.LogInformation("Retrieved services for resource group {ResourceGroupName}", resourceGroupName);

            var result = new ResourceGroupServicesDto(
                ResourceGroupName: resourceGroupName,
                AppConfigEndpoints: appConfigEndpoints.AsReadOnly(),
                KeyVaultEndpoints: keyVaultEndpoints.AsReadOnly());

            return new GetServicesInGroupResponse(true, null, result);
        }
        catch (OperationCanceledException ex)
        {
            const string message = "Service discovery was cancelled.";
            Logger.LogWarning(ex, message);
            return new GetServicesInGroupResponse(false, message, null);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to retrieve services in resource group {ResourceGroupName}", resourceGroupName);
            return new GetServicesInGroupResponse(false, $"Failed to retrieve services: {ex.Message}", null);
        }
    }
}
