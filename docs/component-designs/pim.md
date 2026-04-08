# Component Design — Pim

## Responsibility

Allow the current user to discover their eligible Azure PIM (Privileged Identity Management) role assignments and activate selected roles on demand from within the application. This is required when the user needs elevated permissions (e.g., Key Vault Officer) that are not permanently assigned.

---

## Volatility

This component changes when:
- The Microsoft Graph API for PIM changes (currently `roleManagement/directory/roleEligibilityScheduleRequests`)
- The PIM activation workflow changes (justification, duration, MFA requirements)
- The scope of role management changes (resource-group-scoped vs. subscription-scoped)

---

## Contract: `ConfigCompare.Pim.Contract`

### Interfaces

```csharp
public interface IPimService
{
    Task<GetEligibleRolesResponse> GetEligibleRolesAsync(CancellationToken cancellationToken = default);
    Task<ActivateRoleResponse> ActivateRoleAsync(ActivateRoleRequest request, CancellationToken cancellationToken = default);
    Task<GetActiveRolesResponse> GetActiveRolesAsync(CancellationToken cancellationToken = default);
}
```

### DTOs

```csharp
public record PimRoleDto(
    string RoleAssignmentId,
    string RoleDefinitionId,
    string RoleDefinitionName,
    string DirectoryScopeId,
    string PrincipalId,
    DateTimeOffset? ScheduleStartDateTime,
    DateTimeOffset? ScheduleEndDateTime,
    PimRoleStatus Status);

public enum PimRoleStatus { Eligible, Active, Expired }

public record ActivateRoleRequest(
    string RoleEligibilityScheduleId,
    string Justification,
    int DurationInHours);

public record GetEligibleRolesResponse(bool IsSuccess, string? ErrorMessage, IReadOnlyList<PimRoleDto>? Data);
public record ActivateRoleResponse(bool IsSuccess, string? ErrorMessage, string? ActivationRequestId);
public record GetActiveRolesResponse(bool IsSuccess, string? ErrorMessage, IReadOnlyList<PimRoleDto>? Data);
```

---

## Service: `ConfigCompare.Pim.Service`

### Implementation Notes

- Uses `Microsoft.Graph.GraphServiceClient` with the injected `TokenCredential`
- Required Graph API scope: `RoleManagement.ReadWrite.Directory`
- `GetEligibleRolesAsync`:
  - Calls `graphClient.RoleManagement.Directory.RoleEligibilitySchedules.GetAsync()` filtering by `principalId` of the current user
  - Maps to `PimRoleDto` with `Status = Eligible`
- `ActivateRoleAsync`:
  - Posts a `UnifiedRoleAssignmentScheduleRequest` via `graphClient.RoleManagement.Directory.RoleAssignmentScheduleRequests.PostAsync()`
  - `action = "selfActivate"`, sets `scheduleInfo.startDateTime`, `scheduleInfo.expiration.duration` from `DurationInHours`
  - Returns the request ID for tracking
- `GetActiveRolesAsync`:
  - Calls `graphClient.RoleManagement.Directory.RoleAssignmentSchedules.GetAsync()` for the current user
  - Filters to assignments with active schedule info
- Token must include the `RoleManagement.ReadWrite.Directory` scope; requested via `IAuthService.GetTokenAsync`

### DI Registration

```csharp
public static IServiceCollection AddPimServices(this IServiceCollection services)
{
    services.AddSingleton<GraphServiceClient>(sp =>
    {
        var credential = sp.GetRequiredService<TokenCredential>();
        return new GraphServiceClient(credential, ["RoleManagement.ReadWrite.Directory"]);
    });
    services.AddScoped<IPimService, PimService>();
    return services;
}
```

---

## Dependencies

| Dependency | NuGet Package |
|---|---|
| Microsoft Graph SDK | `Microsoft.Graph` |
| Azure credential | `Azure.Identity` (transitive) |
| Logging | `Microsoft.Extensions.Logging.Abstractions` |

---

## Test Surface

### Unit Tests

| Scenario | Expected |
|---|---|
| `GetEligibleRolesAsync` — user has 2 eligible roles | Returns 2 `PimRoleDto` with `Status=Eligible` |
| `GetEligibleRolesAsync` — Graph throws | `IsSuccess=false`, error message set |
| `GetEligibleRolesAsync` — cancelled | `IsSuccess=false`, cancellation described |
| `ActivateRoleAsync` — valid request | `IsSuccess=true`, `ActivationRequestId` populated |
| `ActivateRoleAsync` — Graph returns 400 (bad justification) | `IsSuccess=false`, descriptive error |
| `ActivateRoleAsync` — already active role | `IsSuccess=false`, descriptive error |
| `GetActiveRolesAsync` — no active roles | `IsSuccess=true`, empty list |
| `GetActiveRolesAsync` — 1 active role | Returns 1 `PimRoleDto` |

---

## Use Cases Served

- UC-11: Interact with PIM in Azure to activate permissions for the current user
