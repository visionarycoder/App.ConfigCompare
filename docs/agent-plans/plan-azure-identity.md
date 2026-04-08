# Agent Plan — AzureIdentity Component

## Goal

Implement the `AzureIdentity` component: the contract library (interfaces + DTOs) and the service library (implementation), along with full unit tests.

---

## Pre-conditions

- Repository cloned; `/src/ConfigCompare.AzureIdentity.Contract/` and `/src/ConfigCompare.AzureIdentity.Service/` project files already exist
- .NET 10 SDK installed
- Agent has read access to `docs/component-designs/azure-identity.md` for the full design spec

---

## Step 1 — Add NuGet References

**Contract project** — no Azure SDK references needed; only:
- `Microsoft.Extensions.Logging.Abstractions`

**Service project**:
- `Azure.Identity`
- `Microsoft.Extensions.Http`
- `Microsoft.Extensions.Logging.Abstractions`
- Project reference → `ConfigCompare.AzureIdentity.Contract`

**Test project** (`ConfigCompare.AzureIdentity.Service.Tests.Unit`):
- `xunit`
- `xunit.runner.visualstudio`
- `Moq`
- `FluentAssertions`
- `coverlet.collector`
- Project reference → `ConfigCompare.AzureIdentity.Service`

---

## Step 2 — Implement Contract

Create in `ConfigCompare.AzureIdentity.Contract`:

1. `TokenInfo.cs` — record with `AccessToken`, `ExpiresOn`, `Scopes`
2. `UserInfo.cs` — record with `DisplayName`, `UserPrincipalName`, `TenantId`, `ObjectId`
3. `GetTokenResponse.cs` — record with `IsSuccess`, `ErrorMessage`, `Data` (TokenInfo?)
4. `GetCurrentUserResponse.cs` — record with `IsSuccess`, `ErrorMessage`, `Data` (UserInfo?)
5. `IsAuthenticatedResponse.cs` — record with `IsSuccess`, `ErrorMessage`, `IsAuthenticated`
6. `IAuthService.cs` — interface with three methods per design spec

---

## Step 3 — Implement Service

Create in `ConfigCompare.AzureIdentity.Service`:

1. `AuthService.cs`:
   - Constructor: `(TokenCredential credential, HttpClient httpClient, ILogger<AuthService> logger)`
   - `GetTokenAsync`: calls `credential.GetTokenAsync(new TokenRequestContext(scopes), cancellationToken)`, wraps in try/catch, returns `GetTokenResponse`
   - `GetCurrentUserAsync`: acquires token for `https://graph.microsoft.com/.default`, calls `GET https://graph.microsoft.com/v1.0/me` via `httpClient`, deserialises `{ displayName, userPrincipalName, id, tenantId }`, returns `GetCurrentUserResponse`
   - `IsAuthenticatedAsync`: attempts `GetTokenAsync(["https://management.azure.com/.default"])`, returns `IsAuthenticated = IsSuccess`
   - All methods: `async`, honor `cancellationToken`, `try/catch`, log errors

2. `ServiceCollectionExtensions.cs`:
   - `AddAzureIdentityServices(this IServiceCollection services)` — registers `DefaultAzureCredential` as `TokenCredential` singleton, `HttpClient` for Graph, and `AuthService` as `IAuthService` singleton

---

## Step 4 — Unit Tests

Create in `ConfigCompare.AzureIdentity.Service.Tests.Unit`:

Implement all scenarios listed in `docs/component-designs/azure-identity.md` (Test Surface section).

Use Moq to mock `TokenCredential` and `HttpClient` (via `HttpMessageHandler` mock).

---

## Step 5 — Verify

```bash
dotnet build src/ConfigCompare.AzureIdentity.Contract
dotnet build src/ConfigCompare.AzureIdentity.Service
dotnet test tests/unit/ConfigCompare.AzureIdentity.Service.Tests.Unit --collect:"XPlat Code Coverage"
```

All tests must pass. Coverage of `AuthService` must be ≥ 80%.

---

## Acceptance Criteria

- [ ] `IAuthService` interface and all DTOs in Contract project
- [ ] `AuthService` implementation in Service project
- [ ] `AddAzureIdentityServices` DI extension registered
- [ ] All unit tests pass
- [ ] No compiler warnings
- [ ] Code coverage ≥ 80% on `AuthService`
