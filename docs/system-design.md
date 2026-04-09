# ConfigCompare — System Design

## Mission Statement

ConfigCompare is a lightweight Win11 WPF desktop tool for reading and comparing settings stored in Azure App Configuration and Azure Key Vault across multiple resource groups. It enables engineering and operations teams to verify configuration parity before and after releases—especially critical during hard cut-over production deployments.

---

## Architecture Philosophy — Volatility-Based Decomposition (VBD)

Components are organized by *what changes together*, not by technical layer. Each component boundary isolates a distinct axis of volatility:

| Axis of Volatility | Component(s) |
|---|---|
| Azure authentication & token lifecycle | AzureIdentity |
| Azure resource group enumeration | ResourceGroup |
| Azure App Configuration API surface | AppConfig |
| Azure Key Vault API surface | KeyVault |
| Comparison rules & output format | Comparison |
| PIM role-activation workflow | Pim |
| User preferences & UI state | Settings |
| Session snapshot & restore | Session |
| Shell UI chrome & navigation | UI Shell (WPF app project) |

Each component exposes only its **Contract** (interfaces + DTOs) and hides its implementation behind a **Service**. Callers are wired together by the DI container—never by direct construction.

---

## Solution Structure

```
ConfigCompare.slnx
│
├── src/
│   ├── ConfigCompare.AzureIdentity.Contract/      # IAuthService, AuthResponse, TokenInfo
│   ├── ConfigCompare.AzureIdentity.Service/       # DefaultAzureCredential-based impl
│   ├── ConfigCompare.ResourceGroup.Contract/      # IResourceGroupService, ResourceGroupDto
│   ├── ConfigCompare.ResourceGroup.Service/       # ARM client impl
│   ├── ConfigCompare.AppConfig.Contract/          # IAppConfigService, AppConfigEntryDto
│   ├── ConfigCompare.AppConfig.Service/           # Azure App Configuration SDK impl
│   ├── ConfigCompare.KeyVault.Contract/           # IKeyVaultService, SecretDto
│   ├── ConfigCompare.KeyVault.Service/            # Azure Key Vault SDK impl
│   ├── ConfigCompare.Comparison.Contract/         # IComparisonService, ComparisonResultDto
│   ├── ConfigCompare.Comparison.Service/          # Diff engine impl
│   ├── ConfigCompare.Pim.Contract/                # IPimService, PimRoleDto
│   ├── ConfigCompare.Pim.Service/                 # Azure PIM API impl
│   ├── ConfigCompare.Session.Contract/            # ISessionService, SessionSnapshotDto
│   ├── ConfigCompare.Session.Service/             # SQLite-backed impl
│   ├── ConfigCompare.Settings.Contract/           # ISettingsService, UserSettingsDto
│   └── ConfigCompare.Settings.Service/            # JSON/SQLite-backed impl
│
├── tests/
│   ├── unit/
│   │   ├── ConfigCompare.AppConfig.Service.Tests.Unit/
│   │   ├── ConfigCompare.AzureIdentity.Service.Tests.Unit/
│   │   ├── ConfigCompare.Comparison.Service.Tests.Unit/
│   │   ├── ConfigCompare.KeyVault.Service.Tests.Unit/
│   │   ├── ConfigCompare.Session.Service.Tests.Unit/
│   │   └── ConfigCompare.Settings.Service.Tests.Unit/
│   └── integration/
│       └── ConfigCompare.Integration.Tests/
│
└── docs/
    ├── system-design.md                           ← this file
    ├── critical-path.md
    ├── integration-plan.md
    ├── component-designs/
    └── agent-plans/
```

---

## Technology Stack

| Concern | Choice | Rationale |
|---|---|---|
| Runtime | .NET 10 | LTS, C# 14 features, performance |
| Language | C# 14 | Primary target platform |
| UI Framework | WPF | Win11 native, mature data-binding |
| UI Pattern | MVVM (CommunityToolkit.Mvvm) | Testable view-models, source generators |
| UI Styling | WPF UI (Wpf-UI) | Fluent/Win11 aesthetic out of box |
| Azure Auth | Azure.Identity | DefaultAzureCredential, SSO |
| Azure AppConfig | Azure.Data.AppConfiguration | Official SDK |
| Azure KeyVault | Azure.Security.KeyVault.Secrets | Official SDK |
| Azure ARM | Azure.ResourceManager | Resource group enumeration |
| Azure PIM | Microsoft.Graph | PIM role activation via MS Graph |
| Persistence | SQLite via Microsoft.Data.Sqlite | Local DB for session + settings |
| DI Container | Microsoft.Extensions.DependencyInjection | Standard .NET DI |
| Logging | Microsoft.Extensions.Logging + Serilog | Structured logging to file/debug |
| Unit Testing | xUnit | Standard .NET testing framework |
| Mocking | Moq | Interface-based mock creation |
| Assertions | FluentAssertions | Readable test assertions |
| Code Coverage | Coverlet + ReportGenerator | 80% minimum target |

