# Integration Plan — ConfigCompare

## Purpose

This document defines the integration points between components, the order in which they should be integrated, and the verification criteria at each integration boundary.

---

## Integration Point Map

```
┌─────────────────────────────────────────────────────────────────────────┐
│  UI Shell (WPF App)                                                     │
│   ├── SettingsViewModel   ──► ISettingsService                          │
│   ├── SessionViewModel    ──► ISessionService                           │
│   ├── ResourceGroupVM     ──► IResourceGroupService, IAuthService       │
│   ├── AppConfigVM         ──► IAppConfigService                         │
│   ├── KeyVaultVM          ──► IKeyVaultService                          │
│   ├── ComparisonVM        ──► IComparisonService                        │
│   └── PimVM               ──► IPimService, IAuthService                 │
│                                                                         │
│  Footer                   ──► IAuthService (current user display name)  │
└─────────────────────────────────────────────────────────────────────────┘
         │
         ▼
┌────────────────────────────────┐
│  IAuthService                  │ ──► Azure AD (DefaultAzureCredential)
└────────────────────────────────┘
         │ TokenCredential
         ▼
┌────────────────────────────────────────────────────────────────────────┐
│  IResourceGroupService  ──► Azure ARM API                              │
│  IAppConfigService      ──► Azure App Configuration                    │
│  IKeyVaultService       ──► Azure Key Vault                            │
│  IPimService            ──► Microsoft Graph (PIM)                      │
└────────────────────────────────────────────────────────────────────────┘
         │
         ▼
┌────────────────────────────────┐
│  IComparisonService            │ ◄── AppConfigEntryDto, SecretDto (in-memory)
└────────────────────────────────┘
         │
         ▼
┌────────────────────────────────┐
│  ISessionService               │ ──► SQLite (%LocalAppData%)
│  ISettingsService              │ ──► JSON file (%LocalAppData%)
└────────────────────────────────┘
```

---

## Integration Points Detail

### IP-1: AzureIdentity → All Azure-backed Services

**Components**: `AzureIdentity.Service` → `ResourceGroup.Service`, `AppConfig.Service`, `KeyVault.Service`, `Pim.Service`

**Contract**: `TokenCredential` (registered as singleton in DI)

**Integration method**: The `TokenCredential` singleton (`DefaultAzureCredential`) is registered once and injected into each Azure-SDK-backed service. No service constructs its own credential.

**Verification**:
- Start app; footer displays current user's display name
- `IResourceGroupService.GetResourceGroupsAsync()` returns non-empty list
- Confirm single sign-in prompt (not one per service)

---

### IP-2: ResourceGroup → AppConfig + KeyVault (Endpoint Discovery)

**Components**: `ResourceGroup.Service` → `AppConfig.Service`, `KeyVault.Service`

**Contract**: `AppConfigEndpointDto.Endpoint` (string URI), `KeyVaultEndpointDto.VaultUri` (string URI)

**Integration method**: The UI ViewModel calls `IResourceGroupService.GetServicesInGroupAsync()` to obtain endpoint URIs, then passes those URIs to `IAppConfigService.GetEntriesAsync(endpoint)` and `IKeyVaultService.GetSecretsAsync(vaultUri)`.

**Verification**:
- Select a resource group in the UI; AppConfig and Key Vault panels populate with entries from that resource group's services
- Selecting a different resource group clears and repopulates both panels

---

### IP-3: AppConfig + KeyVault → Comparison

**Components**: `AppConfig.Service`, `KeyVault.Service` → `Comparison.Service`

**Contract**: `IReadOnlyList<AppConfigEntryDto>`, `IReadOnlyList<SecretDto>`

**Integration method**: The Comparison ViewModel collects entries from two resource group contexts (two `GetEntriesAsync` calls) then passes both lists to `IComparisonService.CompareAppConfigAsync()`.

**Verification**:
- Select two resource groups with known differences; comparison view shows correct `OnlyInA`, `OnlyInB`, `Changed`, and `Identical` rows
- Rows are highlighted with distinct visual styling per `DiffStatus`

---

