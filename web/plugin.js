(function() {
    'use strict';
    
    console.log('xThemeSong: Plugin script loaded');
    
    // Load xThemeSong module
    const script = document.createElement('script');
    script.src = '/xThemeSong/xThemeSong';
    script.onload = function() {
        console.log('xThemeSong: Module loaded, waiting for page ready...');
        // Wait for document to be fully ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                console.log('xThemeSong: DOM ready, initializing...');
                initializePlugin();
            });
        } else {
            // DOM is already ready
            console.log('xThemeSong: DOM already ready, initializing...');
            initializePlugin();
        }
    };
    script.onerror = function() {
        console.error('xThemeSong: Failed to load xThemeSong module');
    };
    document.head.appendChild(script);
    
    // Track processed action sheets to avoid duplicates
    const processedActionSheets = new WeakSet();
    
    /**
     * Adds the "xThemeSong Preferences" menu button to the sidebar under pluginMenuOptions.
     * Based on Jellyfin Enhanced pattern from study.md
     */
    function addSidebarMenuLink() {
        const addMenuButton = function(sidebar) {
            let pluginSettingsSection = sidebar.querySelector('.pluginMenuOptions');

            if (!pluginSettingsSection) {
                pluginSettingsSection = document.createElement('div');
                pluginSettingsSection.className = 'pluginMenuOptions';
                pluginSettingsSection.innerHTML = '<h3 class="sidebarHeader">Plugin Settings</h3>';

                const settingsSection = sidebar.querySelector('.navMenuOption[href*="settings"]')?.closest('.drawerSection');
                if (settingsSection && settingsSection.nextSibling) {
                    sidebar.insertBefore(pluginSettingsSection, settingsSection.nextSibling);
                } else {
                    sidebar.appendChild(pluginSettingsSection);
                }
            }

            if (!pluginSettingsSection.querySelector('#xThemeSongPreferencesLink')) {
                const prefsLink = document.createElement('a');
                prefsLink.setAttribute('is', 'emby-linkbutton');
                prefsLink.className = 'lnkMediaFolder navMenuOption emby-button';
                prefsLink.href = '#'; // Replaced with inline JS modal
                prefsLink.id = 'xThemeSongPreferencesLink';
                prefsLink.onclick = function(e) {
                    e.preventDefault();
                    showUserPreferencesModal();
                };
                prefsLink.innerHTML = `
                    <span class="material-icons navMenuOptionIcon" aria-hidden="true">music_note</span>
                    <span class="sectionName navMenuOptionText">xThemeSong Preferences</span>
                `;

                pluginSettingsSection.appendChild(prefsLink);
                console.log('xThemeSong: Sidebar menu link added to pluginMenuOptions');
            }
        };

        const observer = new MutationObserver(function() {
            const sidebar = document.querySelector('.mainDrawer-scrollContainer');
            if (sidebar && !sidebar.querySelector('#xThemeSongPreferencesLink')) {
                addMenuButton(sidebar);
            }
        });

        observer.observe(document.body, { childList: true, subtree: true });
        
        // Try to add immediately if sidebar already exists
        const sidebar = document.querySelector('.mainDrawer-scrollContainer');
        if (sidebar) {
            addMenuButton(sidebar);
        }
    }
    
    /**
     * Injects the "xThemeSong Preferences" link into the user preferences menu (mypreferencesmenu.html).
     * Adds it as the last item in the first vertical section (after Controls).
     */
    function addUserPreferencesLink() {
        const addLinkToMenu = function() {
            const menuContainer = document.querySelector('#myPreferencesMenuPage:not(.hide) .verticalSection');
            if (!menuContainer) return false;
            
            // Check if link already exists
            if (document.querySelector('#xThemeSongUserPrefsLink')) return true;

            // Create the link element matching Jellyfin's structure
            const prefsLink = document.createElement('a');
            prefsLink.id = 'xThemeSongUserPrefsLink';
            prefsLink.setAttribute('is', 'emby-linkbutton');
            prefsLink.setAttribute('data-ripple', 'false');
            prefsLink.href = '#'; // Trigger modal directly
            prefsLink.className = 'listItem-border emby-button';
            prefsLink.style.display = 'block';
            prefsLink.style.padding = '0';
            prefsLink.style.margin = '0';
            
            prefsLink.onclick = function(e) {
                e.preventDefault();
                // Close the user menu dropdown first if possible
                const userMenu = document.querySelector('#myPreferencesMenuPage');
                if (userMenu && window.dialogHelper) {
                    // Try different ways to close the popover
                    const popover = userMenu.closest('.dialog');
                    if (popover) window.dialogHelper.close(popover);
                }
                showUserPreferencesModal();
            };
            
            prefsLink.innerHTML = `
                <div class="listItem">
                    <span class="material-icons listItemIcon listItemIcon-transparent music_note" aria-hidden="true"></span>
                    <div class="listItemBody">
                        <div class="listItemBodyText">xThemeSong Preferences</div>
                    </div>
                </div>
            `;

            // Insert at the end of the first vertical section
            menuContainer.appendChild(prefsLink);
            console.log('xThemeSong: User preferences link added to user menu');
            return true;
        };

        // Try to add immediately
        if (addLinkToMenu()) return;

        // If not found, observe for when the menu is loaded and visible
        const observer = new MutationObserver(function() {
            if (addLinkToMenu()) {
                observer.disconnect();
            }
        });

        observer.observe(document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ['class'] });
        console.log('xThemeSong: User preferences menu observer installed');
    }
    
    let userPreferences = { enableThemeSongs: true, maxDurationSeconds: 0, volume: 1.0 };
    let themeSongAudioElement = null;
    let themeSongWatchdogInterval = null;

    /**
     * Fetch user preferences from server
     */
    function fetchUserPreferences() {
        if (!window.ApiClient) return Promise.resolve(userPreferences);
        
        var userId = window.ApiClient.getCurrentUserId();
        if (!userId) {
            // Wait and try again if userId is not ready during startup
            console.log('xThemeSong: User not logged in, deferring fetch...');
            return new Promise((resolve) => setTimeout(() => resolve(fetchUserPreferences()), 2000));
        }

        console.log('xThemeSong: Fetching user preferences...');
        var url = window.ApiClient.getUrl('xThemeSong/preferences?userId=' + userId);
        
        return fetch(url, {
            headers: { 'X-Emby-Token': window.ApiClient.accessToken() }
        }).then(response => {
            if (response.ok) return response.json();
            throw new Error('Failed');
        }).then(prefs => {
            userPreferences = {
                enableThemeSongs: prefs.EnableThemeSongs !== false,
                maxDurationSeconds: prefs.MaxDurationSeconds || 0,
                volume: prefs.Volume !== undefined ? prefs.Volume : 1.0
            };
            console.log('xThemeSong: Loaded user preferences', userPreferences);
            return userPreferences;
        }).catch(err => {
            console.warn('xThemeSong: Failed to load user preferences', err);
            return userPreferences;
        });
    }

    /**
     * Render formatting helper for Native Javascript Modal Preferences
     */
    function showUserPreferencesModal() {
        console.log('xThemeSong: Showing user preferences modal');

        import('https://' + window.location.host + '/web/components/dialogHelper/dialogHelper.js').then((DialogHelperModule) => {
            const dialogHelper = DialogHelperModule.default || window.dialogHelper;
            if (!dialogHelper) {
                console.error('xThemeSong: dialogHelper not found');
                return;
            }

            const dlg = dialogHelper.createDialog({
                size: 'small',
                removeOnClose: true,
                scrollY: false
            });

            dlg.classList.add('formDialog');

            const html = `
                <div class="formDialogHeader">
                    <button is="paper-icon-button-light" class="btnCancel autoSize paper-icon-button-light" tabindex="-1" title="Back"><span class="material-icons arrow_back" aria-hidden="true"></span></button>
                    <h3 class="formDialogHeaderTitle">xThemeSong Preferences</h3>
                </div>
                <div class="formDialogContent smoothScrollY" style="padding: 2em; overflow-y: auto;">
                    <form id="xThemeSongUserPrefsForm" style="margin: 0; padding-bottom: 2em;">
                        <div class="fieldDescription" style="margin-bottom: 1.5em; text-align: center;">
                            Control how theme songs play for you personally
                        </div>

                        <div class="checkboxContainer checkboxContainer-withDescription">
                            <label class="emby-checkbox-label">
                                <input id="EnableThemeSongsInput" name="EnableThemeSongs" type="checkbox" is="emby-checkbox" />
                                <span>Enable Theme Songs</span>
                            </label>
                            <div class="fieldDescription checkboxFieldDescription">
                                Turn theme songs on or off for your account
                            </div>
                        </div>
                        <br/>

                        <div class="inputContainer">
                            <label class="inputLabel inputLabelUnfocused" for="MaxDurationSecondsInput">Maximum Playback Duration (seconds)</label>
                            <input id="MaxDurationSecondsInput" name="MaxDurationSeconds" type="number" is="emby-input" min="0" max="300" step="5" class="emby-input" style="width: 100%; box-sizing: border-box; background: var(--theme-background);" />
                            <div class="fieldDescription">
                                Limit how long theme songs play (0 = play full theme)
                            </div>
                        </div>
                        <br/>

                        <div class="inputContainer">
                            <label class="inputLabel inputLabelUnfocused" for="VolumePercentInput">Theme Song Volume (%)</label>
                            <input id="VolumePercentInput" name="VolumePercent" type="range" is="emby-input" min="0" max="100" step="5" style="width: 100%;" />
                            <div class="fieldDescription">
                                <span id="volumeDisplaySpan">100%</span>
                            </div>
                        </div>

                        <br />
                        <br />
                        <div class="formDialogFooter">
                            <button is="emby-button" type="submit" class="raised button-submit block formDialogFooterItem emby-button" style="width: 100%;">
                                <span>Save Preferences</span>
                            </button>
                        </div>
                    </form>
                </div>
            `;

            dlg.innerHTML = html;

            // Handle back button
            const btnCancel = dlg.querySelector('.btnCancel');
            if (btnCancel) {
                btnCancel.addEventListener('click', function() {
                    dialogHelper.close(dlg);
                });
            }

            // Sync range slider display dynamically
            const volInput = dlg.querySelector('#VolumePercentInput');
            const volDisp = dlg.querySelector('#volumeDisplaySpan');
            if (volInput && volDisp) {
                volInput.addEventListener('input', function() {
                    volDisp.textContent = this.value + '%';
                });
            }

            // Show Loading while data parses
            const EnableThemeSongsInput = dlg.querySelector('#EnableThemeSongsInput');
            const MaxDurationSecondsInput = dlg.querySelector('#MaxDurationSecondsInput');
            
            // Populate initially from cache
            EnableThemeSongsInput.checked = userPreferences.enableThemeSongs;
            MaxDurationSecondsInput.value = userPreferences.maxDurationSeconds;
            volInput.value = Math.round(userPreferences.volume * 100);
            volDisp.textContent = volInput.value + '%';

            // Background fetch fresh config from API just in case...
            fetchUserPreferences().then(prefs => {
                EnableThemeSongsInput.checked = prefs.enableThemeSongs;
                MaxDurationSecondsInput.value = prefs.maxDurationSeconds;
                volInput.value = Math.round(prefs.volume * 100);
                volDisp.textContent = volInput.value + '%';
            });

            // Handle Save
            const form = dlg.querySelector('#xThemeSongUserPrefsForm');
            form.addEventListener('submit', function(e) {
                e.preventDefault();
                const saveBtn = form.querySelector('.button-submit');
                const oldText = saveBtn.textContent;
                saveBtn.disabled = true;
                saveBtn.textContent = 'Saving...';

                // Evaluate volume explicitly securely
                const volumePercentEval = parseInt(volInput.value) || 100;
                const bodyPayload = {
                    enableThemeSongs: EnableThemeSongsInput.checked,
                    maxDurationSeconds: parseInt(MaxDurationSecondsInput.value) || 0,
                    volume: volumePercentEval / 100.0
                };

                const userId = window.ApiClient.getCurrentUserId();
                const url = window.ApiClient.getUrl('xThemeSong/preferences?userId=' + userId);

                fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Emby-Token': window.ApiClient.accessToken()
                    },
                    body: JSON.stringify(bodyPayload)
                }).then(response => {
                    saveBtn.disabled = false;
                    saveBtn.textContent = oldText;
                    if (response.ok) {
                        // Immediately update local UI cache!
                        userPreferences = bodyPayload;
                        
                        // Use native Jellyfin toast for clean UX instead of alert alerts
                        if (window.Dashboard && window.Dashboard.alert) {
                            window.Dashboard.alert('Preferences saved successfully!');
                        } else {
                            alert('Preferences saved successfully!');
                        }
                        dialogHelper.close(dlg);
                    } else {
                        throw new Error('Server returned an error');
                    }
                }).catch(err => {
                    saveBtn.disabled = false;
                    saveBtn.textContent = oldText;
                    console.error('xThemeSong:', err);
                    if (window.Dashboard && window.Dashboard.alert) {
                        window.Dashboard.alert('Error saving preferences: ' + err.message);
                    } else {
                        alert('Error saving preferences.');
                    }
                });
            });

            dialogHelper.open(dlg);
        }).catch(err => {
            console.error('xThemeSong: Failed to load dialogHelper module dynamically', err);
            // Fallback native
            if (window.dialogHelper) {
               // Ignore and let standard load evaluate usually dialogs inherently load DialogHelper dynamically via ES module mappings
            }
        });
    }

    /**
     * Intercept HTMLAudioElement to apply theme song preferences
     */
    function interceptThemeSongPlayback() {
        console.log("xThemeSong: Intercepting Theme Song playback mechanics...");
        const originalPlay = window.HTMLMediaElement.prototype.play;
        
        window.HTMLMediaElement.prototype.play = function() {
            var url = this.src || this.currentSrc || "";
            // Check if this audio/media element is playing a theme song (api path /ThemeMedia/)
            if (url.indexOf('/ThemeMedia/') !== -1) {
                console.log('xThemeSong: Theme song playback detected.', url);
                themeSongAudioElement = this;

                if (!userPreferences.enableThemeSongs) {
                    console.log('xThemeSong: Theme songs disabled by user preference, blocking playback.');
                    // Pause/Mute immediately just in case
                    this.muted = true;
                    this.pause();
                    return Promise.resolve();
                }

                // Apply volume
                this.volume = userPreferences.volume;
                console.log('xThemeSong: Set theme song volume to', userPreferences.volume);

                // Handle duration limitation
                if (userPreferences.maxDurationSeconds > 0) {
                    if (themeSongWatchdogInterval) clearTimeout(themeSongWatchdogInterval);
                    
                    themeSongWatchdogInterval = setTimeout(() => {
                        console.log(`xThemeSong: Max duration (${userPreferences.maxDurationSeconds}s) reached. Stopping theme song.`);
                        if (themeSongAudioElement && !themeSongAudioElement.paused) {
                            themeSongAudioElement.pause();
                        }
                    }, userPreferences.maxDurationSeconds * 1000);

                    // clear it if user stops it manually or it ends
                    this.addEventListener('pause', () => clearTimeout(themeSongWatchdogInterval), { once: true });
                    this.addEventListener('ended', () => clearTimeout(themeSongWatchdogInterval), { once: true });
                }
            }
            
            return originalPlay.apply(this, arguments);
        };
    }

    function initializePlugin() {
        console.log('xThemeSong: Initializing plugin...');
        
        // Fetch preferences
        fetchUserPreferences();

        // Setup Player Interception
        interceptThemeSongPlayback();

        // Add link to user preferences menu (user menu dropdown)
        addUserPreferencesLink();
        
        // Add link to sidebar menu (pluginMenuOptions)
        addSidebarMenuLink();
        
        // Check for existing action sheets first
        const existingActionSheets = document.querySelectorAll('.actionSheet');
        console.log('xThemeSong: Found ' + existingActionSheets.length + ' existing action sheets');
        existingActionSheets.forEach(function(actionSheet) {
            onActionSheetOpened(actionSheet);
        });
        
        // Observe for new action sheet menus appearing
        const observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                // Check for added nodes
                mutation.addedNodes.forEach(function(node) {
                    if (node.nodeType === 1) {
                        // Check if this node has actionSheet class
                        if (node.classList && node.classList.contains('actionSheet')) {
                            console.log('xThemeSong: New action sheet detected (direct add)');
                            onActionSheetOpened(node);
                        }
                        // Also check children in case it's a parent container
                        else if (node.querySelectorAll) {
                            const actionSheets = node.querySelectorAll('.actionSheet');
                            if (actionSheets.length > 0) {
                                console.log('xThemeSong: Found ' + actionSheets.length + ' action sheets in added node');
                                actionSheets.forEach(function(as) {
                                    onActionSheetOpened(as);
                                });
                            }
                        }
                    }
                });
                
                // Check for class changes
                if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                    const target = mutation.target;
                    if (target.classList && target.classList.contains('actionSheet')) {
                        console.log('xThemeSong: Action sheet class added to element');
                        onActionSheetOpened(target);
                    }
                }
            });
        });
        
        observer.observe(document.body, {
            childList: true,
            subtree: true,
            attributes: true,
            attributeFilter: ['class']
        });
        
        console.log('xThemeSong: Menu observer installed');
    }
    
    function onActionSheetOpened(actionSheet) {
        // Skip if already processed
        if (processedActionSheets.has(actionSheet)) {
            console.log('xThemeSong: Action sheet already processed, skipping');
            return;
        }
        
        console.log('xThemeSong: Action sheet detected');
        
        // Mark as processed
        processedActionSheets.add(actionSheet);
        
        // Get the current page URL to extract item ID
        const urlParams = new URLSearchParams(window.location.hash.split('?')[1] || '');
        const itemId = urlParams.get('id');
        
        if (!itemId) {
            console.log('xThemeSong: No item ID in URL');
            return;
        }
        
        // Check if this is a movie or series page
        if (!window.location.hash.includes('#/details')) {
            return;
        }
        
        // Check if our menu item already exists
        if (actionSheet.querySelector('[data-id="assignthemesong"]')) {
            console.log('xThemeSong: Menu item already exists');
            return;
        }
        
        // Check permissions first
        if (window.ApiClient) {
            // Get plugin configuration to check permission mode
            var pluginId = '97db7543-d64d-45e1-b2e7-62729b56371f';
            
            Promise.all([
                window.ApiClient.getPluginConfiguration(pluginId),
                window.ApiClient.getCurrentUser ? window.ApiClient.getCurrentUser() : Promise.resolve(null)
            ]).then(function(results) {
                var config = results[0];
                var currentUser = results[1];
                
                // Convert to number in case it's a string
                var permissionMode = parseInt((config.PermissionMode !== undefined && config.PermissionMode !== null) ? config.PermissionMode : 1);
                var isAdmin = currentUser && currentUser.Policy && currentUser.Policy.IsAdministrator;
                
                console.log('xThemeSong: Permission check - Mode:', permissionMode, '(type:', typeof permissionMode, '), IsAdmin:', isAdmin);
                
                // Determine if user has permission
                var hasPermission = false;
                if (permissionMode === 0) {
                    // AdminsOnly
                    hasPermission = isAdmin;
                } else if (permissionMode === 1) {
                    // LibraryManagers (admins only for now)
                    hasPermission = isAdmin;
                } else if (permissionMode === 2) {
                    // Everyone
                    hasPermission = true;
                } else {
                    // Default: allow admins
                    hasPermission = isAdmin;
                }
                
                console.log('xThemeSong: HasPermission:', hasPermission);
                
                if (!hasPermission) {
                    console.log('xThemeSong: User does not have permission to manage themes');
                    return;
                }
                
                console.log('xThemeSong: User has permission, checking item type');
                
                // User has permission, check item type
                window.ApiClient.getItem(window.ApiClient.getCurrentUserId(), itemId).then(function(item) {
                    if (item && (item.Type === 'Movie' || item.Type === 'Series')) {
                        addMenuItemToActionSheet(actionSheet, itemId);
                    }
                }).catch(function(err) {
                    console.error('xThemeSong: Error getting item', err);
                });
            }).catch(function(err) {
                console.error('xThemeSong: Error in permission check', err);
            });
        }
    }
    
    function addMenuItemToActionSheet(actionSheet, itemId) {
        console.log('xThemeSong: Adding menu item for', itemId);
        
        // Find the action sheet scroller
        const scroller = actionSheet.querySelector('.actionSheetScroller');
        if (!scroller) {
            console.warn('xThemeSong: Could not find actionSheetScroller');
            return;
        }
        
        // Create the menu item button
        const menuItem = document.createElement('button');
        menuItem.setAttribute('is', 'emby-button');
        menuItem.setAttribute('type', 'button');
        menuItem.className = 'listItem listItem-button actionSheetMenuItem emby-button';
        menuItem.setAttribute('data-id', 'assignthemesong');
        
        menuItem.innerHTML = `
            <span class="actionsheetMenuItemIcon listItemIcon listItemIcon-transparent material-icons music_note" aria-hidden="true"></span>
            <div class="listItemBody actionsheetListItemBody">
                <div class="listItemBodyText actionSheetItemText">Assign Theme Song</div>
            </div>
        `;
        
        // Add click handler
        menuItem.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            console.log('xThemeSong: Menu item clicked');
            
            // Close the action sheet using multiple methods
            closeActionSheet(actionSheet);
            
            // Show our dialog
            if (window.xThemeSongDialog && window.xThemeSongDialog.show) {
                window.xThemeSongDialog.show(itemId);
            } else {
                console.error('xThemeSong: Dialog not available');
            }
        });
        
        function closeActionSheet(actionSheet) {
            console.log('xThemeSong: Closing action sheet');
            
            // Method 1: Use dialogHelper if available
            if (window.dialogHelper) {
                const dialog = actionSheet.closest('.dialog');
                if (dialog) {
                    window.dialogHelper.close(dialog);
                }
            }
            
            // Method 2: Hide the action sheet directly
            actionSheet.style.display = 'none';
            
            // Method 3: Remove all types of backdrop overlays
            const backdrops = document.querySelectorAll('.backdrop, .backdropFadeIn, .dialogBackdrop, .dialogBackdropOpened');
            backdrops.forEach(backdrop => {
                if (backdrop.parentNode) {
                    backdrop.parentNode.removeChild(backdrop);
                }
            });
            
            // Method 4: Close any parent dialogs and containers
            const parentDialog = actionSheet.closest('.dialog, .dialogContainer');
            if (parentDialog) {
                parentDialog.style.display = 'none';
                if (parentDialog.parentNode) {
                    parentDialog.parentNode.removeChild(parentDialog);
                }
            }
            
            // Method 5: Remove focus container if it exists
            const focusContainer = document.querySelector('.focuscontainer.dialog');
            if (focusContainer) {
                focusContainer.style.display = 'none';
                if (focusContainer.parentNode) {
                    focusContainer.parentNode.removeChild(focusContainer);
                }
            }
        }
        
        // Find a good place to insert - after "Share" or before first divider
        const divider = scroller.querySelector('.actionsheetDivider');
        if (divider) {
            scroller.insertBefore(menuItem, divider);
        } else {
            scroller.appendChild(menuItem);
        }
        
        console.log('xThemeSong: Menu item added successfully');
    }
})();
