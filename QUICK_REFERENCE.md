# ConfigCompare Quick Reference Card

## Launch Application

```powershell
# Debug mode
dotnet run --project src/ConfigCompare.Desktop

# Release mode  
dotnet run --project src/ConfigCompare.Desktop --configuration Release

# Executable (after publish)
./publish/ConfigCompare.Desktop.exe
```

---

## Navigation Map

| Tab | Purpose | Key Actions |
| --- | --- | --- |
| **Resource Groups** | Browse Azure subscriptions | Select RG, view resources |
| **App Config** | Manage configurations | Create, read, update, delete entries |
| **Key Vault** | Manage secrets | View, rotate, access secrets |
| **Comparison** | Compare configs | Side-by-side diff view |
| **PIM** | Activate roles | Request access, set duration |
| **Settings** ⚙️ | Preferences | Theme, auto-refresh, about |

---

## Keyboard Shortcuts

| Action | Shortcut | Status |
| --- | --- | --- |
| Close Settings Popup | Esc | 🚀 Future |
| Find & Replace | Ctrl+H | ✅ Implemented |
| Copy Setting | Ctrl+C | ✅ Implemented |
| Paste Setting | Ctrl+V | ✅ Implemented |

---

## Common Tasks

### View Current User
1. Check **footer** of main window
2. Displays: "[User Display Name] (UPN)"
3. Updated when authentication refreshes

### Change Theme
1. Click ⚙️ icon (Settings)
2. Select theme: Light, Dark, System Default
3. Click Save
4. Theme applies immediately

### Compare Two Configs
1. Click **Comparison** tab
2. Select "Source" App Config or Key Vault
3. Select "Target" App Config or Key Vault
4. View color-coded differences:
   - 🟢 Green: Only in Target
   - 🔴 Red: Only in Source
   - 🟡 Yellow: Changed values
   - ⚪ White: Identical

### Find All Occurrences
1. Click **App Config** or **Key Vault** tab
2. Press Ctrl+H or use Find menu
3. Enter search term
4. All matches highlighted
5. Click "Replace All" (or individual replace)

### Activate a PIM Role
1. Click **PIM** tab
2. View "Eligible Roles" section
3. Click "Activate" button on desired role
4. Enter justification (why needed)
5. Select duration (e.g., 4 hours)
6. Click "Submit"
7. Status updates when approved

---

## Build & Test Commands

```powershell
# Build
dotnet build                              # Debug build
dotnet build --configuration Release      # Release build

# Test
dotnet test                               # Run all tests
dotnet test --verbosity=detailed          # Verbose output
dotnet test --collect:"XPlat Code Coverage"  # With coverage

# Clean
dotnet clean                              # Remove build artifacts

# Publish
dotnet publish --configuration Release --output ./publish
```

---

## Azure CLI Quick Commands

```powershell
# Login
az login
az account show                           # Current account
az account set --subscription "sub-id"    # Switch subscription

# Resource Groups
az group list                             # List RGs
az group show --name "rg-name"           # RG details

# App Configuration
az appconfig list --resource-group "rg"  # List stores
az appconfig kv list --name "store-name" # List entries

# Key Vault
az keyvault list --resource-group "rg"   # List vaults
az keyvault secret list --vault-name "name"  # List secrets
```

---

## Environment Variables

```powershell
# Service Principal Authentication
$env:AZURE_TENANT_ID = "00000000-0000-0000-0000-000000000000"
$env:AZURE_CLIENT_ID = "00000000-0000-0000-0000-000000000000"
$env:AZURE_CLIENT_SECRET = "your-secret-value"

# For Integration Tests
$env:AZURE_SUBSCRIPTION_ID = "00000000-0000-0000-0000-000000000000"
$env:AZURE_TEST_RESOURCE_GROUP = "test-rg-name"
```

---

## File Locations

| Item | Location |
| --- | --- |
| **Settings** | `%LOCALAPPDATA%\ConfigCompare\settings.json` |
| **Sessions** | `%LOCALAPPDATA%\ConfigCompare\sessions.db` |
| **Source Code** | `./src/` |
| **Tests** | `./tests/unit/` and `./tests/integration/` |
| **Documentation** | `./docs/` |
| **Plans** | `./docs/agent-plans/` |
| **Designs** | `./docs/component-designs/` |

---

## Error Messages & Solutions

| Error | Cause | Solution |
| --- | --- | --- |
| "Not authenticated" | No Azure credentials | Run `az login` |
| "Resource not found" | Invalid resource name | Verify in Azure Portal |
| "Access denied" | Insufficient permissions | Review RBAC roles |
| "Timeout" | Network latency | Retry operation |
| "Theme not applied" | Display refresh issue | Restart application |

---

## Performance Tips

1. **Faster Comparisons**: Use filters to reduce dataset size
2. **Batch Operations**: Use Find & Replace for bulk updates
3. **Cool Credentials**: DefaultAzureCredential caches tokens (minimal auth overhead)
4. **Local Sessions**: SQLite cache makes session restore instant
5. **Network**: Connections use Azure SDK connection pooling

---

## Code Structure Quick Guide

```
src/
├── ConfigCompare.AzureIdentity/    → Authentication & tokens
├── ConfigCompare.ResourceGroup/     → Azure resource discovery
├── ConfigCompare.AppConfig/         → App Configuration CRUD
├── ConfigCompare.KeyVault/          → Key Vault secrets access
├── ConfigCompare.Comparison/        → Diff engine (pure logic)
├── ConfigCompare.Settings/          → Local preferences
├── ConfigCompare.Session/           → Session persistence (SQLite)
├── ConfigCompare.Pim/               → PIM role activation
└── ConfigCompare.Desktop/           → WPF UI application

tests/
├── unit/                            → 32 unit tests (fast, local)
└── integration/                     → 5 integration tests (live Azure)
```

---

## Test Categories

| Category | Count | Time | Requires Azure |
| --- | --- | --- | --- |
| Unit Tests | 32 | ~1 sec | No ❌ |
| Integration Tests | 5 | ~12 sec | Yes ✅ |
| **Total** | **37** | **~13 sec** | Mixed |

---

## Support Resources

- **GitHub**: https://github.com/visionarycoder/App.ConfigCompare
- **Issues**: Report bugs on GitHub Issues tab
- **Docs**: See `/docs` folder for detailed information
- **Plans**: See `/docs/agent-plans` for implementation details
- **Designs**: See `/docs/component-designs` for architecture

---

*Quick Reference*  
*ConfigCompare v1.0.0*  
*Last Updated: Current Session*