### IP-4: Settings → UI Shell (Theme + Refresh Timer)

**Components**: `Settings.Service` → UI Shell `MainWindow`, AppConfig/KeyVault ViewModels

**Contract**: `UserSettingsDto.Theme`, `UserSettingsDto.AutoRefreshIntervalSeconds`, `UserSettingsDto.AutoRefreshEnabled`

**Integration method**:
- On startup, `App.xaml.cs` calls `ISettingsService.GetSettingsAsync()` and applies `Theme` via WPF UI `ThemeService`
- Each AppConfig/KeyVault ViewModel subscribes to settings change notification and restarts its `DispatcherTimer` on interval change

**Verification**:
- Change theme to Dark; UI immediately switches without restart
- Change refresh interval to 30s; confirm next auto-refresh fires at ~30s

---

### IP-5: Session → UI Shell (Save on Close / Restore on Startup)

**Components**: `Session.Service` → UI Shell `MainWindow`, Navigation service

**Contract**: `SessionSnapshotDto`

**Integration method**:
- `MainWindow.OnClosing` calls `ISessionService.SaveSessionAsync()` with current navigation state
- `App.xaml.cs` OnStartup calls `ISessionService.LoadSessionAsync()` and navigates to the last active module with the last selected resource groups

**Verification**:
- Open app, navigate to AppConfig module, select a resource group; close app; reopen — same module and resource group are active
- `ClearSession` resets to default state on next launch

---

### IP-6: PIM → AzureIdentity + UI Shell

**Components**: `Pim.Service` → `AzureIdentity.Service` (for current user's `ObjectId`), UI Shell PIM panel

**Contract**: `UserInfo.ObjectId` from `IAuthService.GetCurrentUserAsync()` feeds PIM role filtering

**Integration method**: PIM ViewModel calls `IAuthService.GetCurrentUserAsync()` to obtain `ObjectId`, then `IPimService.GetEligibleRolesAsync()`. Role activation uses `IPimService.ActivateRoleAsync()`.

**Verification**:
- User with at least one eligible PIM role sees it listed; activation succeeds; role appears in active roles list
- App does not crash or show an error if user has no eligible roles

---

## Integration Test Strategy

Integration tests in `ConfigCompare.Integration.Tests` require live Azure credentials. They are skipped automatically in CI when the environment variable `AZURE_TEST_TENANT_ID` is absent.

### Test Scenarios

| Test | Components Under Test | Azure Resources Required |
|---|---|---|
| IT-01: SSO and user identity | AzureIdentity | Azure AD tenant |
| IT-02: List resource groups | AzureIdentity + ResourceGroup | Azure subscription with ≥1 resource group |
| IT-03: List AppConfig entries | AzureIdentity + ResourceGroup + AppConfig | Resource group with AppConfig store |
| IT-04: List Key Vault secrets | AzureIdentity + ResourceGroup + KeyVault | Resource group with Key Vault |
| IT-05: CRUD on AppConfig | AzureIdentity + AppConfig | Writable AppConfig store (test namespace) |
| IT-06: CRUD on Key Vault | AzureIdentity + KeyVault | Writable Key Vault (test secrets) |
| IT-07: Compare two AppConfigs | AzureIdentity + AppConfig + Comparison | Two AppConfig stores with known overlap/diff |
| IT-08: Settings persist across instances | Settings | None (local only) |
| IT-09: Session restore | Session | None (local only) |

---

## Environment Setup for Integration Tests

```
AZURE_TEST_TENANT_ID=<tenant guid>
AZURE_TEST_SUBSCRIPTION_ID=<subscription guid>
AZURE_TEST_RESOURCE_GROUP_A=<resource-group-name>
AZURE_TEST_RESOURCE_GROUP_B=<resource-group-name>
AZURE_TEST_APPCONFIG_ENDPOINT_A=<https://...>
AZURE_TEST_APPCONFIG_ENDPOINT_B=<https://...>
AZURE_TEST_KEYVAULT_URI_A=<https://...>
```

These variables are set in CI via GitHub Actions secrets and in local development via a `.env` file (excluded from source control via `.gitignore`).
