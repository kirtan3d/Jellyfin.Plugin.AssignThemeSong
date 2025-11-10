# Release v0.0.8.0 - Assign Theme Song Plugin

## Fixed Plugin Loading Order Issue

This release addresses the critical issue where the "Assign Theme Song" plugin was not appearing in Jellyfin's media page menu. The problem was caused by alphabetical loading order - our plugin (starting with "A") was loading before the File Transformation plugin (starting with "F"), causing registration to fail.

## Changes Made

### Core Fix: Retry Mechanism for Plugin Registration
- **File**: `Services/StartupTask.cs`
- **Solution**: Implemented a retry mechanism that attempts registration up to 6 times with 3-second delays between attempts
- **Benefit**: Ensures our plugin waits for File Transformation to be fully loaded before attempting registration

### Key Improvements
1. **Retry Logic**: The plugin now retries registration multiple times to handle the alphabetical loading order
2. **Better Error Handling**: More detailed logging and error messages for troubleshooting
3. **Consistent ID**: Using our plugin GUID as the transformation ID for consistency

## Technical Details

The issue was identified by analyzing Jellyfin logs showing:
- Plugins load alphabetically: Artwork → Assign Theme Song → Chapter Creator → ... → File Transformation
- Our plugin was trying to register with File Transformation before it was available
- Now uses retry mechanism: 6 attempts × 3-second delays = 18 seconds total wait time

## Version Information
- **Version**: 0.0.8.0
- **Target ABI**: 10.10.0.0
- **MD5 Checksum**: 22006D0DD108DAA07A3E70D37C44E63C

## Files Modified
- `Services/StartupTask.cs` - Added retry mechanism
- `Jellyfin.Plugin.AssignThemeSong.csproj` - Updated version
- `manifest.json` - Updated version and checksum

## Testing
The plugin should now properly:
- ✅ Register with File Transformation plugin
- ✅ Inject "Assign Theme Song" menu item in media pages
- ✅ Display the theme song assignment modal
- ✅ Handle both YouTube downloads and MP3 uploads

## Requirements
- **File Transformation Plugin**: Still required for Web UI features
- **Jellyfin Version**: 10.10.0 or later
- **FFmpeg**: Required for audio processing

This fix ensures the plugin works reliably regardless of plugin loading order in Jellyfin.
