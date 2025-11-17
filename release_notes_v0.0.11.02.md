# xThemeSong v0.0.11.02 Release Notes

## CRITICAL BUG FIXES - Complete UI & YouTube Download Fixes

This release resolves all remaining critical issues that were preventing the plugin from working correctly in production environments.

### 🚨 Critical Issues Fixed

#### 1. **Backdrop Overlay Staying Open**
- **Problem**: Black overlay (`dialogBackdropOpened`) remained visible after clicking "Assign Theme Song"
- **Root Cause**: Incomplete backdrop removal logic only targeted `.backdrop` classes
- **Fix**: Enhanced action sheet closing to remove ALL backdrop types:
  - `.backdrop`, `.backdropFadeIn`, `.dialogBackdrop`, `.dialogBackdropOpened`
  - `.dialogContainer` and `.focuscontainer.dialog` elements
- **Result**: No more black overlays staying on screen

#### 2. **Dialog System Falling Back to Simple Mode**
- **Problem**: Dialog was using fallback mode even when Jellyfin dialog system was available
- **Root Cause**: Incomplete dialog system detection
- **Fix**: Enhanced detection with multiple verification methods:
  - Global `window.dialogHelper`
  - RequireJS `dialogHelper` module
  - Jellyfin environment checks (`window.Emby`)
- **Result**: Proper Jellyfin dialogs now open consistently

#### 3. **YouTube Cipher Manifest Error**
- **Problem**: "Could not get cipher manifest" error preventing YouTube downloads
- **Root Cause**: Outdated YoutubeExplode library (v6.3.9)
- **Fix**: Updated YoutubeExplode to v6.4.2 with latest YouTube API compatibility
- **Result**: YouTube downloads now work reliably without cipher errors

### 🔧 Technical Improvements

#### Enhanced Action Sheet Management
- Multiple fallback methods for closing action sheets
- Complete removal of all overlay types
- Better error handling and logging

#### Improved Dialog System Detection
- Multi-layered detection approach
- RequireJS integration support
- Proper Jellyfin environment verification

#### Updated Dependencies
- **YoutubeExplode**: 6.3.9 → 6.4.2 (fixes cipher manifest issues)
- Enhanced YouTube download reliability

### 📦 Installation

**MD5 Checksum:** `9771E6B3BE08EC3B8A526DF49849C5CE`

**Download URL:** https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/releases/download/v0.0.11.02/xThemeSong_0.0.11.02.zip

### 🔄 Upgrade Instructions

1. **Install**: Download and install the new version
2. **Restart**: Restart Jellyfin server
3. **Verify**: 
   - Three-dot menu shows "Assign Theme Song"
   - Clicking menu item closes action sheet completely (no overlay left)
   - Proper Jellyfin dialog opens (not fallback mode)
   - YouTube downloads work without cipher errors

### 🎯 What Now Works

- ✅ Three-dot menu integration stable
- ✅ Action sheets close completely (no overlays)
- ✅ Proper Jellyfin dialogs open consistently
- ✅ YouTube downloads work reliably
- ✅ MP3 upload functionality ready
- ✅ API endpoints respond correctly
- ✅ All dependencies properly packaged

### 🐛 Issues Resolved

- **Backdrop Overlay**: No more black overlays staying on screen
- **Dialog Fallback**: Proper Jellyfin dialogs now open
- **YouTube Errors**: Cipher manifest errors fixed with updated library
- **User Experience**: Smooth workflow from menu click to dialog

### 📋 Requirements

- **Jellyfin**: 10.10.0 or later
- **File Transformation Plugin**: Required for Web UI
- **FFmpeg**: Required for audio processing
- **Internet**: Required for YouTube downloads

---

**Note**: This version resolves ALL critical production issues and is recommended for immediate upgrade. The plugin is now production-ready with complete functionality.
