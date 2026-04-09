# Component Design — Settings

## Responsibility

Store, retrieve, and validate user preferences: theme selection, auto-refresh interval, saved resource group names, and all other application-level settings. Persist preferences across application restarts.

---

## Volatility

This component changes when:
- New user-configurable settings are added
- The persistence backend changes
- Default values or validation rules change

---

## Contract: `ConfigCompare.Settings.Contract`

### Interfaces

```csharp
public interface ISettingsService
{
    Task<GetSettingsResponse> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<SaveSettingsResponse> SaveSettingsAsync(UserSettingsDto settings, CancellationToken cancellationToken = default);
    Task<ResetSettingsResponse> ResetSettingsAsync(CancellationToken cancellationToken = default);
}
```

### DTOs

```csharp
public enum AppTheme { Light, Dark, System }

public record UserSettingsDto(
    AppTheme Theme,
    int AutoRefreshIntervalSeconds,
    bool AutoRefreshEnabled,
    IReadOnlyList<string> SavedResourceGroupNames,
    string? LastActiveSubscriptionId);

public record GetSettingsResponse(bool IsSuccess, string? ErrorMessage, UserSettingsDto? Data);
public record SaveSettingsResponse(bool IsSuccess, string? ErrorMessage);
public record ResetSettingsResponse(bool IsSuccess, string? ErrorMessage);
```

### Defaults

| Setting | Default |
|---|---|
| `Theme` | `AppTheme.System` |
| `AutoRefreshIntervalSeconds` | `60` |
| `AutoRefreshEnabled` | `false` |
| `SavedResourceGroupNames` | `[]` |
| `LastActiveSubscriptionId` | `null` |

---

## Service: `ConfigCompare.Settings.Service`

### Implementation Notes

- Persists settings as a JSON file at `%LocalAppData%\ConfigCompare\settings.json`
- `GetSettingsAsync`:
  - Reads and deserialises the file
  - If the file does not exist, returns default `UserSettingsDto`
- `SaveSettingsAsync`:
  - Validates that `AutoRefreshIntervalSeconds` is between 10 and 3600 (returns error if not)
  - Serialises and writes to disk atomically (write to temp file, then rename)
- `ResetSettingsAsync`: deletes the settings file; next `GetSettingsAsync` returns defaults
- Uses `System.Text.Json` with `JsonSerializerOptions.WriteIndented = true` for human-readability
- The settings file is user-scoped (`%LocalAppData%`) — not shared across Windows users

### DI Registration

```csharp
public static IServiceCollection AddSettingsServices(this IServiceCollection services, string settingsFilePath)
{
    services.AddSingleton<ISettingsService>(sp =>
        new SettingsService(settingsFilePath, sp.GetRequiredService<ILogger<SettingsService>>()));
    return services;
}
```

---

## Dependencies

| Dependency | NuGet Package |
|---|---|
| JSON serialization | `System.Text.Json` (in-box) |
| Logging | `Microsoft.Extensions.Logging.Abstractions` |

---

## Test Surface

### Unit Tests

| Scenario | Expected |
|---|---|
| `GetSettingsAsync` — no file exists | Returns defaults, `IsSuccess=true` |
| `GetSettingsAsync` — valid file | Returns deserialized settings |
| `GetSettingsAsync` — corrupt JSON | `IsSuccess=false`, error message set |
| `SaveSettingsAsync` — valid settings | `IsSuccess=true`; subsequent get returns same values |
| `SaveSettingsAsync` — interval < 10 | `IsSuccess=false`, validation error message |
| `SaveSettingsAsync` — interval > 3600 | `IsSuccess=false`, validation error message |
| `SaveSettingsAsync` — write fails (read-only FS) | `IsSuccess=false`, error message set |
| `ResetSettingsAsync` — after save | Subsequent get returns defaults |

---

## Use Cases Served

- UC-2: Allow user to save settings (resource group names, preferences)
- UC-6 (indirect): Auto-refresh interval stored here, read by ViewModel timer
- UI shell: Theme setting read at startup and on change
