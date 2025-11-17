# xThemeSong v0.0.11.01 Release Notes

## CRITICAL BUG FIXES - Action Sheet Overlay & API Route Issues

This release fixes critical issues that were preventing the plugin from working correctly in production environments.

### 🚨 Critical Issues Fixed

#### 1. **API Route Mismatch - 404 Errors**
- **Problem**: API calls were returning 404 "Not Found" errors
- **Root Cause**: ThemeSongController route was `[Route("Items/{itemId}/ThemeSong")]` but JavaScript was calling `xThemeSong/{itemId}`
- **Solution**: Changed API route to `[Route("xThemeSong/{itemId}")]` to match JavaScript calls
- **Result**: API endpoints now respond correctly, no more 404 errors

#### 2. **Action Sheet Overlay Staying Open**
- **Problem**: Three-dot menu and black overlay remained visible after clicking "Assign Theme Song"
- **Root Cause**: Incomplete action sheet closing logic in plugin.js
- **Solution**: Implemented comprehensive action sheet closing with multiple fallback methods:
  - dialogHelper.close() (primary)
  - Direct style.display = 'none'
  - Backdrop removal
  - Parent dialog closing
- **Result**: Action sheets now close properly before dialog opens

#### 3. **Dialog System Fallback Issues**
- **Problem**: Dialog was falling back to simple mode even when Jellyfin dialog system was available
- **Root Cause**: Dialog system detection logic was incomplete
- **Solution**: Enhanced detection with multiple verification methods
- **Result**: Proper Jellyfin dialogs now open when available

### 🔧 Technical Improvements

#### Enhanced Error Handling
- Multiple fallback methods for action sheet closing
- Better dialog system detection
- Improved logging for debugging

#### Code Quality
- Fixed API route consistency across all files
- Enhanced action sheet management
- Better separation of concerns

### 📦 Installation

**MD5 Checksum:** `70B40C634BA755F4E8158B75ECBF087E`

**Download URL:** https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/releases/download/v0.0.11.01/xThemeSong_0.0.11.01.zip

### 🔄 Upgrade Instructions

1. **Backup**: No data migration required - theme files remain intact
2. **Install**: Download and install the new version
3. **Restart**: Restart Jellyfin server
4. **Verify**: 
   - Three-dot menu shows "Assign Theme Song"
   - Clicking menu item opens dialog properly
   - Action sheet closes without overlay issues
   - API calls work without 404 errors

### 🎯 What Now Works

- ✅ Three-dot menu integration stable
- ✅ Action sheets close properly
- ✅ No more black overlay staying on screen
- ✅ API endpoints respond correctly
- ✅ Dialog opens reliably
- ✅ YouTube download and MP3 upload functional

### 🐛 Issues Resolved

- **404 API Errors**: Fixed route mismatch between controller and JavaScript
- **Menu Overlay**: Action sheets now close completely
- **Dialog Fallback**: Proper dialog system detection
- **User Experience**: Smooth workflow from menu click to dialog

### 📋 Requirements

- **Jellyfin**: 10.10.0 or later
- **File Transformation Plugin**: Required for Web UI
- **FFmpeg**: Required for audio processing
- **Internet**: Required for YouTube downloads

---

**Note**: This version resolves the critical production issues and is recommended for immediate upgrade.
