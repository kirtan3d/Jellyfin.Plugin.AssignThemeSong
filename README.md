# xThemeSong

A Jellyfin 12 plugin that allows you to download theme songs from YouTube or upload custom MP3 files for movies and TV shows.

<p align="center">
<img alt="Logo" src="https://raw.githubusercontent.com/ccook45/Jellyfin.Plugin.AssignThemeSong/main/images/icon.png" style="width:50%;" />
</p>

> **Project:** xThemeSong (Jellyfin Plugin AssignThemeSong) — jointly maintained by **Kirtan Patel (@kirtan3d)** and **ccook45 (@ccook45)**. Kirtan created the original project (core theme-song workflow, Web UI, scheduled downloads, media management, export/import, permissions, per-user preferences); ccook45 added Jellyfin 12 support, YouTube auto-search with managed yt-dlp, and drives ongoing maintenance. See the version history below.

## ✨ Features

### Core Features
- 🎵 Download theme songs from YouTube by providing a video ID or URL
- 🔎 **Automatic YouTube Theme Search** - Search YouTube from the media item title/type and rank likely theme-song matches
- 📤 Upload your own MP3 files as theme songs
- 🎬 Supports both movies and TV shows
- 📁 Automatically saves theme songs as `theme.mp3` in media folders
- 📝 Stores metadata in `theme.json` files
- ⏰ Scheduled task to process theme songs
- 🗑️ Delete existing theme songs with confirmation

### User Interface
- 🎛️ Configuration page with **Settings** and **Media Management** tabs
- 🔄 Loading animations during processing
- 🎧 Audio players for existing theme songs
- ✅ Modern modal dialogs for success/error messages
- 📚 Media Management overview for large libraries
- 📄 **Paginated Media Management** - large libraries are rendered a page at a time to keep the UI responsive
- 🔎 **YouTube Auto Search** - Find likely theme songs directly from the Assign Theme Song dialog, with ranked candidates and one-click download
- 🖼️ Lazy-loaded poster thumbnails and deferred audio loading in Media Management
- 🔄 **Live Media Management updates** - the Media Management status and statistics update immediately after a theme is assigned, without requiring a page refresh
- 🖼️ **YouTube result thumbnails** - candidate results show YouTube thumbnails when available
- ▶️ **YouTube verification** - open the selected YouTube result for review before downloading

### Advanced Features
- 📤 **Export/Import Theme Mappings** - Backup and migrate themes between servers
- 🔐 **Role-Based Access Control** - Control who can manage theme songs
- 👤 **Per-User Preferences** - Individual settings for enable/disable, volume, duration
- 📚 **Media Management** - View media with theme song status at a glance
- 📝 **Bulk YouTube URL Assignment** - Set URLs for multiple items in settings
- ⚙️ **Custom FFmpeg Path** - Configure FFmpeg location or use auto-detect
- 📦 **Managed yt-dlp fallback** - Automatically downloads and verifies the official yt-dlp binary when YoutubeExplode cannot provide an audio-only stream

## 📋 Requirements

