# xThemeSong

A Jellyfin plugin that allows you to download theme songs from YouTube or upload custom MP3 files for your movies and TV shows.
<p align="center">
<img alt="Logo" src="https://raw.githubusercontent.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/main/images/icon.png" style="width:50%;" />
</p>

## ✨ Features

### Core Features
- 🎵 Download theme songs from YouTube by providing video ID or URL
- 📤 Upload your own MP3 files as theme songs
- 🎬 Supports both movies and TV shows
- 📁 Automatically saves theme songs as `theme.mp3` in media folders
- 📝 Stores metadata in `theme.json` files
- ⏰ Scheduled task to process theme songs (now with fixed error handling!)

### User Interface
- 🎛️ Configuration page with **tabbed interface** (Settings & Media Library)
- 🔄 Loading animations during processing
- 🎧 Audio player for existing theme songs
- ✅ Modern modal dialogs for success/error messages
- 🗑️ **Delete existing theme songs** with confirmation dialog

### Advanced Features (v1.2.0+)
- 📤 **Export/Import Theme Mappings** - Backup and migrate themes between servers
- 🔐 **Role-Based Access Control** - Control who can manage theme songs
- 👤 **Per-User Preferences** - Individual settings for enable/disable, volume, duration
- 📚 **Media Library Overview** - View all media with theme song status at a glance
- 📝 **Bulk YouTube URL Assignment** - Set URLs for multiple items in settings
- ⚙️ **Custom FFmpeg Path** - Configure FFmpeg location or use auto-detect

## 📋 Requirements

