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
        /// Serves the user preferences HTML page (accessible to all users)
        /// </summary>
        [HttpGet("xThemeSongUserSettings/preferences")]
        [AllowAnonymous]  // Allow all authenticated users
        public IActionResult GetPreferencesView()
        {
            try
            {
                // Read the embedded HTML resource
                var assembly = GetType().Assembly;
                var resourceName = "Jellyfin.Plugin.xThemeSong.Configuration.userPreferences.html";
                
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    _logger.LogError("Could not find embedded resource: {ResourceName}", resourceName);
                    return NotFound("Preferences page not found");
                }

                using var reader = new StreamReader(stream);
                var html = reader.ReadToEnd();
                
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serving preferences view");
                return StatusCode(500, "Failed to load preferences page");
            }
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
        /// Get user's theme song preferences
        /// </summary>
        [HttpGet("xThemeSong/preferences")]
        [ProducesResponseType(typeof(UserThemePreferences), 200)]
        public ActionResult<UserThemePreferences> GetPreferences([FromQuery] string? userId = null)
        {
            try
            {
                // Use provided userId or fallback to current user
                Guid userGuid;
                if (string.IsNullOrEmpty(userId))
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out userGuid))
                    {
                        return Unauthorized("User ID not found");
                    }
                }
                else if (!Guid.TryParse(userId, out userGuid))
                {
                    return BadRequest("Invalid user ID");
                }

                var prefsPath = GetPreferencesPath(userGuid);

                UserThemePreferences prefs;

                if (System.IO.File.Exists(prefsPath))
                {
                    var json = System.IO.File.ReadAllText(prefsPath);
                    var decodeOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    prefs = JsonSerializer.Deserialize<UserThemePreferences>(json, decodeOptions) ?? new UserThemePreferences();
                }
                else
                {
                    // Return default preferences
                    prefs = new UserThemePreferences();
                }

                _logger.LogDebug("Retrieved preferences for user {UserId}", userGuid);
                return Ok(prefs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user preferences");
                return StatusCode(500, "Failed to get preferences");
            }
        }

        /// <summary>
        /// Save user's theme song preferences
        /// </summary>
        [HttpPost("xThemeSong/preferences")]
        public ActionResult SavePreferences([FromQuery] string? userId = null)
        {
            try
            {
                UserThemePreferences? preferences = null;
                try 
                {
                    using var reader = new StreamReader(Request.Body);
                    var jsonString = reader.ReadToEndAsync().Result;
                    
                    if (string.IsNullOrEmpty(jsonString))
                    {
                        return BadRequest("Preferences body is empty");
                    }

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    preferences = JsonSerializer.Deserialize<UserThemePreferences>(jsonString, options);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize preferences");
                    return BadRequest("Invalid preferences format");
                }

                if (preferences == null)
                {
                    return BadRequest("Preferences could not be parsed");
                }

                // Use provided userId or fallback to current user
                Guid userGuid;
                if (string.IsNullOrEmpty(userId))
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out userGuid))
                    {
                        return Unauthorized("User ID not found");
                    }
                }
                else if (!Guid.TryParse(userId, out userGuid))
                {
                    return BadRequest("Invalid user ID");
                }

                var prefsPath = GetPreferencesPath(userGuid);

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

                _logger.LogInformation("Saved preferences for user {UserId}", userGuid);
                return Ok(new { message = "Preferences saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user preferences");
                return StatusCode(500, "Failed to save preferences");
            }
        }
    }
}
