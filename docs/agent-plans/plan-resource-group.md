# Agent Plan — ResourceGroup Component

## Goal

Implement the `ResourceGroup` component: contract library and service library, plus unit tests. The service lists all Azure resource groups accessible to the current user and resolves the AppConfig / Key Vault endpoints within a selected group.

---

## Pre-conditions

- `AzureIdentity` component is complete (`IAuthService`, `TokenCredential` registered in DI)
- `/src/ConfigCompare.ResourceGroup.Contract/` and `/src/ConfigCompare.ResourceGroup.Service/` project files exist
- Agent has read access to `docs/component-designs/resource-group.md`

---

## Step 1 — Add NuGet References

**Contract project**:
- `Microsoft.Extensions.Logging.Abstractions`

**Service project**:
- `Azure.ResourceManager`
- `Azure.ResourceManager.AppConfiguration`
- `Azure.ResourceManager.KeyVault`
- `Microsoft.Extensions.Logging.Abstractions`
- Project reference → `ConfigCompare.ResourceGroup.Contract`
- Project reference → `ConfigCompare.AzureIdentity.Contract`

**Test project** (no separate test project scaffolded; add unit test project `ConfigCompare.ResourceGroup.Service.Tests.Unit` if not present):
- `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions`, `coverlet.collector`
- Project reference → `ConfigCompare.ResourceGroup.Service`

---

## Step 2 — Implement Contract

Create in `ConfigCompare.ResourceGroup.Contract`:

1. `ResourceGroupDto.cs` — record: `Name`, `SubscriptionId`, `Location`, `DisplayName?`
2. `AppConfigEndpointDto.cs` — record: `Name`, `Endpoint`
3. `KeyVaultEndpointDto.cs` — record: `Name`, `VaultUri`
4. `ResourceGroupServicesDto.cs` — record: `ResourceGroupName`, `AppConfigEndpoints`, `KeyVaultEndpoints`
5. `GetResourceGroupsResponse.cs`, `GetServicesInGroupResponse.cs`
6. `IResourceGroupService.cs` — two methods per design spec

---

## Step 3 — Implement Service

Create in `ConfigCompare.ResourceGroup.Service`:

1. `ResourceGroupService.cs`:
   - Constructor: `(ArmClient armClient, ILogger<ResourceGroupService> logger)`
   - `GetResourceGroupsAsync`: iterates `armClient.GetSubscriptions()`, then `subscription.GetResourceGroups()` for each; maps to `ResourceGroupDto`; `try/catch`; returns `GetResourceGroupsResponse`
   - `GetServicesInGroupAsync`: resolves the `ResourceGroupResource` from `armClient`, then calls `GetAppConfigurationStores()` and `GetKeyVaults()`; maps endpoints; returns `GetServicesInGroupResponse`

2. `ServiceCollectionExtensions.cs`:
   - `AddResourceGroupServices(this IServiceCollection services)` — registers `ArmClient` singleton (using injected `TokenCredential`), `IResourceGroupService` scoped

---

## Step 4 — Unit Tests

Mock `ArmClient` and its pagination results using Moq.

Implement all scenarios in `docs/component-designs/resource-group.md` (Test Surface section).

---

## Step 5 — Verify

```bash
dotnet build src/ConfigCompare.ResourceGroup.Contract
dotnet build src/ConfigCompare.ResourceGroup.Service
dotnet test tests/unit/ConfigCompare.ResourceGroup.Service.Tests.Unit --collect:"XPlat Code Coverage"
```

---

## Acceptance Criteria

- [ ] `IResourceGroupService` interface and DTOs in Contract
- [ ] `ResourceGroupService` implementation with ARM client
- [ ] DI extension registered
- [ ] All unit tests pass
- [ ] No compiler warnings
- [ ] Coverage ≥ 80%
