# Component Design — AzureIdentity

## Responsibility

Manage the complete Azure authentication lifecycle: obtain and refresh access tokens using SSO (`DefaultAzureCredential`), expose the current signed-in user's identity, and provide token-acquisition helpers used by all other Azure-SDK-backed components.

---

## Volatility

This component changes when:
- The Azure authentication flow changes (new credential types, MSAL updates)
- Token scopes or audience URIs change
- The user identity claim set changes (e.g., tenant, UPN, display name)

It is **isolated** from all business logic, UI, and persistence concerns.

---

## Contract: `ConfigCompare.AzureIdentity.Contract`

### Interfaces

```csharp
public interface IAuthService
{
    Task<GetTokenResponse> GetTokenAsync(string[] scopes, CancellationToken cancellationToken = default);
    Task<GetCurrentUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<IsAuthenticatedResponse> IsAuthenticatedAsync(CancellationToken cancellationToken = default);
}
```

### DTOs

```csharp
public record TokenInfo(string AccessToken, DateTimeOffset ExpiresOn, string[] Scopes);

public record UserInfo(string DisplayName, string UserPrincipalName, string TenantId, string ObjectId);

public record GetTokenResponse(bool IsSuccess, string? ErrorMessage, TokenInfo? Data);
public record GetCurrentUserResponse(bool IsSuccess, string? ErrorMessage, UserInfo? Data);
public record IsAuthenticatedResponse(bool IsSuccess, string? ErrorMessage, bool IsAuthenticated);
```

---

## Service: `ConfigCompare.AzureIdentity.Service`

### Implementation Notes

- Uses `Azure.Identity.DefaultAzureCredential` (chains: VisualStudio → AzureCLI → InteractiveBrowser)
- `GetTokenAsync` calls `DefaultAzureCredential.GetTokenAsync(new TokenRequestContext(scopes))`
- `GetCurrentUserAsync` decodes the JWT from `GetTokenAsync(["https://graph.microsoft.com/.default"])` to extract `upn`, `name`, `oid`, `tid` claims — or calls `https://graph.microsoft.com/v1.0/me` via `HttpClient` if a full profile is needed
- Token caching is handled automatically by the Azure SDK
- All methods: `async`, `CancellationToken`, `try/catch`, structured logging, return response object

### DI Registration

```csharp
// Extension method in AzureIdentity.Service
public static IServiceCollection AddAzureIdentityServices(this IServiceCollection services)
{
    services.AddSingleton<TokenCredential, DefaultAzureCredential>();
    services.AddSingleton<IAuthService, AuthService>();
    return services;
}
```

---

## Dependencies

| Dependency | NuGet Package |
|---|---|
| Azure credential | `Azure.Identity` |
| HTTP client (Graph /me) | `Microsoft.Extensions.Http` |
| Logging | `Microsoft.Extensions.Logging.Abstractions` |

---

## Test Surface

### Unit Tests (`ConfigCompare.AzureIdentity.Service.Tests.Unit`)

| Scenario | Expected |
|---|---|
| `GetTokenAsync` — credential succeeds | `IsSuccess=true`, `TokenInfo` populated |
| `GetTokenAsync` — credential throws `AuthenticationFailedException` | `IsSuccess=false`, error message populated, no throw |
| `GetTokenAsync` — cancellation token cancelled | `IsSuccess=false`, error describes cancellation |
| `GetCurrentUserAsync` — valid token returned | `IsSuccess=true`, `UserInfo` fields populated |
| `GetCurrentUserAsync` — token acquisition fails | `IsSuccess=false`, error propagated |
| `IsAuthenticatedAsync` — token available | `IsAuthenticated=true` |
| `IsAuthenticatedAsync` — no token | `IsAuthenticated=false` |

---

## Use Cases Served

- UC-1: Connect to Azure using SSO
- UC-2 (indirect): Provide current user name for footer display
- UC-11: Supply identity context for PIM activation
