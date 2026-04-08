# Agent Plan — Settings Component

## Goal

Implement the `Settings` component: contract library and JSON-file-backed service library for persisting user preferences, plus unit tests.

---

## Pre-conditions

- `/src/ConfigCompare.Settings.Contract/` and `/src/ConfigCompare.Settings.Service/` project files exist
- Agent has read access to `docs/component-designs/settings.md`

---

## Step 1 — Add NuGet References

**Contract project**:
- `Microsoft.Extensions.Logging.Abstractions`

**Service project**:
- `Microsoft.Extensions.Logging.Abstractions`
- Project reference → `ConfigCompare.Settings.Contract`

**Test project** (`ConfigCompare.Settings.Service.Tests.Unit`):
- `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions`, `coverlet.collector`
- Project reference → `ConfigCompare.Settings.Service`

---

## Step 2 — Implement Contract

Create in `ConfigCompare.Settings.Contract`:

1. `AppTheme.cs` — enum: `Light`, `Dark`, `System`
2. `UserSettingsDto.cs` — record: `Theme`, `AutoRefreshIntervalSeconds`, `AutoRefreshEnabled`, `SavedResourceGroupNames`, `LastActiveSubscriptionId?`
3. `GetSettingsResponse.cs` — record: `IsSuccess`, `ErrorMessage?`, `Data?`
4. `SaveSettingsResponse.cs` — record: `IsSuccess`, `ErrorMessage?`
5. `ResetSettingsResponse.cs` — record: `IsSuccess`, `ErrorMessage?`
6. `ISettingsService.cs` — three methods per design spec

---

## Step 3 — Implement Service

Create in `ConfigCompare.Settings.Service`:

1. `SettingsService.cs`:
   - Constructor: `(string settingsFilePath, ILogger<SettingsService> logger)`
   - Static `DefaultSettings` property: `AppTheme.System`, 60s interval, refresh disabled, empty resource group list
   - `GetSettingsAsync`: if file does not exist, return defaults; read and deserialise; on `JsonException`, return `IsSuccess=false`
   - `SaveSettingsAsync`: validate `AutoRefreshIntervalSeconds` ∈ [10, 3600]; write to temp file at `settingsFilePath + ".tmp"`, then `File.Move(tmp, path, overwrite: true)` for atomic write
   - `ResetSettingsAsync`: `File.Delete(settingsFilePath)` if exists; return `IsSuccess=true`

2. `ServiceCollectionExtensions.cs`:
   - `AddSettingsServices(this IServiceCollection services, string settingsFilePath)`

---

## Step 4 — Unit Tests

Use a temp file path (e.g., `Path.GetTempFileName()`) per test, cleaned up in `IDisposable.Dispose()`.

Implement all scenarios from `docs/component-designs/settings.md` (Test Surface section).

---

## Step 5 — Verify

```bash
dotnet build src/ConfigCompare.Settings.Contract
dotnet build src/ConfigCompare.Settings.Service
dotnet test tests/unit/ConfigCompare.Settings.Service.Tests.Unit --collect:"XPlat Code Coverage"
```

---

## Acceptance Criteria

- [ ] `ISettingsService` interface, `AppTheme` enum, and all DTOs in Contract
- [ ] `SettingsService` with atomic file write and validation
- [ ] Default settings returned when no file exists
- [ ] DI extension registered
- [ ] All unit tests pass
- [ ] No compiler warnings
- [ ] Coverage ≥ 80%
