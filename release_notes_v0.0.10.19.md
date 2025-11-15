# xThemeSong v0.0.10.19 - Fixed Duplicates & RequireJS Errors

## 🐛 Critical Fixes

### Fixed Duplicate Menu Items
- **FIXED**: Menu item was appearing 4 times due to multiple detection triggers
- **SOLUTION**: Implemented WeakSet tracking to prevent processing same action sheet multiple times
- **RESULT**: Menu item now appears only once

### Fixed RequireJS Errors
- **FIXED**: "require is not defined" error when clicking menu item
- **SOLUTION**: Changed all `require()` calls to `window.require()`
- **FALLBACK**: Added fallback to `alert()` if RequireJS not available
- **RESULT**: Dialog now opens without errors

## 🎯 Technical Changes

### WeakSet Tracking
```javascript
const processedActionSheets = new WeakSet();

function onActionSheetOpened(actionSheet) {
    if (processedActionSheets.has(actionSheet)) {
        return; // Already processed
    }
    processedActionSheets.add(actionSheet);
    // ... process action sheet
}
```

### RequireJS Safety
- All `require()` calls now use `window.require()`
- Added type checking before calling RequireJS
- Graceful fallback for toast notifications

## 📦 Package Contents

- ✅ `Jellyfin.Plugin.xThemeSong.dll` v0.0.10.19
- ✅ `web/` folder (plugin.js, xThemeSong.js, meta.js)
- ✅ All dependencies (YoutubeExplode.dll, AngleSharp.dll, etc.)
- ✅ `meta.json` metadata
- ✅ Configuration and images

## ✅ What Works Now

- ✅ Three-dot menu integration functional
- ✅ "Assign Theme Song" appears once (no duplicates)
- ✅ No RequireJS errors when clicking
- ✅ Modal dialog opens properly
- ✅ All naming consistent using xThemeSong

## 🔄 Complete Naming Consistency

- ✅ Configuration page
- ✅ Task class: `xThemeSongTask`
- ✅ API classes: `xThemeSongRequest`
- ✅ All routes: `/xThemeSong`
- ✅ Script injection
- ✅ Transformation patches

## ✅ Compatibility

- **Jellyfin**: 10.10.0+ (tested on 10.11.2)
- **Requires**: File Transformation Plugin
- **Target ABI**: 10.10.0.0

## 📥 Installation

1. Install File Transformation plugin first (required)
2. Update/Install xThemeSong v0.0.10.19
3. Restart Jellyfin
4. Navigate to movie/TV show → Three-dot menu (⋮) → "Assign Theme Song"

---

**MD5 Checksum**: `A33293C8D3BE427DCF030D3AB8B87B09`  
**Full Changelog**: https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/compare/v0.0.10.18...v0.0.10.19
