#nullable enable

using System;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using Jellyfin.Plugin.xThemeSong.Models;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.xThemeSong.Api
{
    /// <summary>
    /// User Theme Preferences Controller - allows users to manage their own theme song preferences
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("xThemeSong/preferences")]
    public class UserThemePreferencesController : ControllerBase
    {
        private readonly ILogger<UserThemePreferencesController> _logger;
        private readonly IApplicationPaths _appPaths;
        private readonly IUserManager _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserThemePreferencesController"/> class.
        /// </summary>
        public UserThemePreferencesController(
            ILogger<UserThemePreferencesController> logger,
            IApplicationPaths appPaths,
            IUserManager userManager)
        {
            _logger = logger;
            _appPaths = appPaths;
            _userManager = userManager;
        }

        /// <summary>
        /// Gets the current user's ID from claims
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            
            throw new UnauthorizedAccessException("User ID not found");
        }

        /// <summary>
        /// Gets the preferences file path for a user
        /// </summary>
        private string GetPreferencesPath(Guid userId)
        {
            var userDataPath = Path.Combine(_appPaths.DataPath, "users", userId.ToString("N"));
            Directory.CreateDirectory(userDataPath);
            return Path.Combine(userDataPath, "xThemeSong-preferences.json");
        }

        /// <summary>
        /// Get current user's theme song preferences
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(UserThemePreferences), 200)]
        public ActionResult<UserThemePreferences> GetPreferences()
        {
            try
            {
                var userId = GetCurrentUserId();
                var prefsPath = GetPreferencesPath(userId);

                UserThemePreferences prefs;

                if (System.IO.File.Exists(prefsPath))
                {
                    var json = System.IO.File.ReadAllText(prefsPath);
                    prefs = JsonSerializer.Deserialize<UserThemePreferences>(json) ?? new UserThemePreferences();
                }
                else
                {
                    // Return default preferences
                    prefs = new UserThemePreferences();
                }

                _logger.LogDebug("Retrieved preferences for user {UserId}", userId);
                return Ok(prefs);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to preferences");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user preferences");
                return StatusCode(500, "Failed to get preferences");
            }
        }

        /// <summary>
        /// Save current user's theme song preferences
        /// </summary>
        [HttpPost]
        public ActionResult SavePreferences([FromBody] UserThemePreferences preferences)
        {
            try
            {
                var userId = GetCurrentUserId();
                var prefsPath = GetPreferencesPath(userId);

                // Update last modified
                preferences.LastModified = DateTime.UtcNow;

                // Validate values
                if (preferences.Volume < 0) preferences.Volume = 0;
                if (preferences.Volume > 1) preferences.Volume = 1;
                if (preferences.MaxDurationSeconds < 0) preferences.MaxDurationSeconds = 0;

                // Save to file
                var json = JsonSerializer.Serialize(preferences, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                System.IO.File.WriteAllText(prefsPath, json);

                _logger.LogInformation("Saved preferences for user {UserId}", userId);
                return Ok(new { message = "Preferences saved successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to save preferences");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user preferences");
                return StatusCode(500, "Failed to save preferences");
            }
        }
    }
}
