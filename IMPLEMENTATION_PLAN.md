# xThemeSong Plugin - Implementation Plan for Remaining Features

## Overview
This document outlines the implementation plan for the remaining features in todo.md, ensuring no existing functionality is broken during development.

---

## Feature 1: Granular Role-Based Access Control

### Summary
Add permission system to restrict theme song management actions (assign/edit/delete) based on user roles.

### Requirements Analysis
- Jellyfin has native permission system via user policies
- Need to check user permissions before allowing operations
- Provide UI in settings to configure who can manage themes

### Implementation Steps

#### 1.1 Backend - Permission Checking
**Files to Modify:**
- `Api/ThemeSongController.cs` - Add permission checks to all endpoints
- `PluginConfiguration.cs` - Add permission settings

**Changes:**
```csharp
// PluginConfiguration.cs - Add new properties
public enum ThemePermissionMode
{
    AdminsOnly,           // Only server admins
    LibraryManagers,      // Admins + library managers
    Everyone              // All authenticated users
}

public class PluginConfiguration : BasePluginConfiguration
{
    public ThemePermissionMode PermissionMode { get; set; } = ThemePermissionMode.LibraryManagers;
    public List<Guid> AllowedUserIds { get; set; } = new List<Guid>(); // For custom whitelist
    // ... existing properties
}

// ThemeSongController.cs - Add permission check method
private bool HasThemeManagementPermission(ClaimsPrincipal user, BaseItem? item = null)
{
    var config = Plugin.Instance?.Configuration;
    var userId = GetUserId(user);
    
    // Always allow admins
    if (IsAdmin(user)) return true;
    
    switch (config?.PermissionMode)
    {
        case ThemePermissionMode.AdminsOnly:
            return false;
            
        case ThemePermissionMode.LibraryManagers:
            // Check if user is library manager for this item's library
            if (item != null)
            {
                return IsLibraryManager(userId, item);
            }
            return false;
            
        case ThemePermissionMode.Everyone:
            return true;
            
        default:
            return config?.AllowedUserIds?.Contains(userId) ?? false;
    }
}

// Apply to all endpoints that modify themes
[HttpPost]
public async Task<ActionResult> AssignThemeSongFromYouTube(...)
{
    if (!HasThemeManagementPermission(User, item))
    {
        return Forbid("You do not have permission to manage theme songs");
    }
    // ... existing code
}
```

#### 1.2 Frontend - Settings UI
**Files to Modify:**
- `Configuration/configPage.html` - Add permissions section

**Changes:**
```html
<!-- Add to Settings tab -->
<div class="verticalSection">
    <h3 class="sectionTitle">Permissions</h3>
    
    <div class="selectContainer">
        <label class="selectLabel" for="PermissionMode">Who can manage theme songs?</label>
        <select id="PermissionMode" is="emby-select">
            <option value="AdminsOnly">Administrators Only</option>
            <option value="LibraryManagers">Administrators & Library Managers</option>
            <option value="Everyone">Everyone (All Users)</option>
        </select>
        <div class="fieldDescription">
            Library Managers can only manage themes in libraries they have access to
        </div>
    </div>
</div>
```

### Testing
- ✅ Verify admin can always manage themes
- ✅ Test library manager restrictions per library
- ✅ Verify "Everyone" mode works
- ✅ Check API returns 403 Forbidden for unauthorized users
- ✅ Ensure UI hides/disables controls for unauthorized users

### Risks & Mitigation
- **Risk**: Breaking existing functionality for users
- **Mitigation**: Default to `LibraryManagers` (current behavior), existing users unaffected

---

## Feature 2: Season and Collection-Level Themes

### Summary
Support theme songs at Series, Season, and Collection levels with proper hierarchy.

### Requirements Analysis
- Hierarchy: Collection > Series > Season > Episode (Episode inherits from Season/Series)
- Need to store theme location (Series/Season/Collection)
- Fallback logic: Season → Series → Collection
- UI needs to show where theme is inherited from

### Implementation Steps

