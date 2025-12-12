# xThemeSong Plugin - Publishing Guide

This guide explains how to publish a new version of the xThemeSong plugin when updating dependencies (like YoutubeExplode) or making other changes.

---

## Overview

When publishing a new version, you need to update the version number in **3 files** and create a release on GitHub.

### Files to Update:
1. `Jellyfin.Plugin.AssignThemeSong.csproj` - Version numbers + package references
2. `meta.json` - Plugin version and changelog
3. `manifest.json` - Release entry with checksum

---

## Step-by-Step Process

### Step 1: Update Package Version (if updating YoutubeExplode)

Open `Jellyfin.Plugin.AssignThemeSong.csproj` and update the YoutubeExplode version:

```xml
<!-- Change from current version to new version -->
<PackageReference Include="YoutubeExplode" Version="6.5.7" />
```

### Step 2: Update Plugin Version Numbers

#### In `Jellyfin.Plugin.AssignThemeSong.csproj`:
Update all three version fields (use semantic versioning X.Y.Z.0):

```xml
<Version>1.1.1.0</Version>
<AssemblyVersion>1.1.1.0</AssemblyVersion>
<FileVersion>1.1.1.0</FileVersion>
```

#### In `meta.json`:
Update the version and changelog:

```json
{
  "name": "xThemeSong",
  "guid": "97db7543-d64d-45e1-b2e7-62729b56371f",
  "version": "1.1.1.0",
  "targetAbi": "10.11.0.0",
  "overview": "Download theme songs from YouTube or upload custom MP3s for your media library",
  "description": "...",
  "owner": "Kirtan Patel",
  "category": "General",
  "imageUrl": "icon.png",
  "changelog": "v1.1.1: Updated YoutubeExplode to 6.5.7 for improved YouTube compatibility"
}
```

### Step 3: Clean and Build the Project

Open PowerShell in the project directory and run:

```powershell
# Navigate to project directory
cd "d:\Jellyfin Plugins\Jellyfin.Plugin.AssignThemeSong"

# Clean previous builds
dotnet clean

# Remove old build folders (ignore errors if they don't exist)
Remove-Item -Recurse -Force publish -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force bin -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force obj -ErrorAction SilentlyContinue

# Build and publish
dotnet publish Jellyfin.Plugin.AssignThemeSong.csproj -c Release -o publish
```

### Step 4: Verify the Assembly Version

Make sure the DLL has the correct version:

```powershell
[System.Diagnostics.FileVersionInfo]::GetVersionInfo("publish\Jellyfin.Plugin.xThemeSong.dll").FileVersion
```

Expected output: `1.1.1.0` (or whatever version you set)

### Step 5: Create the Zip File

**Important:** Create the zip from INSIDE the publish folder so files are at the root level:

```powershell
# Navigate to publish folder
cd publish

# Create zip file (files will be at root, not in a subfolder)
Compress-Archive -Path * -DestinationPath "..\xThemeSong_v1.1.1.zip" -Force

# Go back to project root
cd ..
```

### Step 6: Get MD5 Checksum

```powershell
certutil -hashfile xThemeSong_v1.1.1.zip MD5
```

Example output:
```
MD5 hash of xThemeSong_v1.1.1.zip:
abc123def456789...
CertUtil: -hashfile command completed successfully.
```

**Copy the hash** (e.g., `abc123def456789...`) - you'll need it for manifest.json

### Step 7: Update `manifest.json`

Add a new version entry at the **TOP** of the versions array:

```json
{
  "name": "xThemeSong",
  "guid": "97db7543-d64d-45e1-b2e7-62729b56371f",
  "overview": "Download theme songs from YouTube or upload custom MP3s for your media library",
  "description": "...",
  "owner": "Kirtan Patel",
  "category": "General",
  "imageUrl": "https://raw.githubusercontent.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/main/images/icon.png",
  "versions": [
    {
        "version": "1.1.1.0",
        "checksum": "<PASTE_MD5_HASH_HERE>",
        "changelog": "v1.1.1: Updated YoutubeExplode to 6.5.7 for improved YouTube compatibility",
        "targetAbi": "10.11.0.0",
        "sourceUrl": "https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/releases/download/v1.1.1/xThemeSong_v1.1.1.zip",
        "timestamp": "2025-12-12T12:00:00Z"
    },
    {
        "version": "1.1.0.0",
        ... // Previous versions stay below
    }
  ]
}
```

