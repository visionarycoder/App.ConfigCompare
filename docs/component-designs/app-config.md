# Component Design — AppConfig

## Responsibility

Read, create, update, and delete key-value entries in an Azure App Configuration store. Support auto-refresh on a user-defined interval and expose the last-refresh timestamp in local time.

---

## Volatility

This component changes when:
- The Azure App Configuration SDK API surface changes
- The entry model (labels, content type, tags, ETag) changes
- Refresh / subscription behavior changes

---

## Contract: `ConfigCompare.AppConfig.Contract`

### Interfaces

```csharp
public interface IAppConfigService
{
    Task<GetEntriesResponse> GetEntriesAsync(string endpoint, CancellationToken cancellationToken = default);
    Task<GetEntryResponse> GetEntryAsync(string endpoint, string key, string? label, CancellationToken cancellationToken = default);
    Task<CreateEntryResponse> CreateEntryAsync(string endpoint, AppConfigEntryDto entry, CancellationToken cancellationToken = default);
    Task<UpdateEntryResponse> UpdateEntryAsync(string endpoint, AppConfigEntryDto entry, CancellationToken cancellationToken = default);
    Task<DeleteEntryResponse> DeleteEntryAsync(string endpoint, string key, string? label, CancellationToken cancellationToken = default);
}
```

### DTOs

```csharp
public record AppConfigEntryDto(
    string Key,
    string? Value,
    string? Label,
    string? ContentType,
    IReadOnlyDictionary<string, string>? Tags,
    DateTimeOffset? LastModified);

public record GetEntriesResponse(bool IsSuccess, string? ErrorMessage, IReadOnlyList<AppConfigEntryDto>? Data, DateTimeOffset? RefreshedAt);
public record GetEntryResponse(bool IsSuccess, string? ErrorMessage, AppConfigEntryDto? Data);
public record CreateEntryResponse(bool IsSuccess, string? ErrorMessage, AppConfigEntryDto? Data);
public record UpdateEntryResponse(bool IsSuccess, string? ErrorMessage, AppConfigEntryDto? Data);
public record DeleteEntryResponse(bool IsSuccess, string? ErrorMessage);
```

---

## Service: `ConfigCompare.AppConfig.Service`

### Implementation Notes

- Uses `Azure.Data.AppConfiguration.ConfigurationClient`
- One `ConfigurationClient` instance per endpoint — managed via a keyed dictionary or factory pattern
- `GetEntriesAsync`: iterates `client.GetConfigurationSettingsAsync(new SettingSelector())`, maps to `AppConfigEntryDto`, sets `RefreshedAt = DateTimeOffset.Now`
- `CreateEntryAsync` / `UpdateEntryAsync`: calls `client.SetConfigurationSettingAsync`; uses ETags for optimistic concurrency where available
- `DeleteEntryAsync`: calls `client.DeleteConfigurationSettingAsync`
- Endpoint-to-client mapping is scoped per service instance; clients are created lazily and cached
- `RefreshedAt` is always local time (`DateTimeOffset.Now`, not UTC)

### Auto-Refresh

Auto-refresh is **not** owned by this service. It is driven by a timer in the ViewModel layer. The ViewModel calls `GetEntriesAsync` on each tick. This keeps the service stateless and easily testable.

### DI Registration

```csharp
public static IServiceCollection AddAppConfigServices(this IServiceCollection services)
{
    services.AddSingleton<IAppConfigClientFactory, AppConfigClientFactory>();
    services.AddScoped<IAppConfigService, AppConfigService>();
    return services;
}
```

`AppConfigClientFactory` creates or retrieves a cached `ConfigurationClient` for a given endpoint, using the injected `TokenCredential`.

---

## Dependencies

| Dependency | NuGet Package |
|---|---|
| App Configuration SDK | `Azure.Data.AppConfiguration` |
| Azure credential | `Azure.Identity` (transitive) |
| Logging | `Microsoft.Extensions.Logging.Abstractions` |

---

## Test Surface

### Unit Tests

| Scenario | Expected |
|---|---|
| `GetEntriesAsync` — store with 3 entries | Returns list of 3 `AppConfigEntryDto`, `IsSuccess=true`, `RefreshedAt` set |
| `GetEntriesAsync` — SDK throws `RequestFailedException` | `IsSuccess=false`, error message set |
| `GetEntriesAsync` — cancelled | `IsSuccess=false`, cancellation described |
| `GetEntryAsync` — key exists | `IsSuccess=true`, single entry returned |
| `GetEntryAsync` — key not found (404) | `IsSuccess=false`, descriptive message |
| `CreateEntryAsync` — valid entry | `IsSuccess=true`, created entry echoed |
| `CreateEntryAsync` — SDK throws | `IsSuccess=false` |
| `UpdateEntryAsync` — existing key | `IsSuccess=true`, updated entry echoed |
| `DeleteEntryAsync` — existing key | `IsSuccess=true` |
| `DeleteEntryAsync` — non-existent key | `IsSuccess=false`, descriptive message |

---

## Use Cases Served

- UC-5: Load and display all configurations from an Azure AppConfig
- UC-6: Manual or auto-refresh; show last refresh timestamp in local time
- UC-9: Create, Edit, Delete entries in Azure App Configuration
