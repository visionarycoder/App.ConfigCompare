# Component Design — Session

## Responsibility

Snapshot and restore the user's complete application state across restarts: open screens, selected resource groups, scroll positions, column widths, and any other UI context needed to resume work with minimal effort.

---

## Volatility

This component changes when:
- The set of screens or UI state that should be persisted changes
- The persistence backend changes (currently SQLite)
- The session schema evolves (new fields, renamed fields)

---

## Contract: `ConfigCompare.Session.Contract`

### Interfaces

```csharp
public interface ISessionService
{
    Task<SaveSessionResponse> SaveSessionAsync(SessionSnapshotDto snapshot, CancellationToken cancellationToken = default);
    Task<LoadSessionResponse> LoadSessionAsync(CancellationToken cancellationToken = default);
    Task<ClearSessionResponse> ClearSessionAsync(CancellationToken cancellationToken = default);
}
```

### DTOs

```csharp
public record SessionSnapshotDto(
    DateTimeOffset SavedAt,
    IReadOnlyList<string> OpenModules,
    string? ActiveModule,
    IReadOnlyList<string> SelectedResourceGroupNames,
    IReadOnlyDictionary<string, string> WindowState,
    IReadOnlyDictionary<string, string> ModuleState);

public record SaveSessionResponse(bool IsSuccess, string? ErrorMessage);
public record LoadSessionResponse(bool IsSuccess, string? ErrorMessage, SessionSnapshotDto? Data);
public record ClearSessionResponse(bool IsSuccess, string? ErrorMessage);
```

`WindowState` and `ModuleState` are freeform key-value maps (serialised JSON) to allow each module to store arbitrary state without requiring schema changes.

---

## Service: `ConfigCompare.Session.Service`

### Implementation Notes

- Uses `Microsoft.Data.Sqlite` to read/write a local SQLite database at `%LocalAppData%\ConfigCompare\configcompare.db`
- Schema: single table `Sessions` with columns `Id` (INTEGER PK), `SavedAt` (TEXT), `Payload` (TEXT — JSON-serialised `SessionSnapshotDto`)
- `SaveSessionAsync`:
  - Upserts a single row (only the most recent session is kept; older rows are deleted on save)
  - Serialises `SessionSnapshotDto` to JSON via `System.Text.Json`
- `LoadSessionAsync`:
  - Reads the single row; deserialises payload
  - Returns `IsSuccess=true, Data=null` if no session exists (not an error — first run)
- `ClearSessionAsync`: deletes all rows
- Database is created / migrated at startup via a `DatabaseInitialiser` called from the DI registration extension

### DI Registration

```csharp
public static IServiceCollection AddSessionServices(this IServiceCollection services, string dbPath)
{
    services.AddSingleton<DatabaseInitialiser>(sp => new DatabaseInitialiser(dbPath));
    services.AddScoped<ISessionService, SessionService>(sp =>
        new SessionService(dbPath, sp.GetRequiredService<ILogger<SessionService>>()));
    return services;
}
```

---

## Dependencies

| Dependency | NuGet Package |
|---|---|
| SQLite driver | `Microsoft.Data.Sqlite` |
| JSON serialization | `System.Text.Json` (in-box) |
| Logging | `Microsoft.Extensions.Logging.Abstractions` |

---

## Test Surface

### Unit Tests

| Scenario | Expected |
|---|---|
| `SaveSessionAsync` — new snapshot | `IsSuccess=true`; subsequent load returns same snapshot |
| `SaveSessionAsync` — overwrite existing | Only latest snapshot returned on load |
| `LoadSessionAsync` — no prior session | `IsSuccess=true`, `Data=null` |
| `LoadSessionAsync` — existing session | `IsSuccess=true`, `Data` matches saved snapshot |
| `ClearSessionAsync` — clears data | Subsequent load returns `Data=null` |
| `SaveSessionAsync` — DB write fails | `IsSuccess=false`, error message set |
| `LoadSessionAsync` — corrupt JSON | `IsSuccess=false`, error message set |

---

## Use Cases Served

- UC-3: Restore previous session on app restart (open screens, selected resource groups, UI state)