- **Jellyfin Server**: Version 10.11.0 or later
- **File Transformation Plugin**: **REQUIRED** for Web UI features to work. Install from [here](https://github.com/IAmParadox27/jellyfin-plugin-file-transformation)
- **FFmpeg**: Must be installed on your Jellyfin server (usually bundled with Jellyfin)
- **Internet Connection**: Required for YouTube downloads

## 🔧 Installation

### Prerequisites

**IMPORTANT:** Install the File Transformation plugin first!

1. Add repository URL: `https://www.iamparadox.dev/jellyfin/plugins/manifest.json` (If not already installed via catalog)
2. Go to **Dashboard → Plugins → Catalog**
3. Search for "File Transformation"
4. Install it and restart Jellyfin
5. Then proceed with installing xThemeSong

### Method 1: From Repository (Recommended)

1. Add repository URL to Jellyfin: `https://raw.githubusercontent.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/main/manifest.json`
2. Go to **Dashboard → Plugins → Catalog**
3. Search for "xThemeSong"
4. Click **Install** and restart Jellyfin

### Method 2: Manual Installation

1. Download the latest release from [GitHub Releases](https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/releases)
2. Extract the zip file
3. Copy the contents to your Jellyfin plugins directory:
   - **Windows**: `%AppData%\Jellyfin\Server\plugins\xThemeSong`
   - **Linux**: `/var/lib/jellyfin/plugins/xThemeSong`
   - **Docker**: `/config/plugins/xThemeSong`
4. Restart Jellyfin

## 📖 Usage

### Assigning a Theme Song

1. Navigate to a movie or TV show in Jellyfin
2. Click the **"⋮" (three dots)** menu
3. Select **"Assign Theme Song"**
4. A modal dialog will open showing:
   - 🎧 Existing theme song audio player (if available)
   - YouTube URL/Video ID input field
   - Drag-and-drop area for MP3 files
5. Choose one of the following:
   - Enter a YouTube video ID or URL
   - Upload an MP3 file (drag-and-drop or browse)
6. Click **"Save Theme Song"**
7. Wait for the loading animation to complete
8. Success message will appear when done!

### Scheduled Task

The plugin includes a scheduled task that processes theme songs:

1. Go to **Dashboard → Scheduled Tasks**
2. Find **"xTheme Songs"**
3. Click **▶ Play** to run immediately, or
4. Configure the schedule (default: daily at 3 AM)

## 📁 File Structure

For each media item with a theme song, the plugin creates:

```
/path/to/movie/
├── movie.mp4
├── theme.mp3          # The theme song audio file
└── theme.json         # Metadata about the theme song
```

### theme.json Format

```json
{
  "YouTubeId": "dQw4w9WgXcQ",
  "YouTubeUrl": "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
  "Title": "Never Gonna Give You Up",
  "Uploader": "RickAstleyVEVO",
  "DateAdded": "2025-01-04T12:00:00Z",
  "DateModified": "2025-01-04T12:00:00Z",
  "IsUserUploaded": false,
  "OriginalFileName": null
}
```

## ⚙️ Configuration

Access plugin settings in **Dashboard → Plugins → xThemeSong**:

### Settings Tab (Admin Access)
- **Overwrite Existing Files**: Whether to overwrite existing theme.mp3 files
- **Audio Bitrate**: Audio quality for downloaded theme songs (default: 192 kbps)
- **FFmpeg Path**: Custom path to FFmpeg executable (leave empty for auto-detect)
- **Permission Mode**: Control who can manage theme songs (Admins Only / Library Managers / Everyone)

### Backup & Migration (Admin)
- **Export to JSON**: Download all theme assignments for backup
- **Export to CSV**: Export for editing in spreadsheet applications
- **Import from JSON**: Restore themes from backup with conflict detection
- **Use Cases**: Server migrations, backups, bulk management

### Media Library Tab (Admin)
The Media Library tab provides a comprehensive overview of all your media:

- **Statistics**: See total media count, items with themes, and items without themes
- **Library Tables**: View all movies and TV shows grouped by library
- **Theme Status**: Quick badges showing which items have theme songs
- **Mini Audio Player**: Preview existing theme songs directly in the table
- **YouTube URL Input**: Enter YouTube URLs for each item
- **Bulk Save**: Save URLs for an entire library, then run the scheduled task to download

### User Preferences (All Users)
Access from: **Dashboard → Plugins → xThemeSong User Preferences**

Each user can customize their theme song experience:
- **Enable/Disable Theme Songs**: Turn theme songs on or off for your account
- **Maximum Duration**: Limit playback to X seconds (0 = play full theme)
- **Volume Control**: Adjust theme song volume (0-100%)
- **Server-Side Storage**: Preferences sync across all your devices

### Deleting Theme Songs

To remove an existing theme song:
1. Navigate to the movie or TV show
2. Click the **"⋮" (three dots)** menu and select **"Assign Theme Song"**
3. Click the **"🗑️ Delete"** button next to the existing theme
4. Confirm the deletion

## 🐛 Troubleshooting

### Plugin doesn't appear in Jellyfin

1. Check Jellyfin logs for errors: `/config/log/log_*.log`
2. Ensure you're running Jellyfin 10.11.0 or later
3. Verify the plugin files are in the correct directory
4. Restart Jellyfin after installation

### Theme songs not downloading

1. Check if FFmpeg is installed and accessible
2. Verify you have an internet connection
3. Check the scheduled task logs in **Dashboard → Scheduled Tasks**
4. Ensure the YouTube URL/ID is valid

### Build from Source

```bash
git clone https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong.git
cd Jellyfin.Plugin.AssignThemeSong
dotnet build -c Release
dotnet publish -c Release -o publish
```

## 📝 Development Status

**Current Version**: v1.3.2

### v1.3.2 Features (Latest - Bug Fix Release)
- ✅ **Fixed Media Library Filter Buttons** - Movies/Series filters now work correctly
- ✅ **Fixed Search by Title** - Search functionality now works with poster column
- ✅ **Fixed Poster Images** - Poster thumbnails now display correctly using Jellyfin's image API
- ✅ **Fixed Type Matching** - API response type (Series) matches UI display (Series)

### v1.3.0 Features (Major Update)
- ✅ **Season/Collection-Level Theme Inheritance** - Assign themes at Series, Season, or BoxSet level
- ✅ **Media Library Filters** - Filter by theme status (with/without) and search by title
- ✅ **Poster Thumbnails** - Display movie/show artwork in library overview
- ✅ **Library Type Tabs** - Quick filter by Movies/Series or specific library
- ✅ **Minimized Logging** - Reduced verbose logging for cleaner output
- ✅ **Theme Hierarchy API** - New endpoint to check theme inheritance

### v1.2.0 Features
- ✅ **Fixed Scheduled Task Error** - No more deserialization crashes
- ✅ **Export/Import Theme Mappings** - JSON & CSV export, import with conflict resolution
- ✅ **Role-Based Access Control** - 3 permission modes (Admins/Managers/Everyone)
- ✅ **Per-User Theme Preferences** - Enable/disable, volume, duration control per user
- ✅ **User Preferences Page** - Accessible to all users for customization
- ✅ **Code Quality** - Reduced warnings from 5 to 1 (80% reduction)
- ✅ **Security** - Permission-based API endpoint protection

### v1.1.0 Features
- ✅ **Tabbed Settings Page** - Clean organization with Settings and Media Library tabs
- ✅ **Media Library Overview** - View all media with theme song status at a glance
- ✅ **Inline Audio Players** - Preview theme songs directly in the library table
- ✅ **Bulk YouTube URL Assignment** - Set URLs for multiple items, download via scheduled task
- ✅ **Statistics Dashboard** - Total media, with themes, without themes counts
- ✅ **Improved Table Styling** - Better visual hierarchy and responsive layout

### v1.0.x Features
- ✅ Plugin loads successfully in Jellyfin
- ✅ **Web UI integration** - Three-dot menu item "Assign Theme Song"
- ✅ **Modern Modal Dialog** with dark theme
- ✅ **Loading Animations** during download/upload
- ✅ **Success/Error Messages** in modal dialogs (no JavaScript alerts)
- ✅ **Audio Player** for existing theme songs
- ✅ **Delete Theme Songs** with confirmation dialog
- ✅ **Drag-and-drop** file upload
- ✅ YouTube download service with YoutubeExplode v6.5.6
- ✅ MP3 upload support
- ✅ API endpoints for theme management
- ✅ Scheduled task for batch processing  
- ✅ **Custom FFmpeg Path** configuration
- ✅ **Cross-Platform FFmpeg Detection** - Windows, Mac, Linux, Docker
- ✅ File Transformation Plugin Integration

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- [Jellyfin](https://github.com/jellyfin/jellyfin) - The media server
- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) - YouTube download library
- Reference plugins: File Transformation, HoverTrailer, and others

## 📧 Support

For issues and questions:
- [GitHub Issues](https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/issues)
- [Jellyfin Forum](https://forum.jellyfin.org/)

---

**Note**: Please report any bugs or issues on GitHub.