#### 2.1 Backend - Hierarchical Theme Storage
**Files to Create/Modify:**
- `Models/ThemeMetadata.cs` - Add theme level property
- `Api/ThemeSongController.cs` - Add season/collection endpoints
- `Plugin.cs` or new `Services/ThemeHiearchyService.cs` - Theme resolution logic

**Changes:**
```csharp
// ThemeMetadata.cs - Add level tracking
public enum ThemeLevel
{
    Item,        // Movie or specific item
    Season,      // Season level
    Series,      // Series level  
    Collection   // Collection/BoxSet level
}

public class ThemeMetadata
{
    public ThemeLevel Level { get; set; } = ThemeLevel.Item;
    public Guid? InheritedFromId { get; set; } // ID of parent if inherited
    // ... existing properties
}

// New ThemeHierarchyService.cs
public class ThemeHierarchyService
{
    public async Task<ThemeMetadata?> ResolveThemeForItem(BaseItem item)
    {
        // Try item-level theme first
        var theme = await GetDirectTheme(item);
        if (theme != null) return theme;
        
        // For episodes, try season then series
        if (item is Episode episode)
        {
            var season = episode.Season;
            if (season != null)
            {
                theme = await GetDirectTheme(season);
                if (theme != null) return theme.WithInheritance(season.Id, ThemeLevel.Season);
            }
            
            var series = episode.Series;
            if (series != null)
            {
                theme = await GetDirectTheme(series);
                if (theme != null) return theme.WithInheritance(series.Id, ThemeLevel.Series);
            }
        }
        
        // Try collection
        var collections = _libraryManager.GetCollections(item);
        foreach (var collection in collections)
        {
            theme = await GetDirectTheme(collection);
            if (theme != null) return theme.WithInheritance(collection.Id, ThemeLevel.Collection);
        }
        
        return null;
    }
}
```

#### 2.2 Frontend - UI for Hierarchy
**Files to Modify:**
- `web/xThemeSong.js` - Add level indicator in dialog
- Update assign dialog to show inheritance

**Changes:**
```javascript
// Show where theme is inherited from
if (metadata.inheritedFromId) {
    var inheritanceMsg = document.createElement('div');
    inheritanceMsg.className = 'inheritance-notice';
    inheritanceMsg.innerHTML = '🔗 Inherited from ' + metadata.level;
    dialogContent.appendChild(inheritanceMsg);
}

// Add option to assign at different levels for series/seasons
if (itemType === 'Season' || itemType === 'Series') {
    var levelSelect = '<select id="themeLevel">' +
        '<option value="Item">This ' + itemType + ' only</option>' +
        '<option value="Series">Entire Series</option>' +
        '</select>';
}
```

### Testing
- ✅ Episode inherits from Season when no episode theme
- ✅ Episode inherits from Series when no Season theme
- ✅ Collection themes apply to all movies in collection
- ✅ Direct theme overrides inherited theme
- ✅ UI clearly shows inheritance source
- ✅ Deletion of parent theme cascades appropriately

### Risks & Mitigation
- **Risk**: Complex inheritance logic may cause confusion
- **Mitigation**: Clear UI indicators, comprehensive logging
- **Risk**: Performance impact from checking multiple levels
- **Mitigation**: Cache resolved themes, optimize queries

---

## Feature 3: Per-User Theme Song Toggle and Duration

### Summary
Allow users to individually disable theme songs or limit playback duration via user settings.

### Requirements Analysis
- Store per-user preferences (enable/disable, max duration)
- Frontend needs to respect user settings during playback
- Settings accessible from user preferences page

### Implementation Steps

#### 3.1 Backend - User Preferences Storage
**Files to Create:**
- `Models/UserThemePreferences.cs` - User-specific settings
- `Api/UserThemePreferencesController.cs` - User preferences API

