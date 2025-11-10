# xThemeSong v0.0.10.0 Release

## Overview
This release focuses on improving compatibility with the File Transformation plugin and enhancing the overall reliability of script injection for the Web UI.

## What's New

### 🔧 File Transformation Plugin Integration
- **Fixed Registration Issues**: Implemented proper service pattern for File Transformation plugin registration
- **Enhanced Retry Logic**: Added robust retry mechanisms with exponential backoff
- **Fallback Registration**: Added StartupTask as a fallback registration method
- **Improved Compatibility**: Better compatibility with File Transformation plugin 2.4.2.0+

### 🏗️ Code Improvements
- **Namespace Update**: Changed from `Jellyfin.Plugin.AssignThemeSong` to `Jellyfin.Plugin.xThemeSong`
- **Better Error Handling**: Comprehensive error handling and logging throughout the registration process
- **Reflection-Based Registration**: Using reflection for dynamic plugin discovery and registration

### 🔄 Build & Deployment
- **Version Bump**: Updated to version 0.0.10.0
- **Clean Build Process**: Removed unnecessary build artifacts
- **Proper Packaging**: Complete release package with all required dependencies

## Technical Details

### File Transformation Integration
The plugin now properly integrates with the File Transformation plugin using:
- Reflection-based dynamic registration
- Multiple retry attempts with increasing delays
- Comprehensive error logging for troubleshooting
- Fallback registration via scheduled task

### Dependencies
- **Target Framework**: .NET 8.0
- **Jellyfin ABI**: 10.10.0.0
- **Required Plugin**: File Transformation Plugin (for Web UI functionality)

## Installation

1. Download the `xThemeSong_0.0.10.0.zip` file
2. Install via Jellyfin Plugin Catalog or manually extract to your plugins directory
3. Ensure File Transformation Plugin is installed for Web UI functionality
4. Restart Jellyfin server

## Known Issues
- None reported in this release

## Changelog
- Fixed File Transformation plugin registration by implementing proper service pattern
- Updated to use HTTP client for registration with retry logic
- Improved reliability and compatibility with File Transformation plugin 2.4.2.0

## Support
For issues or questions, please visit the [GitHub repository](https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong).

---
**MD5 Checksum**: `BFA3DE1F35DCC29CBFC19EE3EEC904D0`
**Release Date**: November 10, 2025
