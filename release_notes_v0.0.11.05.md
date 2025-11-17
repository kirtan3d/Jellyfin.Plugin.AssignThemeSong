# xThemeSong v0.0.11.05 Release Notes

## CRITICAL FIX: Plugin Loading Dependency Conflicts Resolved

This release fixes the critical plugin loading issue that was preventing the plugin from loading in Jellyfin due to dependency conflicts with System.Text.Encodings.Web and other .NET runtime assemblies.

### 🔧 What's Fixed

#### Dependency Conflict Resolution

- **Previous Issue**: Plugin failed to load with `System.IO.FileLoadException: Assembly with same name is already loaded` for `System.Text.Encodings.Web`
- **Root Cause**: Plugin was packaging .NET runtime assemblies that conflict with Jellyfin's runtime environment
- **Solution**: Updated project configuration to exclude unnecessary assemblies and prevent runtime conflicts

#### Project Configuration Updates

- **CopyLocalLockFileAssemblies**: Set to `false` to prevent copying runtime assemblies
- **SelfContained**: Set to `false` to avoid self-contained deployment
- **Package References**: Added `ExcludeAssets="runtime"` to prevent packaging runtime assemblies
- **Build Target**: Added post-build cleanup to remove unnecessary System.* and Microsoft.* assemblies

### 📦 Installation

**MD5 Checksum:** `DD39C53D78C4E3AA244933792F476E09`

**Download URL:** https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/releases/download/v0.0.11.05/xThemeSong_0.0.11.05.zip

### 🔄 What Now Works

- ✅ Plugin loads successfully without dependency conflicts
- ✅ All previous UI fixes remain intact
- ✅ Three-dot menu integration stable
- ✅ YouTube downloads work with latest API
- ✅ MP3 upload functionality ready

### 📋 Requirements

- **Jellyfin**: 10.10.0 or later
- **File Transformation Plugin**: Required for Web UI
- **FFmpeg**: Required for audio processing
- **Internet**: Required for YouTube downloads

### 🎯 Plugin Features

- **Web UI Integration**: Three-dot menu shows "Assign Theme Song"
- **YouTube Downloads**: Latest YoutubeExplode 6.5.6 for API compatibility
- **MP3 Uploads**: Drag-and-drop file upload functionality
- **Scheduled Tasks**: Background processing of theme songs
- **Metadata Storage**: theme.json files with comprehensive metadata

### 🔄 Upgrade Instructions

1. **Install**: Download and install v0.0.11.05
2. **Restart**: Restart Jellyfin server
3. **Verify**: Plugin should now load successfully and appear in plugins list

---

**Note**: This update resolves the critical dependency conflicts that were preventing the plugin from loading. All previous functionality remains intact and improved.
