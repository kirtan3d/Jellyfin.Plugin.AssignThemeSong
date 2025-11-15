# xThemeSong v0.0.10.17 - DOM Ready Fix

## 🔧 Initialization Timing Fix

### Improved DOM Ready Detection
- **FIXED**: Plugin now waits for DOM to be fully ready before initializing
- **CHANGE**: Added `DOMContentLoaded` event listener for proper timing
- **BENEFIT**: Ensures MutationObserver is set up after page is complete

## 🎯 Technical Changes

### Initialization Logic
```javascript
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializePlugin);
} else {
    initializePlugin(); // DOM already ready
}
```

### Action Sheet Detection
- Checks for existing action sheets on initialization
- Sets up MutationObserver to monitor for new action sheets
- Includes comprehensive logging for debugging

## 📦 Package Contents  

- ✅ `Jellyfin.Plugin.xThemeSong.dll` v0.0.10.17
- ✅ `web/` folder (plugin.js, xThemeSong.js, meta.js)
- ✅ All dependencies (YoutubeExplode.dll, AngleSharp.dll, etc.)
- ✅ `meta.json` metadata
- ✅ Configuration and images

## 🔄 Complete Naming Consistency

All "AssignThemeSong" → "xThemeSong":
- ✅ Configuration page
- ✅ Task class: `xThemeSongTask`
- ✅ API request: `xThemeSongRequest`
- ✅ All routes: `/xThemeSong`
- ✅ Script injection
- ✅ Transformation patches

## ✅ Compatibility

- **Jellyfin**: 10.10.0+
- **Requires**: File Transformation Plugin
- **Target ABI**: 10.10.0.0

## 📥 Installation

1. Install File Transformation plugin first
2. Update/Install xThemeSong v0.0.10.17
3. Restart Jellyfin
4. Navigate to movie/TV show → Three-dot menu → "Assign Theme Song"

---

**MD5 Checksum**: `2241BCDEA1DADDEF8470AC94C6845440`  
**Full Changelog**: https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/compare/v0.0.10.16...v0.0.10.17
