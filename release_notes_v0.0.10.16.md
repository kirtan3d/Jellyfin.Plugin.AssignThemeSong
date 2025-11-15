# xThemeSong v0.0.10.16 - Enhanced Action Sheet Detection

## 🎯 Critical Fix

### Enhanced Menu Detection
- **NEW**: Now checks for existing action sheets immediately on page load
- **NEW**: Also monitors for new action sheets appearing via MutationObserver
- **RESULT**: Menu item should now appear reliably in the three-dot menu

## 🐛 Bug Fixes

### Fixed Issues
1. **Action Sheet Detection**: Plugin now handles both pre-existing and dynamically added action sheets
2. **Script Route**: Confirmed `/xThemeSong/plugin` route alignment with ScriptController
3. **Web Folder**: Properly included in published zip for file system fallback

## 🔄 Complete Naming Consistency

All "AssignThemeSong" references have been renamed to "xThemeSong":
- ✅ Configuration page (IDs, variables, event handlers)
- ✅ Task class: `xThemeSongTask`
- ✅ API request class: `xThemeSongRequest`
- ✅ All routes and endpoints
- ✅ Script injection
- ✅ Transformation patches

## 📦 Package Contents

- ✅ `Jellyfin.Plugin.xThemeSong.dll` v0.0.10.16
- ✅ `web/` folder (plugin.js, xThemeSong.js, meta.js)
- ✅ All dependencies (YoutubeExplode.dll, AngleSharp.dll, etc.)
- ✅ `meta.json` metadata
- ✅ Configuration and images

## 🔧 Technical Details

### Action Sheet Detection Logic
```javascript
// Check existing action sheets on load
const existingActionSheets = document.querySelectorAll('.actionSheet');
existingActionSheets.forEach(actionSheet => onActionSheetOpened(actionSheet));

// Monitor for new action sheets
const observer = new MutationObserver(...);
observer.observe(document.body, { childList: true, subtree: true });
```

### Routes
- Script injection: `{basePath}/xThemeSong/plugin`
- Plugin script: `GET /xThemeSong/plugin`
- Dialog module: `GET /xThemeSong/xThemeSong`
- Theme API: `POST /Items/{itemId}/ThemeSong`

## ✅ Compatibility

- **Jellyfin**: 10.10.0+
- **Requires**: File Transformation Plugin
- **Target ABI**: 10.10.0.0

## 📥 Installation

1. Install File Transformation plugin first
2. Update/Install xThemeSong v0.0.10.16
3. Restart Jellyfin
4. Go to any movie/TV show → Three-dot menu → "Assign Theme Song"

## 🧪 Testing

Please verify:
- Three-dot menu shows "Assign Theme Song" option
- Menu item appears for movies and TV shows
- Modal dialog opens when menu item is clicked
- Both YouTube URL and MP3 upload work

---

**MD5 Checksum**: `7D8620115228F3833BEB81EAC767FFA9`  
**Full Changelog**: https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/compare/v0.0.10.15...v0.0.10.16
