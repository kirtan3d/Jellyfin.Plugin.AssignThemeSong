### Release v0.1.0

This release marks a major stabilization of the xThemeSong plugin. The core logic has been refactored to be more robust and reliable.

**✨ Enhancements & Fixes**

- **CRITICAL FIX**: Refactored the API controller to use proper dependency injection for `ThemeDownloadService`. This fixes the core logic for both YouTube downloads and MP3 uploads, which were failing silently in previous builds.
- **FIX**: Corrected a bug where the original filename of user-uploaded MP3s was not being saved correctly in the `theme.json` metadata.
- **ENHANCEMENT**: Implemented a fallback mechanism for direct script injection into Jellyfin's `index.html`. This ensures the plugin's UI will load even if the File Transformation plugin is not installed, improving compatibility.
- **MAINTENANCE**: Performed a major code cleanup and refactoring.
    - Renamed classes to follow standard C# conventions (e.g., `ThemeSongTask`).
    - Removed the large and unnecessary `reference-plugins` directory.
    - Cleaned up the project file (`.csproj`) to ensure a successful and clean build every time.
