# Agent Plan — Comparison Component

## Goal

Implement the `Comparison` component: contract library and service library for diffing two sets of AppConfig entries or Key Vault secrets, plus comprehensive unit tests. This component has no Azure SDK dependencies — it is pure in-memory logic.

---

## Pre-conditions

- `AppConfig.Contract` complete (`AppConfigEntryDto` available)
- `KeyVault.Contract` complete (`SecretDto` available)
- `/src/ConfigCompare.Comparison.Contract/` and `/src/ConfigCompare.Comparison.Service/` project files exist
- Agent has read access to `docs/component-designs/comparison.md`

---

## Step 1 — Add NuGet References

**Contract project**:
- `Microsoft.Extensions.Logging.Abstractions`
- Project reference → `ConfigCompare.AppConfig.Contract`
- Project reference → `ConfigCompare.KeyVault.Contract`

**Service project**:
- `Microsoft.Extensions.Logging.Abstractions`
- Project reference → `ConfigCompare.Comparison.Contract`

**Test project** (`ConfigCompare.Comparison.Service.Tests.Unit`):
- `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions`, `coverlet.collector`
- Project reference → `ConfigCompare.Comparison.Service`

---

## Step 2 — Implement Contract

Create in `ConfigCompare.Comparison.Contract`:

1. `ComparisonOptions.cs` — record: `CaseSensitiveKeys` (default false), `CaseSensitiveValues` (default true), `IncludeLabelsInKey` (default false)
2. `DiffStatus.cs` — enum: `Identical`, `Changed`, `OnlyInA`, `OnlyInB`
3. `ComparisonEntry.cs` — generic record: `EntryA?`, `EntryB?`, `Status`
4. `CompareRequest.cs` — generic record: `LabelA`, `EntriesA`, `LabelB`, `EntriesB`, `Options`
5. `CompareAppConfigResponse.cs` — record: `IsSuccess`, `ErrorMessage?`, `Data?`
6. `CompareSecretsResponse.cs` — record: `IsSuccess`, `ErrorMessage?`, `Data?`
7. `IComparisonService.cs` — two methods per design spec

---

## Step 3 — Implement Service

Create in `ConfigCompare.Comparison.Service`:

1. `ComparisonService.cs`:
   - Constructor: `(ILogger<ComparisonService> logger)`
   - Private helper `Diff<T>(IReadOnlyList<T> a, IReadOnlyList<T> b, Func<T, string> keySelector, Func<T, T, bool> valueEquals, ComparisonOptions options)` → `IReadOnlyList<ComparisonEntry<T>>`
   - Build dictionaries keyed by `keySelector(entry)` using `StringComparer.OrdinalIgnoreCase` or `Ordinal` per `CaseSensitiveKeys`
   - Iterate union of keys; categorise as `Identical`, `Changed`, `OnlyInA`, `OnlyInB`
   - Order results: `OnlyInA` → `OnlyInB` → `Changed` → `Identical`
   - `CompareAppConfigAsync`: calls `Diff` with key = `entry.Key + (IncludeLabelsInKey ? ":" + entry.Label : "")`, value equality = `string.Equals(a.Value, b.Value, sensitivity)`
   - `CompareSecretsAsync`: calls `Diff` with key = `secret.Name`, value equality = `string.Equals(a.Value, b.Value, sensitivity)`
   - Wrap each method in `try/catch`

2. `ServiceCollectionExtensions.cs`:
   - `AddComparisonServices(this IServiceCollection services)`

---

## Step 4 — Unit Tests

No mocking needed (pure logic). Use direct instantiation of `ComparisonService`.

Implement all scenarios from `docs/component-designs/comparison.md` (Test Surface section).

Include performance test: 1000-entry lists must complete in < 1 second (use `Stopwatch`).

---

## Step 5 — Verify

```bash
dotnet build src/ConfigCompare.Comparison.Contract
dotnet build src/ConfigCompare.Comparison.Service
dotnet test tests/unit/ConfigCompare.Comparison.Service.Tests.Unit --collect:"XPlat Code Coverage"
```

---

## Acceptance Criteria

- [ ] `IComparisonService`, `DiffStatus`, `ComparisonEntry<T>`, all request/response records in Contract
- [ ] `ComparisonService` implementation with generic diff helper
- [ ] Result ordering: OnlyInA, OnlyInB, Changed, Identical
- [ ] DI extension registered
- [ ] All unit tests pass including performance scenario
- [ ] No compiler warnings
- [ ] Coverage ≥ 80%
