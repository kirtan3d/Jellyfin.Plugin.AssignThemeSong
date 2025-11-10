# Release v0.0.9.2 - Hybrid File Transformation Registration

## Summary
This release implements a robust hybrid approach to File Transformation plugin registration to handle the alphabetical loading order issue, ensuring reliable Web UI functionality.

## Key Changes

### Major Improvements
- **Hybrid Registration Approach**: Implements registration in both Plugin constructor AND scheduled task for maximum reliability
- **Alphabetical Loading Order Fix**: Addresses the issue where "Assign Theme Song" loads before "File Transformation" plugin
- **Enhanced Retry Mechanism**: 
  - Constructor: 10 attempts with exponential backoff (2-20 seconds)
  - Scheduled Task: 15 attempts with progressive delays (2-30 seconds)
- **Improved Error Handling**: Better logging and fallback mechanisms

### Technical Details
- **Updated Version**: 0.0.9.2 in all project files
- **MD5 Checksum**: 4F2FD8B12CC3B489F254C947B0A58760
- **Target ABI**: 10.10.0.0

## Problem Solved
The plugin was failing to register with File Transformation due to alphabetical loading order:
- "Assign Theme Song" loads before "File Transformation"
- Initial registration attempts failed because File Transformation wasn't available yet
- Now uses constructor registration with retries + scheduled task fallback

## Registration Strategy
1. **Plugin Constructor**: Attempts registration immediately with 10 retries
