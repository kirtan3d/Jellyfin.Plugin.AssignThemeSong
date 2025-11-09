# Jellyfin Assign Theme Song Plugin

A Jellyfin plugin that allows you to download theme songs from YouTube or upload custom MP3 files for your movies and TV shows.

## ✨ Features

- 🎵 Download theme songs from YouTube by providing video ID or URL
- 📤 Upload your own MP3 files as theme songs
- 🎬 Supports both movies and TV shows
- 📁 Automatically saves theme songs as `theme.mp3` in media folders
- 📝 Stores metadata in `theme.json` files
- ⏰ Scheduled task to process theme songs
- 🎛️ Configuration page in Jellyfin dashboard

## 📋 Requirements

- **Jellyfin Server**: Version 10.10.0 or later
- **File Transformation Plugin**: **REQUIRED** for Web UI features to work. Install from [here](https://github.com/IAmParadox27/jellyfin-plugin-file-transformation)
- **FFmpeg**: Must be installed on your Jellyfin server (usually bundled with Jellyfin)
- **Internet Connection**: Required for YouTube downloads

## 🔧 Installation

### Prerequisites

**IMPORTANT:** Install the File Transformation plugin first!

1. Go to **Dashboard → Plugins → Catalog**
2. Search for "File Transformation"
3. Install it and restart Jellyfin
4. Then proceed with installing Assign Theme Song

### Method 1: From Jellyfin Plugin Catalog (Recommended)

1. In Jellyfin, go to **Dashboard → Plugins → Catalog**
2. Search for "Assign Theme Song"
3. Click **Install**
4. Restart Jellyfin

### Method 2: Manual Installation

1. Download the latest release from [GitHub Releases](https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/releases)
2. Extract the zip file
3. Copy the contents to your Jellyfin plugins directory:
   - Windows: `%AppData%\Jellyfin\Server\plugins\Assign Theme Song`
   - Linux: `/var/lib/jellyfin/plugins/Assign Theme Song`
   - Docker: `/config/plugins/Assign Theme Song`
4. Restart Jellyfin

## 📖 Usage

### Assigning a Theme Song

1. Navigate to a movie or TV show in Jellyfin
2. Click the **"⋮" (three dots)** menu
3. Select **"Assign Theme Song"**
4. A modal dialog will open showing:
   - Existing theme song audio player (if available)
   - YouTube URL/Video ID input field
   - Drag-and-drop area for MP3 files
5. Choose one of the following:
   - Enter a YouTube video ID or URL
   - Upload an MP3 file (drag-and-drop or browse)
6. Click **"Save Theme Song"**

### Scheduled Task

The plugin includes a scheduled task that processes theme songs:

1. Go to **Dashboard → Scheduled Tasks**
2. Find **"Assign Theme Songs"**
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

Access plugin settings in **Dashboard → Plugins → Assign Theme Song**:

- **Overwrite Existing Files**: Whether to overwrite existing theme.mp3 files
- **Audio Bitrate**: Audio quality for downloaded theme songs (default: 192 kbps)

## 🐛 Troubleshooting

### Plugin doesn't appear in Jellyfin

1. Check Jellyfin logs for errors: `/config/log/log_*.log`
2. Ensure you're running Jellyfin 10.10.0 or later
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

**Current Version**: v0.0.2.0

This plugin is now stable and fully functional! Features:
- ✅ Plugin loads successfully in Jellyfin
- ✅ **Web UI integration** - Three-dot menu item "Assign Theme Song"
- ✅ **Modal dialog** with drag-and-drop file upload
- ✅ **Audio player** for existing theme songs
- ✅ YouTube download service with YoutubeExplode (dependency included)
- ✅ MP3 upload support with drag-and-drop
- ✅ API endpoints for assigning theme songs
- ✅ Scheduled task for batch processing
- ✅ Configuration page
- ✅ Extensive logging for debugging plugin initialization

### Recent Changes (v0.0.2.0)
- Updated plugin structure to match Jellyfin plugin template standards
- Fixed code structure issues and improved plugin initialization
- Cleaned up build process and removed unnecessary files

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- [Jellyfin](https://github.com/jellyfin/jellyfin) - The media server
- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) - YouTube download library
- Reference plugins: Themerr, Theme Songs Plugin, and others

## 📧 Support

For issues and questions:
- [GitHub Issues](https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/issues)
- [Jellyfin Forum](https://forum.jellyfin.org/)

---

**Note**: Please report any bugs or issues on GitHub.
