# Agent Plan — Session Component

## Goal

Implement the `Session` component: contract library and SQLite-backed service library for snapshotting and restoring user application state across restarts, plus unit tests.

---

## Pre-conditions

- `/src/ConfigCompare.Session.Contract/` and `/src/ConfigCompare.Session.Service/` project files exist
- Agent has read access to `docs/component-designs/session.md`

---

## Step 1 — Add NuGet References

**Contract project**:
- `Microsoft.Extensions.Logging.Abstractions`

**Service project**:
- `Microsoft.Data.Sqlite`
- `Microsoft.Extensions.Logging.Abstractions`
- Project reference → `ConfigCompare.Session.Contract`

**Test project** (`ConfigCompare.Session.Service.Tests.Unit`):
- `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions`, `coverlet.collector`
- Project reference → `ConfigCompare.Session.Service`

---

## Step 2 — Implement Contract

Create in `ConfigCompare.Session.Contract`:

1. `SessionSnapshotDto.cs` — record: `SavedAt`, `OpenModules`, `ActiveModule?`, `SelectedResourceGroupNames`, `WindowState` (dictionary), `ModuleState` (dictionary)
2. `SaveSessionResponse.cs` — record: `IsSuccess`, `ErrorMessage?`
3. `LoadSessionResponse.cs` — record: `IsSuccess`, `ErrorMessage?`, `Data?`
4. `ClearSessionResponse.cs` — record: `IsSuccess`, `ErrorMessage?`
5. `ISessionService.cs` — three methods per design spec

---

## Step 3 — Implement Service

Create in `ConfigCompare.Session.Service`:

1. `DatabaseInitialiser.cs`:
   - Constructor: `(string dbPath)`
   - `InitialiseAsync()`: creates the database file and runs DDL: `CREATE TABLE IF NOT EXISTS Sessions (Id INTEGER PRIMARY KEY, SavedAt TEXT NOT NULL, Payload TEXT NOT NULL)`

2. `SessionService.cs`:
   - Constructor: `(string dbPath, ILogger<SessionService> logger)`
   - `SaveSessionAsync`: opens `SqliteConnection`; deletes all existing rows; inserts one row with `SavedAt = DateTimeOffset.UtcNow.ToString("O")` and `Payload = JsonSerializer.Serialize(snapshot)`; `try/catch`
   - `LoadSessionAsync`: opens connection; reads the single row; deserialises `Payload`; returns `Data=null` (not error) if no row exists
   - `ClearSessionAsync`: `DELETE FROM Sessions`

3. `ServiceCollectionExtensions.cs`:
   - `AddSessionServices(this IServiceCollection services, string dbPath)` — calls `DatabaseInitialiser.InitialiseAsync()` eagerly, registers `ISessionService`

---

## Step 4 — Unit Tests

Use an in-memory SQLite database (`Data Source=:memory:`) for tests.

Implement all scenarios from `docs/component-designs/session.md` (Test Surface section).

---

## Step 5 — Verify

```bash
dotnet build src/ConfigCompare.Session.Contract
dotnet build src/ConfigCompare.Session.Service
dotnet test tests/unit/ConfigCompare.Session.Service.Tests.Unit --collect:"XPlat Code Coverage"
```

---

## Acceptance Criteria

- [ ] `ISessionService` interface and all DTOs in Contract
- [ ] `SessionService` implementation with SQLite persistence
- [ ] `DatabaseInitialiser` creates schema on first run
- [ ] DI extension registered
- [ ] All unit tests pass (using in-memory SQLite)
- [ ] No compiler warnings
- [ ] Coverage ≥ 80%
