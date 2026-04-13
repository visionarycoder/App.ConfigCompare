# Configuration Comparison Tool

A WinUI 3 desktop application for managing, comparing, and synchronizing Azure App Configuration settings.

## 🚀 Features

### Core Features

- **Configurations**: Load and browse key-value pairs from Azure App Configuration endpoints
- **Configuration Comparison**: Side-by-side comparison showing keys only in left, keys in both, and keys only in right
- **Edit & Commit**: Create and update configurations directly in Azure App Configuration
- **Find & Replace**: Bulk find and replace values across a configuration store
- **Copy Settings**: Synchronize configurations between App Configuration instances
- **Modern UI**: NavigationView with left pane navigation

### Technical Highlights

- ✅ **Cloud-Native**: Direct integration with Azure App Configuration service
- ✅ **Secure**: DefaultAzureCredential authentication support
- ✅ **Modern Stack**: Built on .NET 10.0 with WinUI 3 (Windows App SDK) and MVVM patterns
- ✅ **Well-Tested**: 37 total tests (32 unit + 5 integration), 100% passing
- ✅ **Enterprise-Ready**: Comprehensive error handling and validation

## 📋 System Requirements

- .NET 10.0 SDK
- Windows 10 or later (WinUI 3 application)
- Azure subscription with App Configuration resource
- Azure credentials configured (Azure CLI, managed identity, service principal, etc.)

## 🛠️ Building

```bash
# Clone repository
git clone https://github.com/visionarycoder/App.ConfigCompare.git
cd App.ConfigCompare

# Build solution
dotnet build

# Run all tests
dotnet test

# Run application
dotnet run --project src/ConfigCompare.Desktop/ConfigCompare.Desktop.csproj
```

## 📦 Project Structure

```txt
src/
├── ConfigCompare.Desktop/       # WinUI 3 UI Application
├── ConfigCompare.AppConfig/     # Azure App Configuration Service
├── ConfigCompare.AzureIdentity/ # Azure Authentication
├── ConfigCompare.ResourceGroup/ # Resource Group Operations
├── ConfigCompare.Session/       # Session Management
└── ConfigCompare.Settings/      # Application Settings

tests/
├── unit/                        # Unit Tests (32 tests)
└── integration/                 # Integration Tests (5 tests)
```

## 🎯 Quick Start

1. **Launch Application**: Run the desktop application
2. **Authenticate**: Use DefaultAzureCredential (automatically uses available credentials)
3. **Select Page**: Choose an operation from the left navigation pane
4. **Enter Endpoint**: Provide Azure App Configuration endpoint URL
5. **Perform Operation**: Execute comparison, edit, find/replace, or copy

## 🔑 Available Pages

### 1. Configurations Page

- Add one or more Azure App Configuration endpoint URLs
- Browse and view the key-value pairs loaded from each endpoint
- Remove endpoints from the list

### 2. Compare Page

- Select two loaded configurations from dropdown menus
- View a three-column diff: keys only in left, keys in both, keys only in right

### 3. Edit Config Page

- Create new configurations
- Update existing configurations
- Add optional labels
- Commit directly to Azure

### 4. Find & Replace Page

- Search for values across a config store
- Replace with new values
- View affected configuration keys
- See replacement count

### 5. Copy Settings Page

- Sync from source to target instance
- Preserve all metadata
- Validation to prevent source/target confusion
- List all copied keys

### 6. Settings Page

- Select application theme (Light, Dark, System Default)
- View application version and about information

## 🔐 Authentication

The application uses **DefaultAzureCredential** which automatically tries:

1. Environment variables
2. Managed Identity (if running in Azure)
3. Visual Studio Azure credentials
4. Visual Studio Code Azure credentials
5. Device flow (interactive login)

No explicit configuration needed!

## 🏗️ Architecture

### Services

- **AppConfigService**: Provides CRUD operations and advanced features on App Configuration
- **AuthService**: Handles Azure authentication
- **ResourceGroupService**: Discovers resource groups and App Configuration/Key Vault endpoints
- **SessionService**: Manages user sessions
- **SettingsService**: Manages application settings

### Data Models

- ConfigurationItemDto: Represents a configuration setting
- GetConfigurationsResponse: Response for retrieving configurations
- UpdateConfigurationResponse: Response for create/update/delete operations
- FindReplaceResponse: Response for find and replace operations
- CopySettingsResponse: Response for copy settings operations

## ✅ Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/unit/ConfigCompare.AzureIdentity.Service.Tests.Unit/

# Run with verbose output
dotnet test --verbosity=detailed
```

**Current Status**: 37/37 tests passing ✅ (32 unit + 5 integration)

## 📚 Dependencies

### NuGet Packages

- `Microsoft.WindowsAppSDK` v1.6.250228002
- `Azure.Data.AppConfiguration` v1.4.1
- `Azure.Identity` v1.13.2
- `Azure.ResourceManager` v1.14.0
- `Azure.ResourceManager.AppConfiguration` v1.3.1
- `Azure.ResourceManager.KeyVault` v1.4.0
- `CommunityToolkit.Mvvm` v8.3.2
- `CommunityToolkit.Diagnostics` v8.4.0
- `Microsoft.Data.Sqlite` v10.0.0
- `Microsoft.Extensions.DependencyInjection` v10.0.0

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests to ensure everything works
5. Submit a pull request

## 📄 License

[Add your license here]

## 📞 Support

For issues, questions, or suggestions, please create an issue on GitHub.

## 🔄 Changelog

### v1.0.0 (Latest)

- ✅ WinUI 3 desktop application with NavigationView-based UI
- ✅ Configurations page to load and browse App Configuration endpoints
- ✅ Configuration comparison engine (three-column diff view)
- ✅ AppConfig service with CRUD operations
- ✅ Find and replace functionality
- ✅ Copy settings between instances
- ✅ 37 passing tests (32 unit + 5 integration)
- ✅ Modern .NET 10.0 stack
- ✅ Azure authentication integration
