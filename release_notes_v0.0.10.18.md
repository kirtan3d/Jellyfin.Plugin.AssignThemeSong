# xThemeSong v0.0.10.18 - Enhanced Action Sheet Detection

## 🔧 Critical Fix

### Enhanced MutationObserver Configuration
- **FIXED**: MutationObserver now watches for both `childList` changes AND `class` attribute changes
- **NEW**: Detects action sheets appearing when class is added to existing elements
- **NEW**: Checks for nested action sheets within added container nodes
- **RESULT**: Should now properly detect three-dot menu action sheets in Jellyfin 10.11.2

## 🎯 Technical Improvements

### MutationObserver Configuration
```javascript
observer.observe(document.body, {
    childList: true,      // Watch for added/removed nodes
    subtree: true,        // Watch entire tree
    attributes: true,     // Watch for attribute changes
    attributeFilter: ['class']  // Only watch class attribute
});
```

### Detection Logic
1. Checks for new nodes with `actionSheet` class
2. Checks children of added nodes for action sheets
3. Watches for `actionSheet` class being added to existing elements
4. Comprehensive logging at each detection point

## 📦 Package Contents

- ✅ `Jellyfin.Plugin.xThemeSong.dll` v0.0.10.18
- ✅ `web/` folder with all JavaScript files
- ✅ All dependencies (YoutubeExplode.dll, AngleSharp.dll, etc.)
- ✅ `meta.json` metadata
- ✅ Configuration and images

## 🔄 Complete Naming Consistency

All references use "xThemeSong":
- ✅ Configuration page
- ✅ Task class: `xThemeSongTask`
- ✅ API classes and routes
- ✅ Script injection
- ✅ Transformation patches

## ✅ Compatibility

- **Jellyfin**: 10.10.0+
- **Requires**: File Transformation Plugin
- **Target ABI**: 10.10.0.0

## 📥 Installation

1. Install File Transformation plugin first (required)
2. Update/Install xThemeSong v0.0.10.18
3. Restart Jellyfin
4. Navigate to movie/TV show → Click three-dot menu (⋮) → "Assign Theme Song"

## 🧪 Testing

Expected behavior:
- Three-dot menu shows "Assign Theme Song" menu item
- Console shows "xThemeSong: Action sheet detected" when menu opens
- Menu item appears for movies and TV shows
- Modal dialog opens when clicked

---

**MD5 Checksum**: `9E7DB4DEFCF576233CF1488E9BF12BE9`  
**Full Changelog**: https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/compare/v0.0.10.17...v0.0.10.18
