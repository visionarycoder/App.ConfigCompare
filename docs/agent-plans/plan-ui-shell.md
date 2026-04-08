# Agent Plan — UI Shell & Modules

## Goal

Implement the WPF application project (`ConfigCompare.App`): the main shell window, navigation, theme management, footer, and all user-facing modules. Wire all service components together via DI. Deliver a complete, working Win11 desktop application.

---

## Pre-conditions

- All service components complete and tested:
  - AzureIdentity, ResourceGroup, AppConfig, KeyVault, Comparison, Session, Settings, Pim
- Agent has read access to `docs/system-design.md` and all `docs/component-designs/*.md`
- .NET 10 SDK + WPF workload installed

---

## Step 1 — Create App Project

Create a new WPF application project targeting `net10.0-windows`:

```
src/ConfigCompare.App/ConfigCompare.App.csproj
```

Target framework: `net10.0-windows`
`<UseWPF>true</UseWPF>`

---

## Step 2 — Add NuGet References

- `WPF-UI` (Wpf-UI) — Win11 Fluent styling
- `CommunityToolkit.Mvvm` — MVVM source generators, `ObservableObject`, `RelayCommand`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Logging`
- `Serilog.Extensions.Logging.File` (or `Serilog.Sinks.File`)
- `Microsoft.Data.Sqlite`
- Project references → all Contract and Service projects

---

## Step 3 — DI Setup (`App.xaml.cs`)

In `App.xaml.cs`, build `IServiceProvider` in `OnStartup`:

```
host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddAzureIdentityServices();
        services.AddResourceGroupServices();
        services.AddAppConfigServices();
        services.AddKeyVaultServices();
        services.AddComparisonServices();
        services.AddPimServices();
        services.AddSessionServices(dbPath);
        services.AddSettingsServices(settingsFilePath);
        // ViewModels, NavigationService, etc.
    })
    .Build();
```

`dbPath` and `settingsFilePath` → `%LocalAppData%\ConfigCompare\`

---

## Step 4 — Shell: MainWindow

`MainWindow.xaml` / `MainWindow.xaml.cs`:

- WPF UI `FluentWindow` as base
- Left navigation rail using `NavigationView` (WPF UI) with items:
  - Resource Groups
  - App Configuration
  - Key Vault
  - Compare
  - PIM
  - Settings
- `ContentFrame` for page navigation
- Footer bar (bottom): `BuildVersion` | `Uptime` | `CurrentUser`
- Top-right avatar button → opens Settings module

**Footer ViewModel** (`ShellViewModel`):
- `BuildVersion` — from `AssemblyInformationalVersionAttribute`
- `Uptime` — `DispatcherTimer` firing every second, formats as `HH:mm:ss`
- `CurrentUser` — calls `IAuthService.GetCurrentUserAsync()` on startup; displays `UserInfo.DisplayName`

---

## Step 5 — Theme Service

On startup: read `UserSettingsDto.Theme`; call WPF UI `ApplicationThemeManager.Apply(theme)`.

When `UserSettingsDto.Theme` changes (via Settings module): re-apply theme immediately.

Map `AppTheme.System` → `ApplicationTheme.Unknown` (WPF UI follows system).

---

## Step 6 — Session Restore

In `OnStartup` (after DI built):
1. Call `ISessionService.LoadSessionAsync()`
2. If `Data != null`: navigate to `ActiveModule`; restore `SelectedResourceGroupNames` into ResourceGroup ViewModel
3. On `MainWindow.Closing`: call `ISessionService.SaveSessionAsync()` with current navigation state

---

## Step 7 — Modules

### Resource Groups Module (`Modules/ResourceGroups/`)

- `ResourceGroupsPage.xaml` + `ResourceGroupsViewModel`
- On load: calls `IResourceGroupService.GetResourceGroupsAsync()`
- Displays `ListView` of resource groups
- User selects one (or more) → calls `GetServicesInGroupAsync()` → updates AppConfig/KeyVault endpoint context
- PIM activation panel: list eligible roles → activate button (with justification dialog)

### App Configuration Module (`Modules/AppConfig/`)

- `AppConfigPage.xaml` + `AppConfigViewModel`
- Displays `DataGrid` of `AppConfigEntryDto` from selected endpoint
- CRUD toolbar: Add, Edit (inline or dialog), Delete
- Auto-refresh: `DispatcherTimer` driven by `UserSettingsDto.AutoRefreshIntervalSeconds`; shows `Last refreshed: {local time}` label
- Manual refresh button

### Key Vault Module (`Modules/KeyVault/`)

- `KeyVaultPage.xaml` + `KeyVaultViewModel`
- Same structure as AppConfig module but for secrets
- Secret value column hidden by default; reveal button (eye icon) toggles visibility
- Auto-refresh same pattern as AppConfig

### Comparison Module (`Modules/Comparison/`)

- `ComparisonPage.xaml` + `ComparisonViewModel`
- Two endpoint selectors (Resource Group A / Resource Group B)
- "Compare" button calls `IComparisonService.CompareAppConfigAsync()` and/or `CompareSecretsAsync()`
- `DataGrid` with row background color per `DiffStatus`:
  - `OnlyInA` → red
  - `OnlyInB` → blue
  - `Changed` → yellow
  - `Identical` → default

### Settings Module (`Modules/Settings/`)

- `SettingsPage.xaml` + `SettingsViewModel`
- Theme selector: Radio buttons (Light / Dark / System)
- Refresh interval: `NumericUpDown` (10–3600 seconds)
- Auto-refresh toggle
- "About" section: app name, version, description
- Save / Reset buttons

---

## Step 8 — Solution File

Create `ConfigCompare.slnx` in the repository root, referencing all projects.

---

## Step 9 — Verify

```bash
dotnet build src/ConfigCompare.App
dotnet run --project src/ConfigCompare.App
```

Manual walkthrough of all 11 use cases against a real Azure tenant.

---

## Acceptance Criteria

- [ ] App project builds without warnings
- [ ] All service components wired via DI
- [ ] Shell navigation works (all modules accessible)
- [ ] Footer shows build version, uptime, current user
- [ ] Theme switches immediately without restart
- [ ] Session restores on app relaunch
- [ ] AppConfig CRUD works against a real AppConfig store
- [ ] Key Vault CRUD works against a real Key Vault
- [ ] Comparison diff view shows color-coded rows
- [ ] PIM role activation works end-to-end
- [ ] `ConfigCompare.slnx` includes all projects
