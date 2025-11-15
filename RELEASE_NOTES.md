# xThemeSong v0.0.10.9 - MutationObserver Menu Integration

## 🔧 Critical Fix

This release fixes the infinite retry loop issue from v0.0.10.8.

### **Fixed: Infinite Retry Loop**
- **Problem**: v0.0.10.8 was checking for `window.itemHelper` which doesn't exist in Jellyfin
- **Result**: Console showed endless "itemHelper not available yet, retrying..." messages
- **Fixed**: Completely rewrote menu integration using **MutationObserver**

### **New Implementation: MutationObserver Pattern**
- Observes DOM for `.actionSheet` elements (three-dot menus)
- When menu opens, dynamically injects our custom menu item
- Verifies item is Movie or Series before adding menu item
- Properly styled to match Jellyfin's native menu items

## 📋 How It Works

1. **Plugin loads** → Script tag injected via File Transformation ✅
2. **MutationObserver starts** → Watching for action sheet menus ✅
3. **User clicks three-dot menu (⋮)** → Action sheet appears
4. **Observer detects it** → Checks if we're on a details page
5. **Verifies item type** → Must be Movie or Series
6. **Injects menu item** → "Assign Theme Song" with music_note icon
7. **User clicks menu item** → Opens theme song assignment dialog

## 🎯 What to Expect

After installing v0.0.10.9:

1. Navigate to any **movie or TV show** details page
2. Click the **⋮ (three dots)** menu in the top-right
3. You should see **"Assign Theme Song"** menu item appear
4. Click it to open the theme song assignment dialog

### Console Messages
```
xThemeSong: Plugin script loaded
xThemeSong: Module loaded, initializing...
xThemeSong: Initializing menu observer...
xThemeSong: Menu observer installed
[when you click three-dot menu]
xThemeSong: Action sheet detected
xThemeSong: Adding menu item for [itemId]
xThemeSong: Menu item added successfully
```

## 🐛 Troubleshooting

If menu item doesn't appear:
1. **Hard refresh** browser (Ctrl+Shift+R)
2. **Check console** for xThemeSong messages
3. **Clear cache** completely
4. **Restart Jellyfin** to ensure v0.0.10.9 is loaded

## 📊 Technical Details

- **Method**: MutationObserver watching for `.actionSheet` elements
- **Injection Point**: Before `.actionsheetDivider`or at end of menu
- **Item Verification**:  Checks item.Type === 'Movie' || item.Type === 'Series'
- **Menu Item Styling**: Matches Jellyfin's native action sheet menu items

## ⚠️ Requirements

- **Jellyfin**: 10.10.0 or later
- **File Transformation Plugin**: REQUIRED (must be installed first)
- **FFmpeg**: For audio conversion

---

**MD5 Checksum**: `4C65FE17243090A199DCF5C52DB93E01`
