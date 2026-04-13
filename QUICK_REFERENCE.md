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

| Page | Purpose | Key Actions |
| --- | --- | --- |
| **Configurations** | Load App Config endpoints | Add endpoint, view key-value pairs, remove |
| **Compare** | Compare two loaded configs | Select left/right, view three-column diff |
| **Edit Config** | Create/update configurations | Enter endpoint, key, value, label, save |
| **Find & Replace** | Bulk value replacement | Enter endpoint, find/replace terms, execute |
| **Copy Settings** | Sync between instances | Enter source/target endpoints, copy |
| **Settings** ⚙️ | Preferences & about | Theme (System/Light/Dark), app version |

---

## Keyboard Shortcuts

| Action | Shortcut | Status |
| --- | --- | --- |
| Find & Replace | Ctrl+H | ✅ Implemented |
| Copy Setting | Ctrl+C | ✅ Implemented |
| Paste Setting | Ctrl+V | ✅ Implemented |

---

## Common Tasks

### View Current User
1. Check the **title bar** (top right of the main window)
2. Displays the signed-in user's display name
3. Shows "Not signed in" if authentication fails

### Change Theme
1. Click ⚙️ icon (Settings) at the bottom of the navigation pane
2. Select theme: System default, Light, or Dark
3. Theme applies immediately — no save button required

### Compare Two Configs
1. Go to the **Configurations** page and add both endpoints
2. Click **Compare** in the navigation pane
3. Select left and right configurations from the dropdowns
4. Click "Compare" to view the three-column diff:
   - 🔴 Red: Only in Left
   - 🟢 Green: In Both
   - 🔵 Blue: Only in Right

### Find and Replace Values
1. Click **Find & Replace** in the navigation pane
2. Enter the App Configuration endpoint URL
3. Enter the value to find and its replacement
4. Click "Find & Replace" — results show replacement count and affected keys

### Copy Settings Between Stores
1. Click **Copy Settings** in the navigation pane
2. Enter source and target endpoint URLs
3. Click "Copy Settings" — results show copied key count and list

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

1. **Faster Comparisons**: Load only the endpoints you need to compare
2. **Batch Operations**: Use Find & Replace for bulk value updates
3. **Cached Credentials**: DefaultAzureCredential caches tokens (minimal auth overhead)
4. **Local Sessions**: SQLite cache makes session restore instant
5. **Network**: Connections use Azure SDK connection pooling

---

## Code Structure Quick Guide

```
src/
├── ConfigCompare.AzureIdentity/    → Authentication & tokens
├── ConfigCompare.ResourceGroup/     → Azure resource discovery
├── ConfigCompare.AppConfig/         → App Configuration CRUD
├── ConfigCompare.Settings/          → Local preferences (JSON)
├── ConfigCompare.Session/           → Session persistence (SQLite)
└── ConfigCompare.Desktop/           → WinUI 3 UI application
    └── Pages/
        ├── ConfigurationsPage       → Load & browse endpoints
        ├── ComparePage              → Three-column diff
        ├── EditConfigPage           → Create/update configs
        ├── FindReplacePage          → Bulk find & replace
        ├── CopySettingsPage         → Cross-instance copy
        └── SettingsPage             → Theme & about

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
