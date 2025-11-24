# xThemeSong v0.1.0 Release

## Overview
This release includes critical fixes for assembly loading conflicts and improved error handling, particularly around theme song uploads and downloads.

## What's New

### Critical Fixes
- **Fixed assembly loading conflicts**: Resolved issues with System.Text.Encodings.Web and other system libraries that prevented the plugin from loading properly in some Jellyfin installations
- **Aligned dependency versions**: YoutubeExplode now uses version 6.3.16.0 exactly, which matches Jellyfin's expectations and prevents version conflicts

### Improvements
- **Enhanced error handling**: Added robust error handling in the API controller for file uploads, ensuring better reliability when uploading MP3 files
- **Improved logging**: Added comprehensive logging throughout the plugin to make troubleshooting easier
- **Streamlined build process**: Modified the cleanup process to ensure only compatible assemblies are included in the plugin package

### Technical Details
- Fixed parameter mismatches between API controller and service methods
- Improved cleanup of temporary files when uploading MP3s
- Increased logging for better diagnostics in server logs
- Updated assembly versioning to ensure consistency across all plugin components

## Installation

1. Download the ZIP file from this release
2. Install it using Jellyfin's plugin manager or extract to your Jellyfin plugins directory:
   - Windows: `%AppData%\Jellyfin\Server\plugins\`
   - Linux: `/var/lib/jellyfin/plugins/`
   - Docker: `/config/plugins/`
3. Restart Jellyfin

## Requirements
- Jellyfin Server v10.10.0 or later
- [File Transformation Plugin](https://github.com/IAmParadox27/jellyfin-plugin-file-transformation) (recommended for Web UI features)

## Known Issues
- None at this time. Please report any problems you encounter.
