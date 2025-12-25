#nullable enable

using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.xThemeSong.Models
{
    /// <summary>
    /// Root export data container
    /// </summary>
    public class ThemeExportData
    {
        /// <summary>
        /// Plugin version that created this export
        /// </summary>
        public string PluginVersion { get; set; } = string.Empty;

        /// <summary>
        /// When this export was created
        /// </summary>
        public DateTime ExportedAt { get; set; }

        /// <summary>
        /// Total number of themes exported
        /// </summary>
        public int TotalThemes { get; set; }

        /// <summary>
        /// List of all theme song assignments
        /// </summary>
        public List<ThemeExportEntry> Themes { get; set; } = new List<ThemeExportEntry>();
    }

    /// <summary>
    /// Individual theme song entry in export
    /// </summary>
    public class ThemeExportEntry
    {
        /// <summary>
        /// Jellyfin item ID
        /// </summary>
        public Guid ItemId { get; set; }

        /// <summary>
        /// Media item name
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// Full path to media item
        /// </summary>
        public string ItemPath { get; set; } = string.Empty;

        /// <summary>
        /// Type of item (Movie, Series, etc.)
        /// </summary>
        public string ItemType { get; set; } = string.Empty;

        /// <summary>
        /// Theme metadata
        /// </summary>
        public ThemeMetadata? Metadata { get; set; }
    }

    /// <summary>
    /// Result of import operation
    /// </summary>
    public class ImportResult
    {
        /// <summary>
        /// Total themes in import file
        /// </summary>
        public int TotalThemes { get; set; }

        /// <summary>
        /// Number successfully imported
        /// </summary>
        public int Imported { get; set; }

        /// <summary>
        /// Number skipped (already exists, no overwrite)
        /// </summary>
        public int Skipped { get; set; }

        /// <summary>
        /// Number that failed
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// List of error messages
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// List of conflicts encountered
        /// </summary>
        public List<ConflictInfo> Conflicts { get; set; } = new List<ConflictInfo>();
    }

    /// <summary>
    /// Conflict information during import
    /// </summary>
    public class ConflictInfo
    {
        /// <summary>
        /// Name of the item with conflict
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// Reason for conflict
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Action taken
        /// </summary>
        public string Action { get; set; } = string.Empty;
    }
}
