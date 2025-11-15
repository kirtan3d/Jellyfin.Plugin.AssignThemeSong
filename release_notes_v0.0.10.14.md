# xThemeSong v0.0.10.14 - Complete Naming Consistency Update

## 🔄 Major Refactoring

This release completes the renaming process from "AssignThemeSong" to "xThemeSong" throughout the entire codebase for consistency.

### Changes Made

#### Code Changes
- **Configuration Page**: Renamed all IDs and variables from `AssignThemeSong` to `xThemeSong`
- **Task Class**: Renamed `AssignThemeSongTask` to `xThemeSongTask` (file and class name)
- **API Controller**: Renamed `AssignThemeSongRequest` to `xThemeSongRequest`
- **API Routes**: Updated route from `/AssignThemeSong` to `/xThemeSong`
- **Script Controller**: Updated route and resource paths to use `xThemeSong`
- **Transformation Patches**: Updated script injection to use `xThemeSong` plugin name
- **File Renamed**: `AssignThemeSongTask.cs` → `xThemeSongTask.cs`

#### Version Updates
- **Version**: 0.0.10.14
- **Assembly Version**: 0.0.10.14
- **File Version**: 0.0.10.14

### Technical Details

#### Updated Namespaces
- All classes now use consistent `xThemeSong` naming
- Script routes updated: `/xThemeSong/plugin`, `/xThemeSong/xThemeSong`
- Configuration page IDs updated for proper JavaScript functionality

#### File Transformation Integration
- Script injection now uses `<script plugin="xThemeSong">`
- Transformation patches updated to match new naming scheme
- All regex patterns updated for consistency

### Benefits

1. **Consistency**: All code now uses the same naming convention
2. **Clarity**: Plugin identity is clear and consistent across all files
3. **Maintenance**: Easier to understand and maintain codebase
4. **Future-proof**: Consistent naming prevents confusion in future development

### Compatibility

- **Jellyfin Version**: 10.10.0+
- **Requires**: File Transformation Plugin
- **Target ABI**: 10.10.0.0

### Installation

For existing users:
1. Update through Jellyfin's plugin catalog
2. Restart Jellyfin after update
3. Plugin will work with existing theme songs

For new users:
1. Install File Transformation plugin first (required)
2. Install xThemeSong from plugin catalog
3. Restart Jellyfin
4. Navigate to any movie/TV show → Three-dot menu → "Assign Theme Song"

### What's Next

- Continue improving UI/UX
- Add more features for theme song management
- Enhance YouTube download capabilities

---

**Full Changelog**: https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/compare/v0.0.10.13...v0.0.10.14

**MD5 Checksum**: `2A413C9FB72AECE41B1CD595737B43BF`
