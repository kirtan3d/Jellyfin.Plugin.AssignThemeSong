# xThemeSong v1.0.2

## 🎉 New Features

### 🗑️ Delete Theme Song
- Added delete button in the xThemeSong dialog
- Confirmation dialog before deletion
- Deletes both `theme.mp3` and `theme.json` files

### ⚙️ Custom FFmpeg Path Configuration
- New "FFmpeg Path" setting in plugin configuration
- Auto-detection fallback for common FFmpeg locations
- Supports Windows, Linux, macOS, and Docker

### 📚 Media Library Overview Tab
- New tabbed interface in plugin settings (Settings / Media Library)
- Statistics showing total media, items with themes, without themes
- View all movies and TV shows grouped by library
- Mini audio player for previewing existing theme songs
- Editable YouTube URL field for each item
- "Save URLs for this Library" button for bulk assignment
- Run scheduled task to download all queued themes

### 🔧 Handle theme.mp3 Without theme.json
- Plugin now properly handles media with `theme.mp3` but no `theme.json`
- Existing manual theme songs are preserved during scheduled tasks
- Audio player works for themes without metadata

## 📝 Changelog
- Added DELETE endpoint `/xThemeSong/{itemId}` for removing theme songs
- Added `GET /xThemeSong/library/overview` for media library listing
- Added `POST /xThemeSong/{itemId}/url` for saving YouTube URLs
- Added FFmpegPath configuration option
- Updated configPage.html with tabbed interface
- Fixed nullable reference type warnings

## 📋 Requirements
- Jellyfin Server 10.11.0 or later
- File Transformation Plugin (required for Web UI)
- FFmpeg (usually bundled with Jellyfin)

## 📦 Installation
Download `xThemeSong_v1.0.2.zip` and extract to your Jellyfin plugins directory, or install from the plugin repository.
