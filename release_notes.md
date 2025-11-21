# xThemeSong v0.1.0

## 🚀 Changes

### Critical Fixes
- **Installation Fix**: Resolved `System.IO.InvalidDataException` by implementing a proper build script that creates a valid zip structure.
- **Dependency Injection**: Refactored API controller to use proper dependency injection for `ThemeDownloadService`.
- **Upload Fix**: Corrected a bug where the original filename of user-uploaded MP3s was not saved.

### Enhancements
- **Robustness**: Implemented a fallback mechanism for direct script injection if the File Transformation plugin is missing.
- **Cleanup**: Added automatic temp file cleanup to prevent disk clutter.
- **Logging**: Improved logging for better debugging of download issues.

### Maintenance
- **Build System**: Added `build.ps1` for automated and reliable builds.
- **Code Cleanup**: Major refactoring and removal of unused files.

## 📦 Installation

1. Copy the URL of `manifest.json` from the repository.
2. Add it to your Jellyfin Plugins repositories.
3. Install **xThemeSong** from the catalog.
