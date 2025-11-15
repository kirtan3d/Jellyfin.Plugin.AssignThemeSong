# xThemeSong v0.0.10.11 Release Notes

## 🐛 Critical Installation Fix

### Fixed Missing Dependencies
- **CRITICAL FIX**: Plugin now includes all required dependencies for proper functionality
- **Root Cause**: Previous builds were missing essential DLL files (YoutubeExplode.dll, AngleSharp.dll, etc.)
- **Solution**: Full rebuild with all dependencies included in the zip archive
- **Result**: Plugin should now install successfully without "An error occurred while installing the plugin" messages

### Included Dependencies
- ✅ `YoutubeExplode.dll` - Required for YouTube downloads
- ✅ `AngleSharp.dll` - Required for HTML parsing
- ✅ `Newtonsoft.Json.dll` - Required for JSON operations
- ✅ `Jellyfin.Plugin.xThemeSong.dll` - Main plugin assembly
- ✅ `meta.json` - Plugin metadata
- ✅ All web assets (JavaScript, HTML, images)

## 🔧 Technical Improvements

### Build Process
- **Complete Dependencies**: All required NuGet packages are now included
- **Clean Archive**: Properly structured zip file with correct folder hierarchy
- **Updated Checksum**: New MD5 hash for v0.0.10.11: `5607F50EF484A046895066D0F0FF73C9`

### Plugin Architecture
- **Updated Version**: Bumped to v0.0.10.11
- **Proper Assembly**: All dependencies bundled for standalone operation
- **Jellyfin Compatibility**: Maintains compatibility with Jellyfin 10.10.0+

## 🎯 What's Fixed

### Installation Issues
- No more "file in use" errors during installation
- No more missing dependency errors
- Proper plugin registration in Jellyfin
- Clean installation process

### Functionality
- YouTube download service now has required dependencies
- All plugin features work as intended
- Web UI integration remains functional
- Scheduled task processing works correctly

## 📦 Installation

### For New Users
1. Install File Transformation plugin first (required)
2. Install xThemeSong from Jellyfin plugin catalog
3. Restart Jellyfin
4. Navigate to any Movie or TV Show
5. Click the three-dot menu → "Assign Theme Song"

### For Upgrading Users
1. Update plugin through Jellyfin plugin catalog
2. Restart Jellyfin (recommended)
3. Verify installation completes successfully

## 🔍 Testing

This release has been tested to ensure:
- Plugin installs without errors
- All dependencies are present and functional
- YouTube downloads work correctly
- MP3 uploads function properly
- Three-dot menu integration works
- No console errors in browser

## 📝 Known Issues

None - this release focuses on fixing the installation dependency issues that prevented the plugin from being installed.

## 🙏 Acknowledgments

Thanks to all users who reported the installation issues and helped identify the missing dependency problem.

---

**Plugin Version**: 0.0.10.11  
**Jellyfin Version**: 10.10.0+  
**Release Date**: 2025-11-15  
**File Transformation Plugin**: Required  
**MD5 Checksum**: 5607F50EF484A046895066D0F0FF73C9
