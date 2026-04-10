# Configuration Comparison Tool

A comprehensive WPF desktop application for managing, comparing, and synchronizing Azure App Configuration settings with advanced productivity features.

## 🚀 Features

### Core Features

- **Configuration Comparison**: Side-by-side comparison with line-by-line difference detection
- **Application Settings**: Customizable theme, refresh behavior, and notifications
- **Edit & Commit**: Create and update configurations directly in Azure App Configuration
- **Find & Replace**: Bulk find and replace values across multiple configurations
- **Copy Settings**: Synchronize configurations between App Configuration instances
- **Professional UI**: Tab-based navigation with intuitive user interface

### Technical Highlights

- ✅ **Cloud-Native**: Direct integration with Azure App Configuration service
- ✅ **Secure**: DefaultAzureCredential authentication support
- ✅ **Modern Stack**: Built on .NET 10.0 with WPF and MVVM patterns
- ✅ **Well-Tested**: 37 total tests (32 unit + 5 integration), 100% passing
- ✅ **Enterprise-Ready**: Comprehensive error handling and validation

## 📋 System Requirements

- .NET 10.0 SDK
- Windows OS (WPF application)
- Azure subscription with App Configuration resource
- Azure credentials configured (device flow, managed identity, service principal, etc.)

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
├── ConfigCompare.Desktop/       # WPF UI Application
├── ConfigCompare.AppConfig/     # Azure App Configuration Service
├── ConfigCompare.AzureIdentity/ # Azure Authentication
├── ConfigCompare.ResourceGroup/ # Resource Group Operations
├── ConfigCompare.Session/       # Session Management
└── ConfigCompare.Settings/      # Application Settings

tests/
├── unit/                        # Unit Tests (32 tests)
└── integration/                 # Integration Tests
```

## 🎯 Quick Start

1. **Launch Application**: Run the desktop application
2. **Authenticate**: Use DefaultAzureCredential (automatically uses available credentials)
3. **Select Tab**: Choose operation from six available tabs
4. **Enter Endpoint**: Provide Azure App Configuration endpoint URL
5. **Perform Operation**: Execute comparison, edit, find/replace, or copy

## 🔑 Available Operations

### 1. Comparison Tab

- Compare two configuration texts
- Get line-by-line differences
- See total difference count

### 2. Settings Tab

- Select application theme
- Configure refresh behavior
- Enable/disable notifications
- Save or reset to defaults

### 3. Edit Configuration Tab

- Create new configurations
- Update existing configurations
- Add optional labels
- Commit directly to Azure

### 4. Find & Replace Tab

- Search for values across configs
- Replace with new values
- View affected configuration keys
- See replacement count

### 5. Copy Settings Tab

- Sync from source to target instance
- Preserve all metadata
- Validation to prevent source/target confusion
- List all copied keys

### 6. About Tab

- View application version
- See feature overview
- Review copyright information

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
- **ResourceGroupService**: Discovers resource groups and services
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

- `Azure.Data.AppConfiguration` v1.4.1
- `Azure.Identity` v1.13.2
- `CommunityToolkit.Mvvm` v8.3.2
- `CommunityToolkit.Diagnostics` v8.4.0
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

- ✅ WPF desktop application with tab-based UI
- ✅ Configuration comparison engine
- ✅ AppConfig service with CRUD operations
- ✅ Find and replace functionality
- ✅ Copy settings between instances
- ✅ 37 passing tests (32 unit + 5 integration)
- ✅ Modern .NET 10.0 stack
- ✅ Azure authentication integration