---

## Cross-Cutting Concerns

### Dependency Injection

All services are registered in `Program.cs` (or `App.xaml.cs`) via `IServiceCollection` extension methods. Each `*.Service` project exposes a `IServiceCollection.AddXxx()` extension. No component directly instantiates another.

### Async / Await Pattern

Every public method on a service interface:
- Returns `Task<TResponse>` (never `void`)
- Accepts `CancellationToken cancellationToken` as the final parameter
- Honors the cancellation token internally
- Wraps its body in `try/catch`
- Logs all exceptions at `Error` level with full detail
- Returns a populated `TResponse` (never throws to callers)
- Sets `TResponse.IsSuccess = false` and `TResponse.ErrorMessage` when an exception occurs

### Response Object Pattern

All service method return types follow this shape (see individual Contract designs):
```csharp
public class ServiceResponse<T>
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public T? Data { get; init; }
}
```

### Logging

`ILogger<T>` injected into every service. Serilog configured at startup to write structured JSON logs to `%LocalAppData%\ConfigCompare\logs\`. Log levels: `Debug` in development, `Information` in production.

### Naming Conventions

- No underscore prefixes (fields: `camelCase`, properties: `PascalCase`)
- Markdown files: lowercase with hyphens
- Project names follow VBD component naming: `ConfigCompare.<Component>.<Role>`

---

## UI Architecture

### Shell

A single `MainWindow` hosts a navigation frame. The shell is responsible for:
- Top-level navigation rail / sidebar
- Footer bar (build number, uptime, current user)
- Top-right avatar / user settings entry point
- Theme management (light / dark / system)

### Modules

Each VBD component with user-facing screens has a dedicated WPF module folder under the app project:
- `Modules/ResourceGroups/` — resource group selector, PIM activation trigger
- `Modules/AppConfig/` — configuration browser, CRUD
- `Modules/KeyVault/` — secret browser, CRUD
- `Modules/Comparison/` — side-by-side diff view
- `Modules/Settings/` — theme, refresh interval, about

### Theme

WPF UI's `ThemeService` is wired to `UserSettingsDto.Theme` (`Light` | `Dark` | `System`). The `Settings` module exposes the control; the shell applies it at startup and on change.

### Footer

The shell `MainWindow` exposes three footer bindings:
- `BuildVersion` — read from `AssemblyInformationalVersionAttribute`
- `Uptime` — a `DispatcherTimer` incrementing every second
- `CurrentUser` — provided by `IAuthService.GetCurrentUserAsync()`

---

## Data Flow (Primary Use Case: Compare Configs)

```
User selects two resource groups
        │
        ▼
ResourceGroupService.GetResourceGroupsAsync()
        │
        ├──► AppConfigService.GetEntriesAsync(resourceGroupA)
        │
        ├──► AppConfigService.GetEntriesAsync(resourceGroupB)
        │
        ▼
ComparisonService.CompareAsync(entriesA, entriesB)
        │
        ▼
ComparisonResultDto { Added[], Removed[], Changed[], Identical[] }
        │
        ▼
ComparisonViewModel → DataGrid with conditional row styling
```

---

## Security Considerations

- No credentials stored in code or settings files
- `DefaultAzureCredential` chains through: VisualStudio > AzureCLI > ManagedIdentity
- Key Vault secrets displayed in-app only; never written to disk
- PIM activation goes through Graph API; no role changes made directly
- SQLite database stored in `%LocalAppData%\ConfigCompare\` (user-scoped)

---

## Quality Gates

- Minimum 80% code coverage (unit tests)
- All public API surfaces covered by at least one happy-path and one error-path test
- Integration tests require live Azure credentials (skipped in CI without env vars)
- No compiler warnings (treat-warnings-as-errors in release builds)
