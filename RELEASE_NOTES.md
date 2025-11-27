# xThemeSong v0.1.2

## What's New

### CRITICAL FIX 🔧

This release fixes a critical plugin loading issue that prevented the plugin from initializing on Jellyfin 10.11.x servers.

## Changes

- **CRITICAL FIX**: Removed conflicting Jellyfin host assemblies from build output
  - The plugin was bundling Jellyfin 10.10 assemblies (Jellyfin.Data, MediaBrowser.Common, MediaBrowser.Controller, MediaBrowser.Model, etc.) which caused version conflicts when running on Jellyfin 10.11.x servers
  - Plugin now properly relies on host-provided assemblies
- **FIX**: Improved build script to properly clean up unnecessary DLLs
- **FIX**: Corrected manifest.json format to array of plugins (required by Jellyfin plugin repository system)

## Plugin Contents

The plugin zip now contains only essential files:
- `Jellyfin.Plugin.xThemeSong.dll` - Main plugin assembly
- `YoutubeExplode.dll` - YouTube download functionality
- `AngleSharp.dll` - Required dependency for YoutubeExplode
- `meta.json` - Plugin metadata
- `web/` - JavaScript files for UI integration

## Requirements

- **Jellyfin Server**: Version 10.10.0 or later
- **File Transformation Plugin**: **REQUIRED** for Web UI features to work

## Installation

1. Add the plugin repository URL to Jellyfin:
   ```
   https://raw.githubusercontent.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/main/manifest.json
   ```
2. Install "xThemeSong" from the plugin catalog
3. Restart Jellyfin
