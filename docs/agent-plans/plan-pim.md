# Agent Plan — Pim Component

## Goal

Implement the `Pim` component: contract library and Microsoft Graph-backed service library for listing and activating PIM role assignments, plus unit tests.

---

## Pre-conditions

- `AzureIdentity` component complete (`TokenCredential` in DI)
- `/src/ConfigCompare.Pim.Contract/` and `/src/ConfigCompare.Pim.Service/` project files exist
- Agent has read access to `docs/component-designs/pim.md`

---

## Step 1 — Add NuGet References

**Contract project**:
- `Microsoft.Extensions.Logging.Abstractions`

**Service project**:
- `Microsoft.Graph`
- `Azure.Identity` (for credential adapter)
- `Microsoft.Extensions.Logging.Abstractions`
- Project reference → `ConfigCompare.Pim.Contract`
- Project reference → `ConfigCompare.AzureIdentity.Contract`

**Test project** (no separate unit test project scaffolded for Pim; create `ConfigCompare.Pim.Service.Tests.Unit`):
- `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions`, `coverlet.collector`
- Project reference → `ConfigCompare.Pim.Service`

---

## Step 2 — Implement Contract

Create in `ConfigCompare.Pim.Contract`:

1. `PimRoleStatus.cs` — enum: `Eligible`, `Active`, `Expired`
2. `PimRoleDto.cs` — record: `RoleAssignmentId`, `RoleDefinitionId`, `RoleDefinitionName`, `DirectoryScopeId`, `PrincipalId`, `ScheduleStartDateTime?`, `ScheduleEndDateTime?`, `Status`
3. `ActivateRoleRequest.cs` — record: `RoleEligibilityScheduleId`, `Justification`, `DurationInHours`
4. `GetEligibleRolesResponse.cs`, `ActivateRoleResponse.cs`, `GetActiveRolesResponse.cs`
5. `IPimService.cs` — three methods per design spec

---

## Step 3 — Implement Service

Create in `ConfigCompare.Pim.Service`:

1. `PimService.cs`:
   - Constructor: `(GraphServiceClient graphClient, IAuthService authService, ILogger<PimService> logger)`
   - `GetEligibleRolesAsync`:
     - Gets `principalId` from `authService.GetCurrentUserAsync()` → `UserInfo.ObjectId`
     - Calls `graphClient.RoleManagement.Directory.RoleEligibilitySchedules.GetAsync(config => config.QueryParameters.Filter = $"principalId eq '{principalId}'")`
     - Maps to `PimRoleDto` list with `Status = PimRoleStatus.Eligible`
   - `ActivateRoleAsync`:
     - Builds `UnifiedRoleAssignmentScheduleRequest` with `action = "selfActivate"`, `roleEligibilityScheduleId`, `justification`, schedule duration
     - Posts via `graphClient.RoleManagement.Directory.RoleAssignmentScheduleRequests.PostAsync(request)`
     - Returns `ActivationRequestId = response.Id`
   - `GetActiveRolesAsync`:
     - Calls `graphClient.RoleManagement.Directory.RoleAssignmentSchedules.GetAsync()` filtered by `principalId`
     - Maps to `PimRoleDto` list with `Status = PimRoleStatus.Active`

2. `ServiceCollectionExtensions.cs`:
   - `AddPimServices(this IServiceCollection services)` — registers `GraphServiceClient` singleton with `RoleManagement.ReadWrite.Directory` scope, registers `IPimService`

---

## Step 4 — Unit Tests

Mock `GraphServiceClient` (or its underlying `IRequestAdapter`) and `IAuthService`.

Implement all scenarios from `docs/component-designs/pim.md` (Test Surface section).

---

## Step 5 — Verify

```bash
dotnet build src/ConfigCompare.Pim.Contract
dotnet build src/ConfigCompare.Pim.Service
dotnet test tests/unit/ConfigCompare.Pim.Service.Tests.Unit --collect:"XPlat Code Coverage"
```

---

## Acceptance Criteria

- [ ] `IPimService`, `PimRoleDto`, `ActivateRoleRequest`, `PimRoleStatus`, response records in Contract
- [ ] `PimService` implementation using MS Graph
- [ ] DI extension registered
- [ ] All unit tests pass
- [ ] No compiler warnings
- [ ] Coverage ≥ 80%
