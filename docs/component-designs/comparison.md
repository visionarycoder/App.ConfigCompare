# Component Design — Comparison

## Responsibility

Accept two or more collections of configuration entries (from AppConfig or Key Vault) and produce a structured diff: keys present only on one side, keys present on both sides with differing values, and keys that are identical. The comparison is pure logic — it has no Azure SDK dependency.

---

## Volatility

This component changes when:
- Comparison rules change (e.g., case sensitivity, label-awareness, content-type matching)
- The output format or categorization changes
- New comparison modes are added (three-way diff, label-filtered diff)

---

## Contract: `ConfigCompare.Comparison.Contract`

### Interfaces

```csharp
public interface IComparisonService
{
    Task<CompareAppConfigResponse> CompareAppConfigAsync(
        CompareRequest<AppConfigEntryDto> request,
        CancellationToken cancellationToken = default);

    Task<CompareSecretsResponse> CompareSecretsAsync(
        CompareRequest<SecretDto> request,
        CancellationToken cancellationToken = default);
}
```

### DTOs

```csharp
public record CompareRequest<T>(
    string LabelA,
    IReadOnlyList<T> EntriesA,
    string LabelB,
    IReadOnlyList<T> EntriesB,
    ComparisonOptions Options);

public record ComparisonOptions(
    bool CaseSensitiveKeys = false,
    bool CaseSensitiveValues = true,
    bool IncludeLabelsInKey = false);

public record ComparisonEntry<T>(T? EntryA, T? EntryB, DiffStatus Status);

public enum DiffStatus { Identical, Changed, OnlyInA, OnlyInB }

public record CompareAppConfigResponse(
    bool IsSuccess,
    string? ErrorMessage,
    IReadOnlyList<ComparisonEntry<AppConfigEntryDto>>? Data);

public record CompareSecretsResponse(
    bool IsSuccess,
    string? ErrorMessage,
    IReadOnlyList<ComparisonEntry<SecretDto>>? Data);
```

---

## Service: `ConfigCompare.Comparison.Service`

### Implementation Notes

- Pure in-memory computation — no Azure SDK dependency; takes in memory lists only
- `CompareAppConfigAsync`:
  - Builds a key from `AppConfigEntryDto.Key` (and optionally `Label` when `IncludeLabelsInKey=true`)
  - Uses case-insensitive or case-sensitive key comparison per `Options.CaseSensitiveKeys`
  - Categorises each entry as `Identical`, `Changed`, `OnlyInA`, or `OnlyInB`
  - For `Changed`: compares `Value` with `Options.CaseSensitiveValues`
- `CompareSecretsAsync`:
  - Same algorithm applied to `SecretDto.Name` as the key
  - Value comparison on `SecretDto.Value`
  - `ExpiresOn` delta is surfaced in the `Changed` category when values are equal but expiry differs (future enhancement — note in design)
- Results are ordered: `OnlyInA`, `OnlyInB`, `Changed`, `Identical` (most actionable first)
- Method wraps in `try/catch`; returns `IsSuccess=false` on unexpected error (though pure logic is unlikely to throw)

### DI Registration

```csharp
public static IServiceCollection AddComparisonServices(this IServiceCollection services)
{
    services.AddScoped<IComparisonService, ComparisonService>();
    return services;
}
```

---

## Dependencies

| Dependency | Notes |
|---|---|
| `ConfigCompare.AppConfig.Contract` | For `AppConfigEntryDto` input type |
| `ConfigCompare.KeyVault.Contract` | For `SecretDto` input type |
| `Microsoft.Extensions.Logging.Abstractions` | Logging |

No Azure SDK packages — this component is intentionally free of network dependencies.

---

## Test Surface

### Unit Tests

| Scenario | Expected |
|---|---|
| Identical entry sets | All entries `DiffStatus.Identical` |
| Entry only in A | `DiffStatus.OnlyInA` |
| Entry only in B | `DiffStatus.OnlyInB` |
| Same key, different value | `DiffStatus.Changed` |
| Key case mismatch, `CaseSensitiveKeys=false` | Treated as same key |
| Key case mismatch, `CaseSensitiveKeys=true` | Treated as different keys |
| Value case mismatch, `CaseSensitiveValues=false` | `DiffStatus.Identical` |
| Value case mismatch, `CaseSensitiveValues=true` | `DiffStatus.Changed` |
| Empty lists both sides | Returns empty result, `IsSuccess=true` |
| Large lists (1000+ entries) | Returns correct diff, completes in < 1 second |
| Cancellation before processing | `IsSuccess=false`, cancellation described |

---

## Use Cases Served

- UC-8: Compare configurations between resource groups, highlighting missing keys and differing values
