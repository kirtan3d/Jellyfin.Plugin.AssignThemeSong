# xThemeSong v1.2.0 - Major Feature Update 🎵

A Jellyfin plugin to download theme songs from YouTube or upload custom MP3 files for your movies and TV shows.

## ✨ What's New in v1.2.0

### 🐛 **Fixed: Scheduled Task Error**
- **Issue**: Task failed with "Cannot deserialize unknown type" error
- **Fix**: Modified query to use `BaseItemKind` enum filtering (Movie, Series only)
- **Result**: Scheduled task now runs successfully without crashes

### 📤 **Export/Import Theme Mappings** (NEW!)
- **Export to JSON**: Backup all theme assignments with full metadata
- **Export to CSV**: Open in Excel/Sheets for bulk editing
- **Import from JSON**: Restore themes on another server
- **Conflict Resolution**: Handles ID changes, path mismatches, duplicates
- **Use Cases**: Server migrations, backups, bulk theme management

### 🔐 **Role-Based Access Control** (NEW!)
- **Permission Modes**:
  - **Administrators Only**: Restrict theme management to admins
  - **Administrators & Library Managers** (Default): Include library managers
  - **Everyone**: Allow all authenticated users
- **Security**: API returns 403 Forbidden for unauthorized access
- **UI**: Easy dropdown configuration in Settings tab

### 👤 **Per-User Theme Preferences** (NEW!)
- **Enable/Disable**: Users can turn off theme songs for their account
- **Duration Limit**: Set max playback duration (0-300 seconds, 0 = full)
- **Volume Control**: Adjust theme song volume (0-100%)
- **Server-Side Storage**: Preferences sync across devices
- **Access**: Dashboard → Plugins → "xThemeSong User Preferences"

### 🧹 **Code Quality Improvements**
- Reduced compiler warnings from 5 to 1 (80% reduction)
- Added `#nullable enable` for better null safety
- Code cleanup and optimization

---

## 📋 Complete Changelog (v1.2.0)

### New Features
- ✅ Export/Import theme song mappings (JSON/CSV)
- ✅ Role-based access control with 3 permission modes
- ✅ Per-user theme preferences (enable/disable, volume, duration)
- ✅ User preferences page accessible to all users

### Bug Fixes
- ✅ Fixed scheduled task deserialization error
- ✅ Fixed nullable reference type warnings

### Technical Improvements
- ✅ Added `BaseItemKind` enum filtering in queries
- ✅ Proper namespace handling for System.IO.File
- ✅ Improved error handling and logging
- ✅ Code quality: 5 warnings → 1 warning

---

## 🆕 New API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/xThemeSong/export/json` | Export all themes to JSON |
| GET | `/xThemeSong/export/csv` | Export all themes to CSV |
| POST | `/xThemeSong/export/import` | Import themes from JSON |
| GET | `/xThemeSong/preferences` | Get user's theme preferences |
| POST | `/xThemeSong/preferences` | Save user's theme preferences |

---

## 📦 Installation

### From Repository (Recommended)
1. Add repository URL to Jellyfin: 
   ```
   https://raw.githubusercontent.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/main/manifest.json
   ```
2. Go to **Dashboard → Plugins → Catalog**
3. Search for "xThemeSong" and update to v1.2.0
4. Restart Jellyfin

### Prerequisites
- **Jellyfin 10.11.0+** (requires .NET 9)
- **File Transformation Plugin** - Required for Web UI features

---

## 🎯 Key Features (All Versions)

- 🎵 Download theme songs from YouTube (video ID or URL)
- 📤 Upload custom MP3 files as theme songs
- 🎬 Supports movies and TV shows
- 🗑️ Delete existing theme songs
- ⚙️ Custom FFmpeg path configuration
- 📚 Media Library overview with theme song status
- 📝 Bulk YouTube URL assignment
- 📥 Export/Import for backup and migration
- 🔐 Role-based permission control
- 👤 Per-user preference settings
- 🎧 Audio player for existing themes
- ⏰ Scheduled task for batch processing

---

## 🔗 Links

- **GitHub**: https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong
- **Issues**: https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/issues
- **Full Documentation**: [README.md](https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/blob/main/README.md)

---

## 🙏 Feedback

Please report any bugs or issues on GitHub. Happy theming! 🎬🎵
