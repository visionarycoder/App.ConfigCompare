# Settings Popup Feature Documentation

## Overview
ConfigCompare now includes a convenient settings popup that provides quick access to application preferences and information without navigating away from the main workspace.

## Accessing Settings

### Opening the Settings Popup
1. Look for the **⚙️ (settings icon)** at the bottom of the left navigation bar
2. Click the icon to open the hovering settings popup
3. The popup appears as a centered modal overlay with a semi-transparent dark background

### Closing the Settings Popup
- Click the **✕ (close button)** in the top-right corner of the popup
- Click anywhere outside the white popup area (on the dark background)
- Press Escape key (future enhancement)

## Settings Tab

The Settings tab contains application preferences that can be customized:

### Theme Selection
- **Light**: Bright theme for daytime use
- **Dark**: Dark theme for reduced eye strain
- **System Default**: Matches your Windows system theme preference

### Preferences Checkboxes
- **Auto-refresh on changes**: Automatically update displayed data when configurations change
- **Show detailed differences**: Display comprehensive diff information in comparisons
- **Enable notifications**: Show system notifications for important events

### Actions
- **Save**: Persists your preference changes
- **Reset**: Reverts all settings to default values

## About Tab

The About tab displays comprehensive information about the application:

### Application Information
- **Application Name**: Configuration Comparison Tool
- **Version**: Semantic version number (e.g., 1.0.0 or 1.0.1)
- **Build Date**: Date and time when the application was built

### About Section
Description of ConfigCompare and its purpose for managing Azure configuration settings.

### Key Features
- Side-by-side configuration comparison with detailed diff view
- Native Azure App Configuration integration
- Find and replace values across configs with bulk operations
- Copy settings between instances with metadata preservation
- Customizable application settings and preferences

### System Requirements
- Windows 10 or later
- .NET 10.0 Runtime or later
- Azure subscription (for configuration operations)

### Getting Started Guide
Step-by-step instructions for:
1. Configuring Azure credentials
2. Entering App Configuration endpoints
3. Using comparison and synchronization features

### Support & Resources
- Documentation: Available at the project repository
- Report Issues: Submit feedback and bug reports on GitHub
- License: MIT License - See LICENSE file for details

## Design Details

### Popup Overlay Design
- **Background**: Semi-transparent dark overlay (#80000000) - allows you to see the main application behind the popup
- **Content Area**: White rounded rectangle with professional styling
- **Position**: Centered on screen with fixed margins
- **Size**: Maximum width of 500px for readability
- **Scrollable**: About tab content scrolls if it exceeds the visible area

### Event Handling
- **Click Detection**: Uses VisualTreeHelper for precise click location detection
- **Smart Dismissal**: Distinguishes between clicks on content vs. background
- **Dynamic Content**: Version and build date update automatically based on assembly metadata

## Features

### Semantic Versioning
- Application uses semantic versioning format: Major.Minor.Build
- Build number auto-increments with each compilation
- Version is displayed dynamically from assembly metadata
- No manual version updates needed

### Responsive Content
- About section scrolls independently if content exceeds available space
- Proper typography and line spacing for optimal readability
- Color-coded text hierarchy for better information scanning

## Technical Implementation

### XAML Structure
- Settings popup is implemented as a Grid overlay with TabControl
- Uses WPF binding and event handlers for interaction
- Proper z-index ensures popup appears above main content

### Code-Behind Logic
- `SettingsIconButton_Click`: Opens the popup
- `SettingsPopupOverlay_MouseDown`: Handles click-outside detection
- `CloseSettingsButton_Click`: Closes the popup
- `DisplayVersionInfo()`: Updates version and build date at runtime

## Future Enhancements
Potential improvements for future versions:
- Keyboard shortcuts (Esc to close)
- Theme application across entire application
- Preference persistence to disk
- Check for updates feature
- Advanced settings tab with more options
