# xThemeSong v0.1.1

## What's New

### Maintenance Release 🧹

This release focuses on code quality improvements and cleanup.

## Changes

- **MAINTENANCE**: Comprehensive code review and cleanup of all source files
- **MAINTENANCE**: Removed unused imports and verified all API endpoints
- **MAINTENANCE**: Cleaned up empty directories and unnecessary files
- **ENHANCEMENT**: Verified all web components (plugin.js, xThemeSong.js) are properly integrated
- **ENHANCEMENT**: Updated build script for more reliable builds

## Features

- 🎵 Download theme songs from YouTube by providing video ID or URL
- 📤 Upload your own MP3 files as theme songs
- 🎬 Supports both movies and TV shows
- 📁 Automatically saves theme songs as `theme.mp3` in media folders
- 📝 Stores metadata in `theme.json` files
- ⏰ Scheduled task to process theme songs
- 🎛️ Configuration page in Jellyfin dashboard
- 🔌 File Transformation Plugin integration with fallback support

## Requirements

- **Jellyfin Server**: Version 10.10.0 or later
- **File Transformation Plugin**: **REQUIRED** for Web UI features to work
- **FFmpeg**: Must be installed on your Jellyfin server (usually bundled with Jellyfin)

## Installation

1. Add the plugin repository URL to Jellyfin:
   ```
   https://raw.githubusercontent.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/main/manifest.json
   ```
2. Install "xThemeSong" from the plugin catalog
3. Restart Jellyfin

## Usage

1. Navigate to a movie or TV show
2. Click the **⋮** (three dots) menu
3. Select **"Assign Theme Song"**
4. Enter a YouTube URL or upload an MP3 file
5. Click **"Save Theme Song"**
