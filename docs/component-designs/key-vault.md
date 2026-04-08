# Component Design — KeyVault

## Responsibility

Read, create, update, and delete key-value secret pairs in an Azure Key Vault. Secrets are surfaced only in-memory and never written to disk.

---

## Volatility

This component changes when:
- The Azure Key Vault SDK API surface changes
- The secret metadata model changes (content type, tags, expiry)
- Access control or managed identity patterns change

---

## Contract: `ConfigCompare.KeyVault.Contract`

### Interfaces

```csharp
public interface IKeyVaultService
{
    Task<GetSecretsResponse> GetSecretsAsync(string vaultUri, CancellationToken cancellationToken = default);
    Task<GetSecretResponse> GetSecretAsync(string vaultUri, string name, CancellationToken cancellationToken = default);
    Task<CreateSecretResponse> CreateSecretAsync(string vaultUri, SecretDto secret, CancellationToken cancellationToken = default);
    Task<UpdateSecretResponse> UpdateSecretAsync(string vaultUri, SecretDto secret, CancellationToken cancellationToken = default);
    Task<DeleteSecretResponse> DeleteSecretAsync(string vaultUri, string name, CancellationToken cancellationToken = default);
}
```

### DTOs

```csharp
public record SecretDto(
    string Name,
    string? Value,
    string? ContentType,
    IReadOnlyDictionary<string, string>? Tags,
    DateTimeOffset? CreatedOn,
    DateTimeOffset? UpdatedOn,
    DateTimeOffset? ExpiresOn,
    bool IsEnabled);

public record GetSecretsResponse(bool IsSuccess, string? ErrorMessage, IReadOnlyList<SecretDto>? Data, DateTimeOffset? RefreshedAt);
public record GetSecretResponse(bool IsSuccess, string? ErrorMessage, SecretDto? Data);
public record CreateSecretResponse(bool IsSuccess, string? ErrorMessage, SecretDto? Data);
public record UpdateSecretResponse(bool IsSuccess, string? ErrorMessage, SecretDto? Data);
public record DeleteSecretResponse(bool IsSuccess, string? ErrorMessage);
```

---

## Service: `ConfigCompare.KeyVault.Service`

### Implementation Notes

- Uses `Azure.Security.KeyVault.Secrets.SecretClient`
- One `SecretClient` per vault URI — managed via a factory/cache (same pattern as AppConfig)
- `GetSecretsAsync`:
  - Calls `client.GetPropertiesOfSecretsAsync()` to iterate secret names and metadata (no values)
  - Then calls `client.GetSecretAsync(name)` for each to retrieve values
  - **Important**: Listing secrets does not return values; individual fetch per secret is required
  - `RefreshedAt = DateTimeOffset.Now`
- `CreateSecretAsync`: `client.SetSecretAsync(name, value)` then updates properties for `ContentType`, `Tags`, `ExpiresOn`
- `UpdateSecretAsync`: same path as Create (`SetSecret` is upsert)
- `DeleteSecretAsync`: `client.StartDeleteSecretAsync(name)` — starts soft delete; does not block for purge
- Secret values are **never** logged; only secret names and metadata are included in log output

### Security Note

Secret values must never be persisted to disk (SQLite, settings files, or logs). The ViewModel layer must treat the in-memory `SecretDto.Value` as ephemeral and clear it when the view is unloaded.

### DI Registration

```csharp
public static IServiceCollection AddKeyVaultServices(this IServiceCollection services)
{
    services.AddSingleton<IKeyVaultClientFactory, KeyVaultClientFactory>();
    services.AddScoped<IKeyVaultService, KeyVaultService>();
    return services;
}
```

---

## Dependencies

| Dependency | NuGet Package |
|---|---|
| Key Vault secrets SDK | `Azure.Security.KeyVault.Secrets` |
| Azure credential | `Azure.Identity` (transitive) |
| Logging | `Microsoft.Extensions.Logging.Abstractions` |

---

## Test Surface

### Unit Tests

| Scenario | Expected |
|---|---|
| `GetSecretsAsync` — vault with 2 secrets | Returns list of 2 `SecretDto`, values populated |
| `GetSecretsAsync` — SDK throws `RequestFailedException` | `IsSuccess=false`, error message set |
| `GetSecretsAsync` — cancelled | `IsSuccess=false`, cancellation described |
| `GetSecretAsync` — name exists | `IsSuccess=true`, value populated |
| `GetSecretAsync` — name not found | `IsSuccess=false`, descriptive message |
| `CreateSecretAsync` — valid secret | `IsSuccess=true`, created secret echoed |
| `UpdateSecretAsync` — existing name | `IsSuccess=true`, updated secret echoed |
| `DeleteSecretAsync` — existing name | `IsSuccess=true` |
| `DeleteSecretAsync` — SDK throws | `IsSuccess=false` |

---

## Use Cases Served

- UC-10: Read, Create, Update, and Delete Key Vault secret pairs
- UC-8 (indirect): Secret names used in cross-resource-group comparison
