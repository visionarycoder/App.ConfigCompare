# ConfigCompare Desktop Application - Feature Summary

## Overview

ConfigCompare is a WinUI 3 desktop application for managing and comparing Azure App Configuration settings.

## Features Implemented

### 1. **Configurations Page**

- **Purpose**: Load and browse Azure App Configuration stores
- **Capabilities**:
  - Add App Configuration endpoint URLs
  - View all key-value pairs from each loaded endpoint
  - Remove endpoints from the list
  - Side-by-side display of endpoint name, status, and key-value count
- **Use Case**: Browse and monitor configuration entries across environments

### 2. **Compare Page**

- **Purpose**: Compare key-value sets from two loaded configurations
- **Capabilities**:
  - Select left and right configurations from dropdowns (populated from Configurations page)
  - Three-column diff view: "Only in Left", "In Both", "Only in Right"
  - Each column shows key and value pairs
- **Use Case**: Identify differences between two App Configuration stores

### 3. **Settings Page**

- **Purpose**: Customize application appearance and view about information
- **Features**:
  - Theme selection (System Default, Light, Dark) via radio buttons
  - About section with application version
- **Note**: Also accessible via the gear icon at the bottom of the navigation pane

### 4. **Edit Config Page**

- **Purpose**: Create and commit configurations to Azure App Configuration
- **Features**:
  - Input App Configuration endpoint
  - Enter configuration key and value
  - Optional label for configuration
  - Direct commit to Azure App Configuration service
  - Success/error status display
  - Form validation for required fields
- **Technology**: Uses `IAppConfigService` with Azure.Data.AppConfiguration SDK

### 5. **Find & Replace Page**

- **Purpose**: Find and replace values across a configuration store
- **Features**:
  - Specify App Configuration endpoint
  - Enter search term and replacement value
  - Bulk find and replace across all configurations
  - Display affected configuration keys
  - Shows count of replacements made
  - Case-sensitive replacement
- **Technology**: Batch processing via AppConfigService

### 6. **Copy Settings Page**

- **Purpose**: Synchronize settings between App Configuration instances
- **Features**:
  - Enter source and target App Configuration endpoints
  - Copy all settings from source to target
  - Preserves labels, tags, and content types
  - Prevents source/target confusion validation
  - Lists all copied keys
  - Shows copy count
- **Technology**: Cross-instance synchronization via AppConfigService

## Architecture

### Project Structure

```txt
src/
├── ConfigCompare.Desktop/          # WinUI 3 Desktop UI (net10.0-windows)
│   ├── MainWindow.xaml             # Main window with NavigationView
│   ├── MainWindow.xaml.cs          # Event handlers and navigation logic
│   ├── Pages/                      # Page implementations
│   │   ├── ConfigurationsPage      # Load and browse App Config endpoints
│   │   ├── ComparePage             # Three-column diff view
│   │   ├── EditConfigPage          # Create/update configurations
│   │   ├── FindReplacePage         # Find and replace values
│   │   ├── CopySettingsPage        # Copy configs between instances
│   │   └── SettingsPage            # Theme + About
│   └── ViewModels/                 # MVVM ViewModels
│       ├── MainViewModel.cs        # Main app navigation
│       ├── EditConfigViewModel.cs  # Edit config operations
│       ├── FindReplaceViewModel.cs # Find/replace operations
│       └── CopySettingsViewModel.cs# Copy settings operations
├── ConfigCompare.AppConfig/        # Azure App Configuration Service
│   ├── IAppConfigService.cs        # Service interface
│   ├── AppConfigService.cs         # Service implementation
│   └── Resources/                  # DTOs and response models
├── ConfigCompare.AzureIdentity/    # Azure auth and identity
├── ConfigCompare.ResourceGroup/    # Resource group and endpoint discovery
├── ConfigCompare.Session/          # Session management (SQLite)
└── ConfigCompare.Settings/         # Application settings (JSON)

tests/
├── unit/                           # Unit tests (32 tests, all passing)
└── integration/                    # Integration tests (5 tests, all passing)
```

### Service Interfaces

#### IAppConfigService

```csharp
Task<GetConfigurationsResponse> GetConfigurationsAsync(string endpoint, CancellationToken cancellationToken)
Task<UpdateConfigurationResponse> UpdateConfigurationAsync(string endpoint, ConfigurationItemDto configuration, CancellationToken cancellationToken)
Task<UpdateConfigurationResponse> DeleteConfigurationAsync(string endpoint, string key, CancellationToken cancellationToken)
Task<FindReplaceResponse> FindAndReplaceAsync(string endpoint, string findValue, string replaceValue, List<string>? keysToUpdate, CancellationToken cancellationToken)
Task<CopySettingsResponse> CopySettingsAsync(string sourceEndpoint, string targetEndpoint, CancellationToken cancellationToken)
```

## Technical Details

### Dependencies

- **UI Framework**: WinUI 3 via Windows App SDK v1.6
- **Azure SDK**:
  - `Azure.Data.AppConfiguration` v1.4.1
  - `Azure.Identity` v1.13.2 (DefaultAzureCredential for auth)
  - `Azure.ResourceManager` v1.14.0
  - `Azure.ResourceManager.AppConfiguration` v1.3.1
  - `Azure.ResourceManager.KeyVault` v1.4.0
- **Microsoft Toolkit**:
  - `CommunityToolkit.Mvvm` v8.3.2 (MVVM patterns)
  - `CommunityToolkit.Diagnostics` v8.4.0 (Guard parameter validation)
- **Storage**: `Microsoft.Data.Sqlite` v10.0.0 (session persistence)
- **Microsoft Extensions**: Dependency injection support

### Error Handling

- Form validation with user-friendly error messages
- Azure credential exception handling
- Graceful failure responses with detailed error information
- Status display for all operations

## Build Status

✅ **All Systems Operational**

- Build: Successful (0 warnings, 0 errors)
- Tests: 37/37 passing (32 unit + 5 integration)
- Framework: .NET 10.0
- IDE: Visual Studio Code / Visual Studio

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- Windows 10 or later
- Azure subscription with App Configuration resource
- Azure credential setup (Default Azure Credential flow)

### Build and Run

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Run desktop application
dotnet run --project src/ConfigCompare.Desktop/ConfigCompare.Desktop.csproj
```

### Configuration

- Set Azure App Configuration endpoint in each page
- Authenticate using DefaultAzureCredential (Azure CLI, managed identity, etc.)
- All operations are read-your-writes consistent within Azure App Configuration

## Future Enhancements

- Batch configuration file import/export
- Configuration history and audit logging
- Advanced filtering in find/replace
- Scheduled copy operations
- Configuration version control
- Role-based access control (RBAC) enforcement
