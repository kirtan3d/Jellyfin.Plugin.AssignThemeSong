# xThemeSong v0.0.10.15 - Critical Route Fix & Complete Naming Consistency

## 🐛 Critical Bug Fix

### Fixed Script Injection Route Mismatch
- **CRITICAL FIX**: Plugin.cs was injecting script from `/xThemeSong/script` but ScriptController serves it at `/xThemeSong/plugin`
- **Solution**: Updated Plugin.cs to use correct route: `/xThemeSong/plugin`
- **Result**: Scripts should now load correctly without 406 errors

### Included Web Folder
- **FIXED**: Web folder now properly included in published zip for file system fallback
- **Benefit**: ScriptController can fall back to file system if embedded resources fail
- **Files**: `web/plugin.js`, `web/xThemeSong.js`, `web/meta.js` now included

## 🔄 Complete Naming Consistency

### All AssignThemeSong → xThemeSong Changes
- ✅ Configuration page IDs and variables
- ✅ Task class renamed: `xThemeSongTask`
- ✅ API request class: `xThemeSongRequest`
- ✅ API routes: `/xThemeSong`
- ✅ Script routes: `/xThemeSong/plugin`, `/xThemeSong/xThemeSong`
- ✅ Transformation patches use `xThemeSong`
- ✅ All embedded resource paths updated

## 📦 What's Included

### Complete Package
- ✅ `Jellyfin.Plugin.xThemeSong.dll` - Main plugin assembly
- ✅ `web/` folder with all JavaScript files
- ✅ All required dependencies (YoutubeExplode.dll, AngleSharp.dll, etc.)
- ✅ `meta.json` - Plugin metadata
- ✅ Configuration and image files

## 🎯 Fixed Issues

1. **Route Mismatch**: Script injection now uses `/xThemeSong/plugin` matching ScriptController
2. **Missing Web Folder**: Web files now included for proper fallback mechanism
3. **Naming Consistency**: All code uses xThemeSong naming convention
4. **Version Alignment**: All files now at v0.0.10.15

## 🔧 Technical Details

### Updated Routes
- Script injection: `{basePath}/xThemeSong/plugin`
- Main plugin script endpoint: `GET /xThemeSong/plugin`
- xThemeSong module endpoint: `GET /xThemeSong/xThemeSong`
- Theme song API: `POST /Items/{itemId}/ThemeSong`

### File Structure in ZIP
```
Jellyfin.Plugin.xThemeSong.dll
AngleSharp.dll
YoutubeExplode.dll
meta.json
web/
  ├── plugin.js
  ├── xThemeSong.js
  └── meta.js
```

## ✅ Compatibility

- **Jellyfin Version**: 10.10.0+
- **Requires**: File Transformation Plugin (for Web UI features)
- **Target ABI**: 10.10.0.0

## 📥 Installation

1. Install File Transformation plugin first (required)
2. Update/Install xThemeSong from plugin catalog
3. Restart Jellyfin
4. Navigate to movie/TV show → Three-dot menu → "Assign Theme Song"

---

**MD5 Checksum**: `A1E6CEBF9011B0D0E8D88D6D3657AE53`  
**Full Changelog**: https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/compare/v0.0.10.14...v0.0.10.15