**Implementation:**
```csharp
// Models/UserThemePreferences.cs
public class UserThemePreferences
{
    public bool EnableThemeSongs { get; set; } = true;
    public int MaxDurationSeconds { get; set; } = 30; // 0 = full duration
    public float Volume { get; set; } = 1.0f; // 0.0 to 1.0
}

// Store in Jellyfin's user data directory
// Path: /config/data/users/{userId}/xThemeSong-preferences.json

// Api/UserThemePreferencesController.cs
[ApiController]
[Route("xThemeSong/preferences")]
public class UserThemePreferencesController : ControllerBase
{
    [HttpGet]
    public ActionResult<UserThemePreferences> GetPreferences()
    {
        var userId = GetUserId();
        return LoadUserPreferences(userId);
    }
    
    [HttpPost]
    public ActionResult SavePreferences([FromBody] UserThemePreferences prefs)
    {
        var userId = GetUserId();
        SaveUserPreferences(userId, prefs);
        return Ok();
    }
}
```

#### 3.2 Frontend - User Settings Integration
**Files to Create/Modify:**
- `web/userSettings.html` (new) - User preference page
- `web/xThemeSong.js` - Respect user preferences during playback

**Implementation:**
```javascript
// Integrate with Jellyfin's user settings
// Add new settings page accessible from user menu

// In playback logic:
async function playThemeSong(itemId) {
    var prefs = await getUserPreferences();
    
    if (!prefs.enableThemeSongs) {
        console.log('Theme songs disabled for this user');
        return;
    }
    
    var audio = new Audio(themeUrl);
    audio.volume = prefs.volume;
    
    if (prefs.maxDurationSeconds > 0) {
        setTimeout(() => audio.pause(), prefs.maxDurationSeconds * 1000);
    }
    
    audio.play();
}
```

#### 3.3 Settings UI
**Add to configPage.html or separate user settings:**
```html
<div class="checkboxContainer">
    <label class="emby-checkbox-label">
        <input id="EnableThemeSongs" type="checkbox" is="emby-checkbox" />
        <span>Play theme songs</span>
    </label>
</div>

<div class="inputContainer">
    <label for="MaxDuration">Maximum playback duration (seconds, 0 = full)</label>
    <input id="MaxDuration" type="number" is="emby-input" min="0" max="300" />
</div>

<div class="inputContainer">
    <label for="ThemeVolume">Theme song volume</label>
    <input id="ThemeVolume" type="range" min="0" max="100" value="100" />
</div>
```

### Testing
- ✅ User can disable themes entirely
- ✅ Duration limit works correctly (cuts off at specified time)
- ✅ Volume control applies properly
- ✅ Settings persist across sessions
- ✅ Different users have independent preferences

### Risks & Mitigation
- **Risk**: User settings not syncing across devices
- **Mitigation**: Store server-side, not in browser local storage
- **Risk**: Playback control may not work in all Jellyfin clients
- **Mitigation**: Document compatibility, graceful degradation

---

## Feature 4: Export/Import Theme Mappings

### Summary
Export all theme song assignments to JSON/CSV and import on another server for migration/backup.

### Requirements Analysis
- Export format: JSON with all metadata
- CSV format for spreadsheet editing
- Import with validation and conflict resolution
- Useful for: migrations, backups, bulk editing

### Implementation Steps

#### 4.1 Backend - Export/Import API
**Files to Create/Modify:**
- `Api/ThemeExportController.cs` - Export/import endpoints
- `Models/ThemeExportData.cs` - Export data model

