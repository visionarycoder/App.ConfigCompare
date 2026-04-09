# Component Design — ResourceGroup

## Responsibility

Enumerate Azure resource groups visible to the currently authenticated user, and resolve the Azure App Configuration and Key Vault endpoints present within a selected resource group.

---

## Volatility

This component changes when:
- The Azure Resource Manager (ARM) API surface changes
- The filtering logic for surfacing relevant services (AppConfig, KeyVault) within a resource group changes
- Resource group metadata shape changes

---

## Contract: `ConfigCompare.ResourceGroup.Contract`

### Interfaces

```csharp
public interface IResourceGroupService
{
    Task<GetResourceGroupsResponse> GetResourceGroupsAsync(CancellationToken cancellationToken = default);
    Task<GetServicesInGroupResponse> GetServicesInGroupAsync(string resourceGroupName, CancellationToken cancellationToken = default);
}
```

### DTOs

```csharp
public record ResourceGroupDto(
    string Name,
    string SubscriptionId,
    string Location,
    string? DisplayName);

public record ResourceGroupServicesDto(
    string ResourceGroupName,
    IReadOnlyList<AppConfigEndpointDto> AppConfigEndpoints,
    IReadOnlyList<KeyVaultEndpointDto> KeyVaultEndpoints);

public record AppConfigEndpointDto(string Name, string Endpoint);
public record KeyVaultEndpointDto(string Name, string VaultUri);

public record GetResourceGroupsResponse(bool IsSuccess, string? ErrorMessage, IReadOnlyList<ResourceGroupDto>? Data);
public record GetServicesInGroupResponse(bool IsSuccess, string? ErrorMessage, ResourceGroupServicesDto? Data);
```

---

## Service: `ConfigCompare.ResourceGroup.Service`

### Implementation Notes

- Uses `Azure.ResourceManager.ArmClient` with a `TokenCredential` injected from `IAuthService`
- `GetResourceGroupsAsync`:
  - Calls `armClient.GetSubscriptions()` to iterate subscriptions accessible to the current credential
  - For each subscription, calls `subscription.GetResourceGroups()` to list groups
  - Returns a flat list of `ResourceGroupDto` records across all subscriptions
- `GetServicesInGroupAsync`:
  - Queries `resourceGroup.GetAppConfigurationStores()` for AppConfig endpoints
  - Queries `resourceGroup.GetKeyVaults()` for Key Vault URIs
  - Maps results to `ResourceGroupServicesDto`
- Pagination handled via `AsyncPageable<T>` — iterate fully before returning
- Token credential is obtained at construction via injected `TokenCredential` (registered as `DefaultAzureCredential`)

### DI Registration

```csharp
public static IServiceCollection AddResourceGroupServices(this IServiceCollection services)
{
    services.AddSingleton<ArmClient>(sp =>
    {
        var credential = sp.GetRequiredService<TokenCredential>();
        return new ArmClient(credential);
    });
    services.AddScoped<IResourceGroupService, ResourceGroupService>();
    return services;
}
```

---

## Dependencies

| Dependency | NuGet Package |
|---|---|
| ARM client | `Azure.ResourceManager` |
| App Configuration management | `Azure.ResourceManager.AppConfiguration` |
| Key Vault management | `Azure.ResourceManager.KeyVault` |
| Azure credential | `Azure.Identity` (transitive via AzureIdentity component) |
| Logging | `Microsoft.Extensions.Logging.Abstractions` |

---

## Test Surface

### Unit Tests

| Scenario | Expected |
|---|---|
| `GetResourceGroupsAsync` — two subscriptions, multiple groups | All groups returned in flat list |
| `GetResourceGroupsAsync` — ARM throws | `IsSuccess=false`, error message set |
| `GetResourceGroupsAsync` — cancelled | `IsSuccess=false`, cancellation described |
| `GetServicesInGroupAsync` — group with 2 AppConfig + 1 KeyVault | Both endpoint lists populated correctly |
| `GetServicesInGroupAsync` — group with no AppConfig | `AppConfigEndpoints` empty, `IsSuccess=true` |
| `GetServicesInGroupAsync` — ARM throws | `IsSuccess=false` |

---

## Use Cases Served

- UC-1: Display resource groups the current user has access to
- UC-4: Allow the user to select a resource group and its AppConfig/KeyVault services
- UC-7: Enable selection of additional resource groups for comparison
