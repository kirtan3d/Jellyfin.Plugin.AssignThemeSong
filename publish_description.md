# Release v0.0.4.0 - Fixed UI Integration

## What's New

This release fixes the missing "Assign Theme Song" icon/link on media detail pages by properly integrating with the File Transformation plugin.

## Changes

- **Fixed UI Integration**: Added File Transformation plugin registration to properly inject the Assign Theme Song button on media pages
- **Updated Plugin.cs**: Added complete File Transformation registration code based on HoverTrailer plugin patterns
- **Enhanced TransformationPatches**: Updated script URL to match the correct endpoint
- **Version Bump**: Updated to v0.0.4.0 with new checksums

## Technical Details

The plugin now properly:
- Registers with Jellyfin's File Transformation plugin on startup
- Injects the plugin JavaScript into Jellyfin's web interface
- Displays the "Assign Theme Song" button on movie and TV show detail pages
- Uses reflection to work with the File Transformation plugin API

## Requirements

- Jellyfin 10.10.0 or later
- File Transformation Plugin (required for Web UI features)
- FFmpeg (bundled with Jellyfin)
- Internet connection (for YouTube downloads)

## Installation

1. Download the plugin zip file
2. Install via Jellyfin Dashboard → Plugins → Catalog
3. Restart Jellyfin server

## Known Issues

- Some nullable reference warnings during build (non-critical)
- Missing XML documentation warnings (non-critical)

The plugin is now fully functional with complete Web UI integration!