**Implementation:**
```csharp
// Models/ThemeExportData.cs
public class ThemeExportData
{
    public string PluginVersion { get; set; }
    public DateTime ExportedAt { get; set; }
    public List<ThemeExportEntry> Themes { get; set; }
}

public class ThemeExportEntry
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; }
    public string ItemPath { get; set; }
    public string ItemType { get; set; } // Movie, Series, Season, etc.
    public ThemeMetadata Metadata { get; set; }
}

// Api/ThemeExportController.cs
[ApiController]
[Route("xThemeSong/export")]
public class ThemeExportController : ControllerBase
{
    [HttpGet("json")]
    [Produces("application/json")]
    public async Task<ActionResult<ThemeExportData>> ExportJson()
    {
        var themes = await CollectAllThemes();
        return new ThemeExportData
        {
            PluginVersion = Plugin.Instance.Version.ToString(),
            ExportedAt = DateTime.UtcNow,
            Themes = themes
        };
    }
    
    [HttpGet("csv")]
    [Produces("text/csv")]
    public async Task<FileResult> ExportCsv()
    {
        var csv = await GenerateCsv();
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", "xThemeSong-export.csv");
    }
    
    [HttpPost("import")]
    public async Task<ActionResult<ImportResult>> Import([FromBody] ThemeExportData data)
    {
        var result = await ImportThemes(data);
        return result;
    }
}

// ImportResult with conflict handling
public class ImportResult
{
    public int TotalThemes { get; set; }
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; }
    public List<ConflictInfo> Conflicts { get; set; }
}

public class ConflictInfo
{
    public string ItemName { get; set; }
    public string Reason { get; set; } // "PathMismatch", "AlreadyExists", etc.
    public string Action { get; set; } // What was done
}
```

#### 4.2 Frontend - Export/Import UI
**Files to Modify:**
- `Configuration/configPage.html` - Add export/import section

**Implementation:**
```html
<!-- Add new tab or section in Settings -->
<div class="verticalSection">
    <h3 class="sectionTitle">Backup & Migration</h3>
    
    <div class="fieldDescription">
        Export your theme song assignments for backup or migration to another server
    </div>
    
    <div style="margin-top: 1em;">
        <button is="emby-button" class="raised" id="btnExportJson">
            <span>📥 Export (JSON)</span>
        </button>
        <button is="emby-button" class="raised" id="btnExportCsv">
            <span>📥 Export (CSV)</span>
        </button>
    </div>
    
    <h4 style="margin-top: 2em;">Import Themes</h4>
    <div class="fieldDescription">
        Import theme assignments from a previous export
    </div>
    
    <input type="file" id="importFile" accept=".json" style="margin-top: 1em;" />
    <button is="emby-button" class="raised" id="btnImport" style="margin-top: 0.5em;">
        <span>📤 Import</span>
    </button>
    
    <div id="importResults" style="margin-top: 1em; display: none;">
        <!-- Shows import results -->
    </div>
</div>

<script>
document.getElementById('btnExportJson').addEventListener('click', async function() {
    var url = ApiClient.getUrl('xThemeSong/export/json');
    window.location.href = url + '?api_key=' + ApiClient.accessToken();
});

document.getElementById('btnExportCsv').addEventListener('click', async function() {
    var url = ApiClient.getUrl('xThemeSong/export/csv');
    window.location.href = url + '?api_key=' + ApiClient.accessToken();
});

document.getElementById('btnImport').addEventListener('click', async function() {
    var file = document.getElementById('importFile').files[0];
    if (!file) {
        Dashboard.alert('Please select a file to import');
        return;
    }
    
    var reader = new FileReader();
    reader.onload = async function(e) {
        try {
            var data = JSON.parse(e.target.result);
            var result = await importThemes(data);
            showImportResults(result);
        } catch (err) {
            Dashboard.alert('Error importing: ' + err.message);
        }
    };
    reader.readAsText(file);
});

async function importThemes(data) {
    var url = ApiClient.getUrl('xThemeSong/export/import');
    var response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Emby-Token': ApiClient.accessToken()
        },
        body: JSON.stringify(data)
    });
    return await response.json();
}

function showImportResults(result) {
    var html = '<h4>Import Results</h4>' +
        '<p>✅ Imported: ' + result.imported + '</p>' +
        '<p>⚠️ Skipped: ' + result.skipped + '</p>' +
        '<p>❌ Failed: ' + result.failed + '</p>';
    
    if (result.conflicts.length > 0) {
        html += '<h5>Conflicts:</h5><ul>';
        result.conflicts.forEach(function(c) {
            html += '<li>' + c.itemName + ': ' + c.reason + ' (' + c.action + ')</li>';
        });
        html += '</ul>';
    }
    
    document.getElementById('importResults').innerHTML = html;
    document.getElementById('importResults').style.display = 'block';
}
</script>
```

