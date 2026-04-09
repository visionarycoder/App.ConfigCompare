# Critical Path — ConfigCompare

## Overview

The critical path identifies the sequence of increments where each depends on the previous one completing before work can begin. Parallel tracks that do not block the main chain are noted separately.

---

## Dependency Graph

```
[1] AzureIdentity
        │
        ├──────────────────────────────┐
        ▼                              ▼
[2] ResourceGroup               [2b] Settings + Session
        │                              │
        ▼                              │
[3] AppConfig + KeyVault ◄─────────────┘
        │
        ▼
[4] Comparison
        │
        ▼
[5] UI Shell + Module Integration
        │
        ▼
[6] PIM (additive — plugs into shell)
        │
        ▼
[7] Integration Testing + Quality Gate
```

---

## Increment 1 — AzureIdentity (Blocker)

**Why it's first**: Every other component that talks to Azure needs a `TokenCredential` or `IAuthService`. Nothing can be built or tested against real Azure without this.

**Deliverables**:
- `IAuthService` interface and DTOs
- `AuthService` implementation using `DefaultAzureCredential`
- Unit tests (mocked credential)
- DI registration extension

**Exit criteria**: `IAuthService.GetCurrentUserAsync()` returns the signed-in user's display name and UPN in a manual smoke test against a real Azure tenant.

---

## Increment 2a — ResourceGroup (Blocks: AppConfig, KeyVault, UI)

**Depends on**: AzureIdentity (for `TokenCredential`)

**Deliverables**:
- `IResourceGroupService` interface and DTOs
- `ResourceGroupService` implementation using ARM client
- Unit tests
- DI registration extension

**Exit criteria**: `GetResourceGroupsAsync()` returns at least one resource group from a live Azure subscription.

---

## Increment 2b — Settings + Session (Parallel with 2a)

**Depends on**: Nothing (pure local I/O)

**Deliverables**:
- `ISettingsService` + `SettingsService`
- `ISessionService` + `SessionService`
- SQLite database initializer
- Unit tests for both services

**Exit criteria**: Settings survive an app restart; session snapshot can be saved and fully restored.

---

## Increment 3 — AppConfig + KeyVault (Can be parallelized)

**Depends on**: AzureIdentity (Increment 1), ResourceGroup (Increment 2a) for endpoint discovery

**Deliverables**:
- `IAppConfigService` + `AppConfigService`
- `IKeyVaultService` + `KeyVaultService`
- Client factory/cache implementations
- Unit tests for both services

**Exit criteria**: Given an AppConfig endpoint from a real resource group, all entries can be listed, and a test entry can be created and deleted.

---

## Increment 4 — Comparison (Blocks: UI comparison view)

**Depends on**: AppConfig and KeyVault contracts (DTOs)

**Deliverables**:
- `IComparisonService` + `ComparisonService`
- Full unit test suite (pure logic — no Azure needed)

**Exit criteria**: Given two in-memory entry lists with known differences, `CompareAppConfigAsync` correctly categorizes all entries.

---

## Increment 5 — UI Shell + Module Integration (Critical UI milestone)

**Depends on**: All service components (Increments 1–4), Settings (for theme), Session (for restore)

**Deliverables**:
- WPF application project (`ConfigCompare.App`)
- `MainWindow` with navigation rail, footer (build, uptime, user), avatar
- Theme service wired to `UserSettingsDto.Theme`
- Navigation to each module
- Resource group selector module
- AppConfig browser module (list, CRUD)
- Key Vault browser module (list, CRUD)
- Comparison module (side-by-side diff view)
- Settings module (theme, refresh interval, about)
- Auto-refresh timer wired in AppConfig + KeyVault ViewModels
- Session save on close / restore on startup

**Exit criteria**: Full manual walkthrough of all use cases (UC-1 through UC-10) on a real Azure tenant.

---

## Increment 6 — PIM Integration (Additive)

**Depends on**: AzureIdentity (Increment 1), UI Shell (Increment 5) for surface area

**Deliverables**:
- `IPimService` + `PimService`
- Unit tests
- PIM panel in the UI (list eligible roles, activate with justification and duration)

**Exit criteria**: User can activate an eligible PIM role from within the app without leaving to the Azure portal.

---

## Increment 7 — Integration Testing + Quality Gate

**Depends on**: All prior increments

**Deliverables**:
- Integration test project wired to a live Azure test tenant (skipped in CI without credentials)
- Code coverage report at or above 80%
- All compiler warnings resolved
- `ConfigCompare.slnx` solution file referencing all projects

**Exit criteria**: CI pipeline passes; coverage gate met; all integration tests pass against the test tenant.

---

## Summary Timeline (Relative)

| Increment | Parallel? | Relative Duration |
|---|---|---|
| 1 — AzureIdentity | No (blocker) | 1 sprint |
| 2a — ResourceGroup | No | 0.5 sprint |
| 2b — Settings + Session | Yes (parallel with 2a) | 1 sprint |
| 3 — AppConfig + KeyVault | Parallel pair | 1.5 sprints |
| 4 — Comparison | No | 0.5 sprint |
| 5 — UI Shell + Modules | No (largest increment) | 3 sprints |
| 6 — PIM | Parallel with late Increment 5 | 1 sprint |
| 7 — Integration + QA | No | 1 sprint |

**Total critical path**: ~8.5 sprints (assuming 1-week sprints and a single agent/developer per track)
