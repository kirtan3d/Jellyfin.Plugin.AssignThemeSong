# Release v0.0.9.1 - Improved Plugin Integration

## Summary
This release focuses on fixing assembly version mismatches and improving integration with the File Transformation plugin for more reliable Web UI functionality.

## Key Changes

### Bug Fixes
- **Fixed Assembly Version Mismatch**: Corrected DLL version from 0.0.8.0 to 0.0.9.0
- **Enhanced Retry Mechanism**: Improved plugin loading order handling with 6 attempts × 3-second delays

### Improvements
- **File Transformation Plugin Integration**: Updated callback method signatures to match File Transformation expectations
- **Base Path Handling**: Proper URL generation for script injection from network configuration
- **Code Structure**: Cleaned up plugin initialization patterns based on reference plugins

### Technical Details
- **Updated Version**: 0.0.9.1 in all project files
- **MD5 Checksum**: 0ECCDF8DAA7AD74E0F269E91263320B2
- **Target ABI**: 10.10.0.0

## Installation Notes
- **Prerequisite**: File Transformation plugin must be installed for Web UI features
- **Compatibility**: Jellyfin Server 10.10.0 or later
- **Dependencies**: FFmpeg required for audio processing

## Files Updated
- `Jellyfin.Plugin.AssignThemeSong.csproj` - Version updates
- `manifest.json` - Updated checksum and version info
- `Plugin.cs` - Cleaned up initialization code
- `Helpers/TransformationPatches.cs` - Improved callback signatures
- `README.md` - Updated documentation

## References
- File Transformation Plugin: https://github.com/IAmParadox27/jellyfin-plugin-file-transformation
- HoverTrailer Plugin: https://github.com/Fovty/HoverTrailer
