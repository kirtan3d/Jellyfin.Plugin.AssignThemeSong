# Release v0.0.2.0 - Jellyfin Assign Theme Song Plugin

## What's New

This release focuses on improving the plugin structure and build process based on studying established Jellyfin plugin patterns.

## Changes

### Code Structure Improvements
- Updated plugin class structure to match Jellyfin plugin template standards
- Fixed code structure issues identified from comparing with reference plugins
- Improved plugin initialization and registration patterns
- Enhanced build configuration and cleanup process

### Build and Packaging
- Updated version to 0.0.2.0
- Added proper MD5 checksum verification for the plugin assembly
- Cleaned up unnecessary files and build artifacts
- Improved project configuration for better compatibility

### Reference Plugins Studied
- **Jellyfin Plugin Template**: https://github.com/jellyfin/jellyfin-plugin-template
- **File Transformation Plugin**: https://github.com/IAmParadox27/jellyfin-plugin-file-transformation  
- **Jellyfin Enhanced**: https://github.com/n00bcodr/Jellyfin-Enhanced

## Installation

### Prerequisites
- Jellyfin Server 10.10.0 or later
- File Transformation Plugin (required for Web UI features)
- FFmpeg (usually bundled with Jellyfin)

### Installation Methods
1. **From Jellyfin Plugin Catalog** (recommended)
   - Add manifest.json URL to Jellyfin plugin repositories
   - Install from Jellyfin's plugin page

2. **Manual Installation**
   - Download the release zip file
   - Extract to Jellyfin plugins directory
   - Restart Jellyfin

## Features
- Download theme songs from YouTube using video ID/URL
- Upload custom MP3 files as theme songs
- Support for both movies and TV shows
- Complete Web UI with drag-and-drop upload
- Scheduled task for batch processing
- Configuration page in Jellyfin dashboard

## Technical Details
- **Target ABI**: 10.10.0.0
- **Assembly Checksum**: DECF57C25A6E1E81B063BBB711D1DE18
- **Build Platform**: .NET 8.0
- **Dependencies**: YoutubeExplode, Newtonsoft.Json

## Next Steps
- Continue testing and bug fixes
- Add more configuration options
- Improve error handling and user feedback
- Consider additional audio source integrations

## Support
For issues and questions, please visit the [GitHub repository](https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong) or the [Jellyfin Forum](https://forum.jellyfin.org/).

---
*Built with reference to established Jellyfin plugin patterns for better compatibility and maintainability.*
