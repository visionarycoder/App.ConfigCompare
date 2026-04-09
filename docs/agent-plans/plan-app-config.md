# Agent Plan — AppConfig Component

## Goal

Implement the `AppConfig` component: contract library and service library for reading, creating, updating, and deleting Azure App Configuration entries, plus full unit tests.

---

## Pre-conditions

- `AzureIdentity` component complete (`TokenCredential` in DI)
- `/src/ConfigCompare.AppConfig.Contract/` and `/src/ConfigCompare.AppConfig.Service/` project files exist
- Agent has read access to `docs/component-designs/app-config.md`

---

## Step 1 — Add NuGet References

**Contract project**:
- `Microsoft.Extensions.Logging.Abstractions`

**Service project**:
- `Azure.Data.AppConfiguration`
- `Microsoft.Extensions.Logging.Abstractions`
- Project reference → `ConfigCompare.AppConfig.Contract`
- Project reference → `ConfigCompare.AzureIdentity.Contract`

**Test project** (`ConfigCompare.AppConfig.Service.Tests.Unit`):
- `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions`, `coverlet.collector`
- Project reference → `ConfigCompare.AppConfig.Service`

---

## Step 2 — Implement Contract

Create in `ConfigCompare.AppConfig.Contract`:

1. `AppConfigEntryDto.cs` — record: `Key`, `Value?`, `Label?`, `ContentType?`, `Tags?`, `LastModified?`
2. `GetEntriesResponse.cs` — record: `IsSuccess`, `ErrorMessage?`, `Data?`, `RefreshedAt?`
3. `GetEntryResponse.cs`, `CreateEntryResponse.cs`, `UpdateEntryResponse.cs`, `DeleteEntryResponse.cs`
4. `IAppConfigService.cs` — five methods per design spec

---

## Step 3 — Implement Service

Create in `ConfigCompare.AppConfig.Service`:

1. `IAppConfigClientFactory.cs` + `AppConfigClientFactory.cs`:
   - `GetClient(string endpoint)` — creates and caches a `ConfigurationClient` per endpoint using the injected `TokenCredential`

2. `AppConfigService.cs`:
   - Constructor: `(IAppConfigClientFactory factory, ILogger<AppConfigService> logger)`
   - `GetEntriesAsync`: calls `client.GetConfigurationSettingsAsync(new SettingSelector())`, maps each `ConfigurationSetting` to `AppConfigEntryDto`, sets `RefreshedAt = DateTimeOffset.Now`; `try/catch`
   - `GetEntryAsync`: calls `client.GetConfigurationSettingAsync(key, label)`; maps result; handles 404 as `IsSuccess=false`
   - `CreateEntryAsync` / `UpdateEntryAsync`: calls `client.SetConfigurationSettingAsync`
   - `DeleteEntryAsync`: calls `client.DeleteConfigurationSettingAsync`

3. `ServiceCollectionExtensions.cs`:
   - `AddAppConfigServices(this IServiceCollection services)`

---

## Step 4 — Unit Tests

Mock `IAppConfigClientFactory` (returning a mocked `ConfigurationClient` or equivalent wrapper).

Implement all scenarios from `docs/component-designs/app-config.md`.

---

## Step 5 — Verify

```bash
dotnet build src/ConfigCompare.AppConfig.Contract
dotnet build src/ConfigCompare.AppConfig.Service
dotnet test tests/unit/ConfigCompare.AppConfig.Service.Tests.Unit --collect:"XPlat Code Coverage"
```

---

## Acceptance Criteria

- [ ] `IAppConfigService` interface and all DTOs in Contract
- [ ] `AppConfigService` implementation with client factory
- [ ] DI extension registered
- [ ] All unit tests pass
- [ ] No compiler warnings
- [ ] Coverage ≥ 80%