**Replace:**
- `<PASTE_MD5_HASH_HERE>` with the actual MD5 from Step 6
- Timestamp with current UTC time in ISO format

### Step 8: Commit and Push Changes

```powershell
# Stage all changes
git add -A

# Commit with descriptive message
git commit -m "v1.1.1: Updated YoutubeExplode to 6.5.7"

# Push to GitHub
git push origin main
```

### Step 9: Create GitHub Release

```powershell
gh release create v1.1.1 "xThemeSong_v1.1.1.zip" --title "xThemeSong v1.1.1" --notes "## Changes

- Updated YoutubeExplode to 6.5.7 for improved YouTube compatibility

**Full Changelog**: https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/compare/v1.1.0...v1.1.1"
```

### Step 10: Cleanup

Delete the local zip file:

```powershell
del xThemeSong_v1.1.1.zip
```

---

## Quick Reference Commands

Here's a combined script you can modify and run:

```powershell
# Set version (change this for each release)
$VERSION = "1.1.1"

# Clean and build
cd "d:\Jellyfin Plugins\Jellyfin.Plugin.AssignThemeSong"
dotnet clean
Remove-Item -Recurse -Force publish,bin,obj -ErrorAction SilentlyContinue
dotnet publish Jellyfin.Plugin.AssignThemeSong.csproj -c Release -o publish

# Verify version
[System.Diagnostics.FileVersionInfo]::GetVersionInfo("publish\Jellyfin.Plugin.xThemeSong.dll").FileVersion

# Create zip from inside publish folder
cd publish
Compress-Archive -Path * -DestinationPath "..\xThemeSong_v$VERSION.zip" -Force
cd ..

# Get MD5
certutil -hashfile "xThemeSong_v$VERSION.zip" MD5

# After updating manifest.json with checksum...
git add -A
git commit -m "v$VERSION: Updated YoutubeExplode to X.X.X"
git push origin main
gh release create "v$VERSION" "xThemeSong_v$VERSION.zip" --title "xThemeSong v$VERSION" --notes "Updated YoutubeExplode for improved compatibility"

# Cleanup
del "xThemeSong_v$VERSION.zip"
```

---

## Version Naming Convention

- **Major** (1.x.x.0): Breaking changes or major new features
- **Minor** (x.1.x.0): New features, backward compatible
- **Patch** (x.x.1.0): Bug fixes, dependency updates
- **Build** (x.x.x.0): Always 0 for releases

Examples:
- `1.1.1.0` - Patch release for dependency update
- `1.2.0.0` - Minor release with new feature
- `2.0.0.0` - Major release with breaking changes

---

## Troubleshooting

### Zip file has wrong structure
**Problem:** Files are in a subfolder inside the zip instead of at root level.
**Solution:** Make sure you `cd publish` before running `Compress-Archive -Path *`

### Assembly version doesn't match
**Problem:** DLL shows wrong version after build.
**Solution:** Run `dotnet clean` and delete bin/obj folders before rebuilding.

### Plugin doesn't load in Jellyfin
**Problem:** "Assembly with same name is already loaded" error.
**Solution:** Clean rebuild and make sure all version numbers match in csproj, meta.json, and manifest.json.

---

## Checklist

Before publishing, verify:

- [ ] Updated version in `Jellyfin.Plugin.AssignThemeSong.csproj` (3 places)
- [ ] Updated version in `meta.json`
- [ ] Clean rebuild completed
- [ ] Verified DLL version with `FileVersionInfo`
- [ ] Created zip from inside publish folder
- [ ] Got MD5 checksum
- [ ] Added new version entry to `manifest.json` (at TOP of array)
- [ ] Committed and pushed to GitHub
- [ ] Created GitHub release with zip file
- [ ] Cleaned up local zip file
