# Release v0.0.3.0 - Jellyfin Assign Theme Song Plugin (Alpha)

## What's New

This alpha release focuses on improving File Transformation plugin integration based on studying the Jellyfin-Enhanced plugin's implementation pattern.

## Key Changes

### File Transformation Plugin Integration
- **Method Signature Update**: Changed `TransformationPatches.PatchIndexHtml` to `IndexHtml` with proper parameter types
- **JObject Payload**: Updated to use direct JObject construction instead of JSON string parsing
- **Script URL**: Fixed script URL path from `/AssignThemeSong/script` to `../AssignThemeSong/script`
- **Return Type**: Now returns string directly instead of JObject wrapper

### Code Structure Improvements
- Cleaned up logging and console output in StartupTask
- Improved method signatures to match Jellyfin-Enhanced plugin patterns
- Enhanced error handling and logging

### Reference Plugin Studied
- **Jellyfin Enhanced**: https://github.com/n00bcodr/Jellyfin-Enhanced
  - File Transformation registration pattern
  - Method signatures and parameter types
  - JObject payload construction

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
- **Zip Archive Checksum**: 6ECBFD886EE2C9DF8B930B4F9EE18224
- **Build Platform**: .NET 8.0
- **Dependencies**: YoutubeExplode, Newtonsoft.Json

## Known Issues (Alpha)
- This is an alpha development version
- Some nullable reference warnings present
- Missing XML documentation for some public members

## Next Steps
- Continue testing and bug fixes
- Add more configuration options
- Improve error handling and user feedback
- Consider additional audio source integrations

## Support
For issues and questions, please visit the [GitHub repository](https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong) or the [Jellyfin Forum](https://forum.jellyfin.org/).

---
*Alpha development version - File Transformation integration improved based on Jellyfin-Enhanced plugin patterns.*
