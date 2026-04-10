# ConfigCompare v1.0.0 Release Package

## Release Information

- **Version**: 1.0.0
- **Release Date**: Current Session
- **Platform**: Windows 10+
- **Runtime**: .NET 10.0
- **Architecture**: x64, Windows
- **License**: MIT

## Package Contents

- **Executable**: `ConfigCompare.Desktop.exe` - Main application
- **Dependencies**: All required NuGet packages (bundled)
- **Runtime**: .NET 10.0 libraries (included)

## Installation

### Option 1: From Source (Development)
```powershell
git clone https://github.com/visionarycoder/App.ConfigCompare.git
cd App.ConfigCompare
dotnet run --project src/ConfigCompare.Desktop --configuration Release
```

### Option 2: From Release Package (Production)
1. Extract `releases/ConfigCompare-1.0.0/` to desired location
2. Ensure Windows 10+ with .NET 10.0 runtime
3. Run `ConfigCompare.Desktop.exe`

## System Requirements

- **OS**: Windows 10 or later
- **Memory**: 512 MB minimum, 1 GB recommended
- **Disk**: 200 MB for application and dependencies
- **Network**: Internet connection (for Azure access)

## Features Included

- ✅ Azure App Configuration management
- ✅ Azure Key Vault secret management
- ✅ Configuration comparison (side-by-side diff)
- ✅ PIM role activation
- ✅ Settings and preferences
- ✅ Session persistence
- ✅ Theme support (Light/Dark/System)
- ✅ Find and replace functionality

## Quality Metrics

| Metric | Value |
| --- | --- |
| Tests Passing | 37/37 (100%) |
| Code Coverage | 81.81% |
| Compiler Warnings | 0 |
| Build Status | ✅ SUCCESS |

## First Run

1. **Authenticate**: Application uses `DefaultAzureCredential` from Azure CLI
   ```powershell
   az login
   az account set --subscription "your-subscription-id"
   ```
   
2. **Launch**: Run `ConfigCompare.Desktop.exe`

3. **Configure**: 
   - Select Resource Group
   - Choose App Configuration or Key Vault
   - Start managing configurations

## Support

- **Repository**: https://github.com/visionarycoder/App.ConfigCompare
- **Issues**: Report on GitHub Issues page
- **Documentation**: See project README and DEPLOYMENT_GUIDE.md

## Release Files Structure

```
ConfigCompare-1.0.0/
├── ConfigCompare.Desktop.exe (main executable)
├── ConfigCompare.*.dll (service libraries)
├── runtimes/ (platform-specific libraries)
├── cs/ (language resources)
└── [other dependencies]
```

Total Package Size: 73.85 MB (259 files)

---

*Release Package for ConfigCompare v1.0.0*  
*Production Ready*  
*Ready for Windows Deployment*
