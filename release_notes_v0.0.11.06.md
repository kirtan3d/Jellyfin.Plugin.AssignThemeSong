# xThemeSong v0.0.11.06 Release Notes

## CRITICAL FIX: Missing YoutubeExplode Dependency Resolved

This release fixes the critical plugin loading issue that was preventing the plugin from loading in Jellyfin due to missing `YoutubeExplode` assembly dependency.

### 🔧 What's Fixed

#### Missing Dependency Resolution

- **Previous Issue**: Plugin failed to load with `System.IO.FileNotFoundException: Could not load file or assembly 'YoutubeExplode, Version=6.5.6.0'`