- **Jellyfin Server**: **Version 12.0.0 or later**
- **.NET**: **.NET 10**
- **File Transformation Plugin**: **REQUIRED** for Web UI features to work. Install from the [File Transformation plugin](https://github.com/IAmParadox27/jellyfin-plugin-file-transformation)
- **FFmpeg**: Must be installed on your Jellyfin server (usually bundled with Jellyfin)
- **Internet Connection**: Required for YouTube downloads, automatic search, and the first managed yt-dlp download
- **yt-dlp**: No manual installation is required. xThemeSong downloads the appropriate official standalone binary when the fallback is needed

## 🔧 Installation

### From the Jellyfin Plugin Repository

1. Add this repository URL to Jellyfin:
   `https://raw.githubusercontent.com/ccook45/Jellyfin.Plugin.AssignThemeSong/main/manifest.json`
2. Go to **Dashboard → Plugins → Repositories** and add the URL.
3. Go to **Dashboard → Plugins → Catalog**.
4. Search for **xThemeSong**.
5. Click **Install** and restart Jellyfin.

### Manual Installation

1. Download the latest release from [GitHub Releases](https://github.com/ccook45/Jellyfin.Plugin.AssignThemeSong/releases).
2. Extract the zip file.
3. Copy the contents to your Jellyfin plugins directory:
   - **Windows**: `%AppData%\Jellyfin\Server\plugins\xThemeSong`
   - **Linux**: `/var/lib/jellyfin/plugins/xThemeSong`
   - **Docker**: `/config/plugins/xThemeSong`
4. Restart Jellyfin.

## 📖 Usage

### Assigning a Theme Song

1. Navigate to a movie or TV show in Jellyfin.
2. Click the **"⋮" (three dots)** menu.
3. Select **"Assign Theme Song"**.
4. A modal dialog will open showing:
   - 🎧 Existing theme song audio player, if available
   - Combined YouTube URL/search input field
   - Drag-and-drop area for MP3 files
5. Choose one of the following:
   - Leave the YouTube field blank and click **"🔎 Search YouTube"** to search automatically using the media title and type
   - Enter a YouTube video ID or URL to resolve that exact video as a single selectable result
   - Enter ordinary search terms to search YouTube and choose a result to download
   - Upload an MP3 file (drag-and-drop or browse)
6. When searching YouTube, select a result and click **Download**. The selected result is downloaded through the normal YoutubeExplode-first/yt-dlp-fallback path.
7. When uploading an MP3 file, use the normal upload/save controls.
7. Wait for the loading animation to complete.
8. A success message will appear when done.

### Managed yt-dlp fallback

xThemeSong normally uses **YoutubeExplode 6.6.2** for YouTube downloads. If Jellyfin/YouTube returns a manifest without a usable audio-only stream, xThemeSong automatically downloads the appropriate official **yt-dlp** standalone binary for the server platform and uses it as a fallback.

The managed binary is stored under Jellyfin's application data directory rather than inside the plugin package, so installing a plugin update does not require re-downloading it unless the managed executable is missing or unusable. The download is verified against yt-dlp's published SHA-256 checksum before it is installed.

Supported managed binaries currently include Windows x64/x86/ARM64, macOS, Linux x64/ARM64, and Alpine/musl Linux x64/ARM64 where yt-dlp publishes standalone builds. An existing `YT_DLP_PATH` environment variable can still be used for advanced/custom deployments.

The official yt-dlp executable includes the yt-dlp EJS components, but current yt-dlp YouTube extraction also relies on a supported JavaScript runtime such as Deno. FFmpeg remains required for audio extraction/post-processing. If a managed yt-dlp download succeeds but YouTube extraction still fails, the Jellyfin log will contain the yt-dlp error.

### Automatic YouTube Theme Search

From **Assign Theme Song**, click **🔎 Auto Search**.

xThemeSong builds multiple targeted YouTube searches from the media item's title, media type, and release year when available. For TV series, searches include variants such as:

- `<title> opening theme song`
- `<title> opening song`
- `<title> OP opening`
- `<title> official opening`
- `<title> opening full`
- `<title> opening creditless`
- `<title> ending theme song`
- `<title> theme song`
- `<title> OST opening`

For movies, searches include theme, main-theme, soundtrack, official-theme, instrumental, and OST variants.

Searches run with bounded concurrency to improve response time without creating an uncontrolled number of simultaneous requests. Candidates are deduplicated, scored, and filtered to favor likely theme music while demoting trailers, recaps, reviews, explained videos, full episodes, reactions, and other unrelated results. Up to 10 candidates are returned for review.

The search UI displays the result title, channel, duration, thumbnail, and a **Verify on YouTube** action. You can review a result before using **Download**.

This search workflow was inspired by the search approach used by [jellyfin-theme-downloader](https://github.com/ummmno/jellyfin-theme-downloader), which searches using the media title plus media-type/theme terms and prefers shorter results.

### Scheduled Task

The plugin includes a scheduled task that processes theme songs:

1. Go to **Dashboard → Scheduled Tasks**.
2. Find **"xTheme Songs"**.
3. Click **▶ Play** to run immediately, or
4. Configure the schedule.

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

Access plugin settings in **Dashboard → Plugins → xThemeSong**.

### Settings Tab
- **Overwrite Existing Files**: Whether to overwrite existing `theme.mp3` files
- **Audio Bitrate**: Audio quality for downloaded theme songs
- **FFmpeg Path**: Custom path to FFmpeg executable, or leave empty for auto-detect
- **Permission Mode**: Control who can manage theme songs (**Admins Only / Library Managers / Everyone**)

### Backup & Migration
- **Export to JSON**: Download all theme assignments for backup
- **Export to CSV**: Export for editing in spreadsheet applications
- **Import from JSON**: Restore themes from backup with conflict detection
- **Use Cases**: Server migrations, backups, and bulk management

### Media Management Tab

The Media Management tab provides an overview of your movies and TV shows:

- **Statistics**: See total media count, items with themes, and items without themes
- **Library Tables**: View movies and TV shows grouped by library
- **Theme Status**: Quick badges showing which items have theme songs
- **Mini Audio Player**: Preview existing theme songs directly in the table
- **YouTube URL Input**: Enter YouTube URLs for each item
- **Bulk Save**: Save URLs for multiple items, then run the scheduled task to download
- **Pagination**: Choose 25, 50, or 100 items per page and move between pages
- **Search and Filters**: Search by title and filter by media type, library, and theme status
- **Live status refresh**: After a theme is successfully assigned from the Assign Theme Song dialog, the affected row and Media Management statistics update immediately without refreshing the whole page

Pagination limits how many rows are rendered at once, which improves responsiveness for large media libraries while retaining the existing search and filtering controls.

### User Preferences

Access from **Dashboard → Plugins → xThemeSong User Preferences**.

Each user can customize their theme song experience:
- **Enable/Disable Theme Songs**: Turn theme songs on or off for your account
- **Maximum Duration**: Limit playback to X seconds (0 = play full theme)
- **Volume Control**: Adjust theme song volume (0-100%)
- **Server-Side Storage**: Preferences sync across your devices

### Deleting Theme Songs

To remove an existing theme song:

1. Navigate to the movie or TV show.
2. Click the **"⋮" (three dots)** menu and select **"Assign Theme Song"**.
3. Click the **"🗑️ Delete"** button next to the existing theme.
4. Confirm the deletion.

## 🐛 Troubleshooting

### Plugin doesn't appear in Jellyfin

1. Check Jellyfin logs for errors: `/config/log/log_*.log`.
2. Ensure you're running **Jellyfin 12.0.0 or later**.
3. Verify the plugin files are in the correct directory.
4. Restart Jellyfin after installation.

### Theme songs not downloading

1. Check that FFmpeg is installed and accessible.
2. Verify that the Jellyfin server has internet access.
3. Check the scheduled task logs in **Dashboard → Scheduled Tasks**.
4. Ensure the YouTube URL/ID is valid.
5. If YoutubeExplode reports no audio-only streams, look for the **managed yt-dlp** download/fallback messages in the Jellyfin log.
6. If yt-dlp reports a JavaScript-runtime/EJS error, install a supported runtime such as Deno in the Jellyfin environment.

### Automatic YouTube Search returns poor matches

- Review several candidates instead of downloading the first result.
- Use the **Verify on YouTube** action to inspect the source before downloading.
- The search is intentionally heuristic; YouTube search results can vary over time.
- If the correct opening theme is not near the top, try the media title without extra punctuation in the library metadata.

### Media Management is slow or does not show all items

- Use the built-in pagination controls to limit the number of rows rendered at once.
- Use search and the library/theme filters to narrow the displayed results.
- If you upgraded from an older release, restart Jellyfin after installing the current release.

## 🔨 Build from Source

This fork targets Jellyfin 12 and .NET 10.

```bash
git clone https://github.com/ccook45/Jellyfin.Plugin.AssignThemeSong.git
cd Jellyfin.Plugin.AssignThemeSong
dotnet build -c Release
dotnet publish -c Release -o publish
```

The project currently uses **YoutubeExplode 6.6.2** for YouTube access.

## 📝 Development Status

**Current Version**: **v1.4.22**

### Jellyfin 12 / Fork Maintenance

- Targets **Jellyfin 12.0.0** and **.NET 10**
- Updated Jellyfin package references to 12.0.0
- Updated authorization handling for Jellyfin 12 role claims
- Removed the runtime dependency on the incompatible Jellyfin 12 `IUserManager.GetUserByName` API path
- Added a Jellyfin 12 build/release workflow
- Corrected plugin assembly/file version reporting
- Ensures the plugin logo is included in release packages
- Uses **YoutubeExplode 6.6.2** for current YouTube compatibility

### v1.4.23

- Added a second set of Media Library pagination controls at the bottom of the page, matching the existing top controls.
- Previous/Next navigation and page-size selection remain synchronized through the same pagination state.

## v1.4.22
- ✅ **Unavailable-result filtering** - YouTube search results are validated before they are presented for selection, so deleted/private/unavailable videos are filtered out when YoutubeExplode can identify them as unavailable
- ✅ **Safer search validation** - Automatic theme search validates the highest-ranked candidates while keeping transient/API validation failures from incorrectly hiding usable results
- ✅ **Consistent override validation** - Manual YouTube URL searches and text searches use the same unavailable-result filtering before download selection
- ✅ **Clearer YouTube fallback behavior** - The existing YoutubeExplode-first download flow and managed yt-dlp fallback remain unchanged

### v1.4.21
- ✅ **YouTube URL/search override input** - The assignment dialog accepts a YouTube URL/video ID or ordinary search terms in the same field
- ✅ **Exact video selection** - A YouTube URL resolves to one selectable result instead of directly downloading
- ✅ **Manual YouTube search** - Search text returns selectable YouTube candidates before download
- ✅ **YoutubeExplode-first search with yt-dlp fallback** - Existing fallback behavior is preserved for search failures

### v1.4.20
- ✅ **Managed yt-dlp fallback** - xThemeSong automatically downloads the appropriate official yt-dlp standalone binary when the normal YoutubeExplode path has no audio-only streams
- ✅ **SHA-256 verification** - the downloaded yt-dlp binary is verified against the official yt-dlp checksum list before use
- ✅ **Platform detection** - selects the appropriate Windows, macOS, Linux/glibc, or Linux/musl binary for the server architecture
- ✅ **Persistent tool storage** - managed yt-dlp is stored in Jellyfin's application data rather than the plugin package
- ✅ **Safer process arguments** - yt-dlp fallback invocation uses structured process arguments instead of shell-style quoting

### v1.4.17
- ✅ **Live Media Management refresh** - Media Management immediately updates the affected item's theme status and statistics after a successful theme assignment
- ✅ Avoids a full library reload, preserving the performance improvements for large libraries

### v1.4.16
- ✅ **Faster YouTube search** - runs targeted searches with bounded concurrent queries
- ✅ Removed unnecessary sequential YouTube metadata hydration because search results already contain the title, channel, duration, and video ID
- ✅ Keeps candidate deduplication and ranking while reducing avoidable request latency

### v1.4.15
- ✅ **Expanded opening-theme search coverage** with multiple opening, OP, official, creditless, ending, theme, and OST query variants
- ✅ Increased the candidate pool and returns up to 10 results
- ✅ Improved ranking to strongly favor opening/theme music and demote trailers, recaps, reviews, episodes, reactions, and other unrelated content

### v1.4.14
- ✅ **Fixed Jellyfin 12 thumbnail authentication** for YouTube search results
- ✅ Added targeted theme-focused search terms for opening/ending/main theme results
- ✅ Added YouTube thumbnail display and direct result verification workflow

### v1.4.13
- ✅ **Handled Jellyfin JSON casing** so the search UI works with either CamelCase or PascalCase JSON responses
- ✅ Restored reliable rendering of result title, channel, duration, URL, and thumbnails across Jellyfin formatter profiles

### v1.4.12
- ✅ Changed search duration serialization to a numeric duration value for reliable Web UI rendering
- ✅ Added a Jellyfin-side thumbnail proxy for YouTube thumbnails
- ✅ Changed YouTube verification to an explicit action that opens the selected YouTube URL
- ✅ Improved handling of search result metadata and verification links

### v1.4.11
- ✅ Added YouTube search result thumbnails
- ✅ Added a **Verify on YouTube** action before download
- ✅ Added logging/fallback behavior when result metadata hydration fails

### v1.4.10
- ✅ Published a distinct point release to make the updated YouTube search metadata behavior visible to Jellyfin's plugin update system

### v1.4.9
- ✅ **Automatic YouTube Theme Search** - Search YouTube from a movie/series title and rank likely theme-song results
- ✅ **Candidate Review** - Shows multiple search results with title, channel, and duration before downloading
- ✅ **One-Click Download** - Download a selected search result directly to the media item's `theme.mp3`
- ✅ Uses media type and production year when available to improve search queries

### v1.4.8
- ✅ Fixed plugin version reporting so the installed assembly/package reports the release version correctly
- ✅ Explicitly packages the plugin logo so it is available in the release
- ✅ Retains the Media Management pagination and rendering improvements

### v1.4.7
- ✅ **Media Management Pagination** - Large libraries are displayed a page at a time instead of rendering every row at once
- ✅ **Lazy poster loading** - Poster images are loaded as needed
- ✅ **Deferred audio loading** - Audio elements use deferred loading to reduce initial page load work

### v1.4.6
- ✅ Updated **YoutubeExplode** to 6.6.2 for current YouTube compatibility
- ✅ Jellyfin 12 / .NET 10 maintenance release

### v1.4.5
- ✅ Updated the Jellyfin 12-compatible YouTube download path and released the first stable post-migration package used to diagnose current YouTube compatibility
- ✅ Retained the Jellyfin 12 / .NET 10 build and release workflow

### v1.4.4
- ✅ **Jellyfin 12 compatibility release**
- ✅ Built against **Jellyfin 12.0.0 / .NET 10**

### v1.4.3
- ✅ Jellyfin 12 compatibility package built against **Jellyfin 12.0.0 / .NET 10**

### v1.4.2
- ✅ Jellyfin 12 compatibility package built against **Jellyfin 12.0.0 / .NET 10**

### v1.4.1
- ✅ Jellyfin 12 compatibility package built against **Jellyfin 12.0.0 / .NET 10**

### v1.4.0
- ✅ **Jellyfin 12 compatibility** - migrated from the Jellyfin 10.11 / .NET 9 baseline to Jellyfin 12.0 APIs and .NET 10
- ✅ Added a Jellyfin 12 build/release workflow
- ✅ Established the xThemeSong Jellyfin 12 release/manifest packaging

### v1.3.2 (original project)
- ✅ **Fixed Media Library Filter Buttons** - Movies/Series filters now work correctly
- ✅ **Fixed Search by Title** - Search functionality works correctly with the poster column
- ✅ **Fixed Poster Images** - Poster thumbnails display correctly using Jellyfin's image API
- ✅ **Fixed Type Matching** - API response type (Series) matches the UI display (Series)

### v1.3.1 (original project)
- ✅ Fixed Media Library filter buttons for Movies/TV Shows
- ✅ Fixed search by title
- ✅ Fixed type matching between the API response (`Series`) and the UI (`TV Shows`)

### v1.3.0 (original project)
- ✅ **Season/Collection-Level Theme Inheritance** - Assign themes at Series, Season, or BoxSet level
- ✅ **Media Library Filters** - Filter by theme status and search by title
- ✅ **Poster Thumbnails** - Display movie/show artwork in the library overview
- ✅ **Library Type Tabs** - Quick filter by Movies/Series or specific library
- ✅ **Minimized Logging** - Reduced verbose logging for cleaner output
- ✅ **Theme Hierarchy API** - Endpoint for checking theme inheritance

### v1.2.0 (original project)
- ✅ **Fixed Scheduled Task Error** - Removed deserialization crashes
- ✅ **Export/Import Theme Mappings** - JSON & CSV export, import with conflict resolution
- ✅ **Role-Based Access Control** - Admins/Managers/Everyone permission modes
- ✅ **Per-User Theme Preferences** - Enable/disable, volume, and duration controls per user
- ✅ **User Preferences Page** - Accessible to all users
- ✅ **Code Quality** - Reduced warnings from 5 to 1
- ✅ **Security** - Permission-based API endpoint protection

### v1.1.0 (original project)
- ✅ **Tabbed Settings Page** - Settings and Media Library tabs
- ✅ **Media Library Overview** - View media with theme-song status
- ✅ **Inline Audio Players** - Preview theme songs in the library table
- ✅ **Bulk YouTube URL Assignment** - Set URLs for multiple items and download via the scheduled task
- ✅ **Statistics Dashboard** - Total media, with themes, and without themes counts
- ✅ **Improved Table Styling** - Better visual hierarchy and responsive layout

### v1.0.x (original project)
- ✅ Plugin loads successfully in Jellyfin
- ✅ **Web UI integration** - Three-dot menu item for "Assign Theme Song"
- ✅ **Modern Modal Dialog** with dark theme
- ✅ **Loading Animations** during download/upload
- ✅ **Success/Error Messages** in modal dialogs
- ✅ **Audio Player** for existing theme songs
- ✅ **Delete Theme Songs** with confirmation
- ✅ **Drag-and-drop** file upload
- ✅ YouTube download service using YoutubeExplode v6.5.6
- ✅ MP3 upload support
- ✅ API endpoints for theme management
- ✅ Scheduled task for batch processing
- ✅ **Custom FFmpeg Path** configuration
- ✅ **Cross-Platform FFmpeg Detection** - Windows, Mac, Linux, Docker
- ✅ File Transformation Plugin Integration

## 👥 Maintainers & Contributors
- **Kirtan Patel (@kirtan3d)** — original creator: theme-song workflow, Web UI, scheduled downloads, media management, export/import, permissions, per-user preferences
- **ccook45 (@ccook45)** — co-maintainer: Jellyfin 12 support & migration, YouTube auto-search, managed yt-dlp, search/pagination improvements, documentation & release automation

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

When contributing, please preserve the original project's attribution and version history. Improvements to the Jellyfin 12 fork should be documented in the version history above.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- **Kirtan Patel (@kirtan3d)** - Original author of the AssignThemeSong project and creator of the original v1.0.x–v1.3.x feature history
- [Jellyfin](https://github.com/jellyfin/jellyfin) - The media server
- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) - YouTube download library
- [jellyfin-theme-downloader](https://github.com/ummmno/jellyfin-theme-downloader) - Reference for the automatic YouTube theme-search workflow
- File Transformation and other Jellyfin plugin projects referenced by the original project

## 📧 Support

For issues and questions:
- [GitHub Issues](https://github.com/ccook45/Jellyfin.Plugin.AssignThemeSong/issues)
- [Jellyfin Forum](https://forum.jellyfin.org/)

---

**Note:** This project is not officially endorsed by the Jellyfin project. Please report bugs or issues on GitHub.
