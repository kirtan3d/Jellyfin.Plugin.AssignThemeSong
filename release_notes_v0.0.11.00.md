# xThemeSong v0.0.11.00 Release Notes

## Major Version Bump - Stable Release

This release represents a major milestone with significant improvements to plugin stability, error handling, and user experience.

### 🎯 Key Improvements

#### 1. **Fixed RequireJS Availability Issues**
- **Problem**: Dialog failed to open with "RequireJS not available" errors
- **Solution**: Implemented robust fallback system that tries multiple approaches:
  - Jellyfin's dialog system with RequireJS (primary)
  - Global dialogHelper directly (fallback)
  - Simple custom modal dialog (ultimate fallback)
- **Result**: Dialog now opens reliably in all scenarios

#### 2. **Enhanced Error Handling**
- Comprehensive error checking for all dialog dependencies
- Graceful degradation when Jellyfin modules are unavailable
- Proper null checking for loading indicators
- Fallback alerts when toast notifications fail

#### 3. **Improved Menu Integration**
- Fixed WeakSet tracking to prevent duplicate menu items
- Enhanced MutationObserver for action sheet detection
- Menu item now appears exactly once without errors
- Proper integration with Jellyfin's three-dot menu system

#### 4. **Code Quality & Dependencies**
- All dependencies properly packaged (YoutubeExplode, AngleSharp)
- Web files included in build output
- Consistent naming scheme throughout (xThemeSong)
- Proper versioning across all files

### 🔧 Technical Details

**Dependencies Included:**
- YoutubeExplode.dll (v6.3.9) - YouTube download functionality
- AngleSharp.dll - HTML parsing for transformation
- All web assets (plugin.js, xThemeSong.js, meta.js)

**Fixed Issues:**
- RequireJS module loading failures
- Dialog creation errors
- Duplicate menu item injection
- Dependency packaging in zip archive

### 📦 Installation

**MD5 Checksum:** `B6DBA07EC878E4BCD2D4E430B28FEF6A`

**Download URL:** https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/releases/download/v0.0.11.00/xThemeSong_0.0.11.00.zip

### 🔄 Upgrade Instructions

1. **Backup**: No data migration required - theme files remain intact
2. **Install**: Download and install the new version
3. **Restart**: Restart Jellyfin server
4. **Verify**: Check that "Assign Theme Song" appears in three-dot menus

### 🚀 What's Next

This release establishes a stable foundation for future features:
- Batch theme song processing
- Theme song library management
- Enhanced YouTube integration
- Custom theme song scheduling

### 📋 Requirements

- **Jellyfin**: 10.10.0 or later
- **File Transformation Plugin**: Required for Web UI
- **FFmpeg**: Required for audio processing
- **Internet**: Required for YouTube downloads

---

**Note**: This version is production-ready and recommended for all users.