### Testing
- ✅ JSON export contains all themes with metadata
- ✅ CSV export is readable in spreadsheet apps
- ✅ Import creates theme.json files correctly
- ✅ Conflict detection works (path mismatches, duplicates)
- ✅ Import handles missing items gracefully
- ✅ Large exports (100+ themes) work without timeout

### Risks & Mitigation
- **Risk**: Path mismatches between servers
- **Mitigation**: Match by item name if path doesn't exist, show conflicts
- **Risk**: Version compatibility issues
- **Mitigation**: Version check in export format, migration logic
- **Risk**: Large imports may timeout
- **Mitigation**: Process in batches, show progress

---

## Development Priority & Phases

### Phase 1 (Highest Priority)
1. **Season/Collection-Level Themes** - Most requested, good user value
2. **Export/Import** - Critical for migrations and backups

### Phase 2 (Medium Priority)
3. **Per-User Preferences** - Nice to have, improves UX

### Phase 3 (Lower Priority)
4. **Role-Based Access** - Only needed in multi-user environments

---

## Testing Strategy for Each Feature

### General Testing Checklist
- [ ] Unit tests for new services/models
- [ ] Integration tests for API endpoints
- [ ] UI testing in actual Jellyfin environment
- [ ] Test with existing themes (migration path)
- [ ] Test with clean install
- [ ] Performance testing with large libraries (1000+ items)
- [ ] Cross-browser testing for UI changes
- [ ] Test on Docker, Windows, Linux

### Regression Testing (Existing Functionality)
After each feature implementation, verify:
- [ ] Individual movie themes still work
- [ ] Individual series themes still work
- [ ] Upload MP3 functionality works
- [ ] YouTube download works
- [ ] Scheduled task runs successfully
- [ ] Delete functionality works
- [ ] Media Library overview displays correctly
- [ ] Settings save and load correctly

---

## File Structure After Implementation

```
Jellyfin.Plugin.AssignThemeSong/
├── Api/
│   ├── ThemeSongController.cs (modified - permissions, hierarchy)
│   ├── UserThemePreferencesController.cs (new)
│   └── ThemeExportController.cs (new)
├── Models/
│   ├── ThemeMetadata.cs (modified - level, inheritance)
│   ├── UserThemePreferences.cs (new)
│   └── ThemeExportData.cs (new)
├── Services/
│   ├── ThemeHierarchyService.cs (new)
│   ├── ThemeExportService.cs (new)
│   └── ThemeDownloadService.cs (existing)
├── Configuration/
│   └── configPage.html (modified - permissions, export/import)
├── web/
│   ├── xThemeSong.js (modified - hierarchy support)
│   ├── userSettings.html (new)
│   └── plugin.js (existing)
└── PluginConfiguration.cs (modified - new settings)
```

---

## Estimated Development Time

| Feature | Backend | Frontend | Testing | Total |
|---------|---------|----------|---------|-------|
| Role-Based Access | 4 hrs | 2 hrs | 2 hrs | 8 hrs |
| Season/Collection Themes | 8 hrs | 4 hrs | 4 hrs | 16 hrs |
| Per-User Preferences | 6 hrs | 4 hrs | 2 hrs | 12 hrs |
| Export/Import | 6 hrs | 3 hrs | 3 hrs | 12 hrs |
| **Total** | **24 hrs** | **13 hrs** | **11 hrs** | **48 hrs** |

---

## Rollout Strategy

1. Implement features in separate branches
2. Test each feature thoroughly before merging
3. Release as minor version updates (1.2.0, 1.3.0, etc.)
4. Document breaking changes clearly
5. Provide migration guides for users
6. Collect feedback before next feature

---

## Documentation Updates Needed

- [ ] Update README.md with new features
- [ ] Create user guide for hierarchical themes
- [ ] Document export/import process
- [ ] Add FAQ for permission errors
- [ ] Update API documentation
