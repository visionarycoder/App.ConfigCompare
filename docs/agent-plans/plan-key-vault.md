# Agent Plan — KeyVault Component

## Goal

Implement the `KeyVault` component: contract library and service library for reading, creating, updating, and deleting Azure Key Vault secrets, plus full unit tests. Secret values must never be logged or persisted.

---

## Pre-conditions

- `AzureIdentity` component complete (`TokenCredential` in DI)
- `/src/ConfigCompare.KeyVault.Contract/` and `/src/ConfigCompare.KeyVault.Service/` project files exist
- Agent has read access to `docs/component-designs/key-vault.md`

---

## Step 1 — Add NuGet References

**Contract project**:
- `Microsoft.Extensions.Logging.Abstractions`

**Service project**:
- `Azure.Security.KeyVault.Secrets`
- `Microsoft.Extensions.Logging.Abstractions`
- Project reference → `ConfigCompare.KeyVault.Contract`
- Project reference → `ConfigCompare.AzureIdentity.Contract`

**Test project** (`ConfigCompare.KeyVault.Service.Tests.Unit`):
- `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions`, `coverlet.collector`
- Project reference → `ConfigCompare.KeyVault.Service`

---

## Step 2 — Implement Contract

Create in `ConfigCompare.KeyVault.Contract`:

1. `SecretDto.cs` — record: `Name`, `Value?`, `ContentType?`, `Tags?`, `CreatedOn?`, `UpdatedOn?`, `ExpiresOn?`, `IsEnabled`
2. `GetSecretsResponse.cs` — record: `IsSuccess`, `ErrorMessage?`, `Data?`, `RefreshedAt?`
3. `GetSecretResponse.cs`, `CreateSecretResponse.cs`, `UpdateSecretResponse.cs`, `DeleteSecretResponse.cs`
4. `IKeyVaultService.cs` — five methods per design spec

---

## Step 3 — Implement Service

Create in `ConfigCompare.KeyVault.Service`:

1. `IKeyVaultClientFactory.cs` + `KeyVaultClientFactory.cs`:
   - `GetClient(string vaultUri)` — creates and caches a `SecretClient` per vault URI using injected `TokenCredential`

2. `KeyVaultService.cs`:
   - Constructor: `(IKeyVaultClientFactory factory, ILogger<KeyVaultService> logger)`
   - `GetSecretsAsync`: calls `client.GetPropertiesOfSecretsAsync()` to get names/metadata, then calls `client.GetSecretAsync(name)` for each to retrieve values; maps to `SecretDto` list; sets `RefreshedAt`; **never logs `Value`**
   - `GetSecretAsync`: calls `client.GetSecretAsync(name)`, maps result; handles 404 as `IsSuccess=false`
   - `CreateSecretAsync` / `UpdateSecretAsync`: calls `client.SetSecretAsync(name, value)`, then `client.UpdateSecretPropertiesAsync` for metadata
   - `DeleteSecretAsync`: calls `client.StartDeleteSecretAsync(name)`

3. `ServiceCollectionExtensions.cs`:
   - `AddKeyVaultServices(this IServiceCollection services)`

---

## Step 4 — Unit Tests

Mock `IKeyVaultClientFactory`.

Implement all scenarios from `docs/component-designs/key-vault.md`.

Verify that no test asserts on logged output that would include a secret value (this would be a security violation in the test itself).

---

## Step 5 — Verify

```bash
dotnet build src/ConfigCompare.KeyVault.Contract
dotnet build src/ConfigCompare.KeyVault.Service
dotnet test tests/unit/ConfigCompare.KeyVault.Service.Tests.Unit --collect:"XPlat Code Coverage"
```

---

## Acceptance Criteria

- [ ] `IKeyVaultService` interface and all DTOs in Contract
- [ ] `KeyVaultService` implementation; secret values never logged
- [ ] DI extension registered
- [ ] All unit tests pass
- [ ] No compiler warnings
- [ ] Coverage ≥ 80%
