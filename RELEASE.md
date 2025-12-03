# xThemeSong v1.1.0 🎵

A Jellyfin plugin to download theme songs from YouTube or upload custom MP3 files for your movies and TV shows.

## ✨ What's New in v1.1.0

### Enhanced Settings Page
- **Tabbed Interface** - Clean organization with separate Settings and Media Library tabs
- **Proper Tab Switching** - Tabs now switch correctly using inline display styles

### Media Library Overview
- **Statistics Dashboard** - View total media count, items with themes, and items without themes at a glance
- **Inline Audio Players** - Preview theme songs directly in the library table
- **Improved Table Styling** - Better visual hierarchy with library groups and responsive layout
- **Type Badges** - Quick identification of Movies vs TV Series

### Bulk URL Assignment
- **YouTube URL Input** - Enter URLs for each media item in the library table
- **Save per Library** - Save URLs for an entire library section
- **Scheduled Download** - URLs are processed when the scheduled task runs

---

## 📋 Changelog Since v1.0.2

### v1.1.0 (Current)
- ✅ Enhanced tabbed settings page UI
- ✅ Improved Media Library overview with inline audio players
- ✅ Bulk YouTube URL assignment functionality
- ✅ Statistics dashboard (total, with theme, without theme)
- ✅ Better table styling and responsive layout
- ✅ Fixed tab switching to work properly

### v1.0.6
- Fixed config page tabs showing both content at once
- Improved table styling in Media Library

### v1.0.5
- Fixed Media Library overview to use proper item type filtering

### v1.0.4
- Fixed critical build issue - plugin DLL was being deleted during build
- Plugin now loads correctly in Jellyfin

### v1.0.3
- Fixed plugin initialization - resolved null reference issue
- Settings page and menu now appear correctly

### v1.0.2
- ✅ **Delete Theme Song** functionality with confirmation dialog
- ✅ **Custom FFmpeg Path** configuration (with auto-detect fallback)
- ✅ **Media Library Overview** tab to view all media with theme song status
- ✅ Handle `theme.mp3` files without accompanying `theme.json`
- ✅ Bulk YouTube URL assignment in settings

---

## 📦 Installation

### From Repository (Recommended)
1. Add repository URL to Jellyfin: `https://raw.githubusercontent.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/main/manifest.json`
2. Go to **Dashboard → Plugins → Catalog**
3. Search for "xThemeSong" and install
4. Restart Jellyfin

### Prerequisites
- **Jellyfin 10.11.0+** (requires .NET 9)
- **File Transformation Plugin** - Required for Web UI features

---

## 🎯 Features

- 🎵 Download theme songs from YouTube (video ID or URL)
- 📤 Upload custom MP3 files as theme songs
- 🎬 Supports movies and TV shows
- 🗑️ Delete existing theme songs
- ⚙️ Custom FFmpeg path configuration
- 📚 Media Library overview with theme song status
- 📝 Bulk YouTube URL assignment
- 🎧 Audio player for existing themes
- ⏰ Scheduled task for batch processing
- 🔄 Loading animations and success/error messages

---

**Full documentation**: [GitHub Repository](https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong)
