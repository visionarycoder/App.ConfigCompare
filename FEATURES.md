# ConfigCompare Desktop Application - Feature Summary

## Overview

ConfigCompare is a WPF desktop application for managing and comparing Azure App Configuration settings with enhanced productivity features.

## Features Implemented

### 1. **Configuration Comparison Tab**

- **Purpose**: Side-by-side comparison of two configuration strings
- **Capabilities**:
  - Input source and target configuration texts
  - Line-by-line difference detection
  - Display total difference count
  - Clear form for new comparison
- **Use Case**: Quickly identify configuration changes between environments

### 2. **Application Settings Tab**

- **Purpose**: Customize application behavior
- **Features**:
  - Theme selection (Light, Dark, System Default)
  - Auto-refresh toggle
  - Detailed differences display toggle
  - Notification settings
  - Save and reset functionality

### 3. **About Tab**

- **Purpose**: Display application information and feature overview
- **Content**: Version, copyright, and comprehensive feature list

### 4. **Edit Configuration Tab** ✅ NEW

- **Purpose**: Create and commit configurations to Azure App Configuration
- **Features**:
  - Input App Configuration endpoint
  - Enter configuration key and value
  - Optional label for configuration
  - Direct commit to Azure App Configuration service
  - Success/error status display
  - Form validation for required fields
- **Technology**: Uses `IAppConfigService` with Azure.Data.AppConfiguration SDK

### 5. **Find & Replace Tab** ✅ NEW

- **Purpose**: Find and replace values across multiple configurations
- **Features**:
  - Specify App Configuration endpoint
  - Enter search term and replacement value
  - Bulk find and replace across all configurations
  - Display affected configuration keys
  - Shows count of replacements made
  - Case-sensitive replacement
- **Technology**: Batch processing via AppConfigService

### 6. **Copy Settings Tab** ✅ NEW

- **Purpose**: Synchronize settings between App Configuration instances
- **Features**:
  - Select source and target App Configuration endpoints
  - Copy all settings from source to target
  - Preserves labels, tags, and content types
  - Prevents source/target confusion validation
  - Lists all copied keys with pagination
  - Shows copy count
- **Technology**: Cross-instance synchronization via AppConfigService

## Architecture

### Project Structure

```txt
src/
├── ConfigCompare.Desktop/          # WPF Desktop UI (net10.0-windows)
│   ├── MainWindow.xaml             # Main UI with 6 tabs
│   ├── MainWindow.xaml.cs          # Event handlers and UI logic
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
├── ConfigCompare.ResourceGroup/    # Resource group operations
├── ConfigCompare.Session/          # Session management
└── ConfigCompare.Settings/         # Application settings

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

- **WPF Framework**: Windows Presentation Foundation for desktop UI
- **Azure SDK**:
  - `Azure.Data.AppConfiguration` v1.4.1
  - `Azure.Identity` v1.13.2 (DefaultAzureCredential for auth)
- **Microsoft Toolkit**:
  - `CommunityToolkit.Mvvm` v8.3.2 (MVVM patterns)
  - `CommunityToolkit.Diagnostics` v8.4.0 (Guard parameter validation)
- **Microsoft Extensions**: Dependency injection support

### UI Styling

- Color scheme: Professional dark header (#2E2E2E) with light content area
- Button colors: Blue (#007ACC) for primary actions, yellow (#FFC107) for find/replace, green (#28A745) for copy
- Responsive layout with proper spacing and font sizing
- Status messages with color coding (green for success, red for errors)

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

- Set Azure App Configuration endpoint in each tab
- Authenticate using DefaultAzureCredential (device flow, managed identity, etc.)
- All operations are read-your-writes consistent within Azure App Configuration

## Future Enhancements

- Batch configuration file import/export
- Configuration history and audit logging
- Advanced filtering in find/replace
- Scheduled copy operations
- Configuration version control
- Role-based access control (RBAC) enforcement
