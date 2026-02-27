(function() {
    'use strict';
    
    console.log('xThemeSong: Plugin script loaded');
    
    // Load xThemeSong module
    const script = document.createElement('script');
    script.src = '/xThemeSong/xThemeSong';
    script.onload = function() {
        console.log('xThemeSong: Module loaded, waiting for page ready...');
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                console.log('xThemeSong: DOM ready, initializing...');
                initializePlugin();
            });
        } else {
            console.log('xThemeSong: DOM already ready, initializing...');
            initializePlugin();
        }
    };
    script.onerror = function() {
        console.error('xThemeSong: Failed to load xThemeSong module');
    };
    document.head.appendChild(script);
    
    const processedActionSheets = new WeakSet();
    let userPreferences = { enableThemeSongs: true, maxDurationSeconds: 0, volume: 1.0 };
    let themeSongAudioElement = null;
    let themeSongWatchdogInterval = null;

    // ========================
    // SIDEBAR MENU LINK (button, not anchor -- avoids Jellyfin's SPA router hash issue)
    // ========================
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
                const prefsBtn = document.createElement('button');
                prefsBtn.type = 'button';
                prefsBtn.id = 'xThemeSongPreferencesLink';
                prefsBtn.className = 'lnkMediaFolder navMenuOption emby-button';
                prefsBtn.style.background = 'none';
                prefsBtn.style.border = 'none';
                prefsBtn.style.width = '100%';
                prefsBtn.style.textAlign = 'left';
                prefsBtn.style.cursor = 'pointer';
                prefsBtn.style.padding = '0.5em 1em';
                prefsBtn.addEventListener('click', function(e) {
                    e.preventDefault();
                    e.stopPropagation();
                    showUserPreferencesModal();
                });
                prefsBtn.innerHTML = `
                    <span class="material-icons navMenuOptionIcon" aria-hidden="true" style="margin-right:0.5em; vertical-align:middle;">music_note</span>
                    <span class="sectionName navMenuOptionText">xThemeSong Preferences</span>
                `;

                pluginSettingsSection.appendChild(prefsBtn);
                console.log('xThemeSong: Sidebar menu button added to pluginMenuOptions');
            }
        };

        const observer = new MutationObserver(function() {
            const sidebar = document.querySelector('.mainDrawer-scrollContainer');
            if (sidebar && !sidebar.querySelector('#xThemeSongPreferencesLink')) {
                addMenuButton(sidebar);
            }
        });
        observer.observe(document.body, { childList: true, subtree: true });
        
        const sidebar = document.querySelector('.mainDrawer-scrollContainer');
        if (sidebar) addMenuButton(sidebar);
    }

    // ========================
    // USER PREFERENCES MENU LINK (button in user settings dropdown)
    // ========================
    function addUserPreferencesLink() {
        const addLinkToMenu = function() {
            const menuContainer = document.querySelector('#myPreferencesMenuPage:not(.hide) .verticalSection');
            if (!menuContainer) return false;
            if (document.querySelector('#xThemeSongUserPrefsLink')) return true;

            const prefsBtn = document.createElement('button');
            prefsBtn.id = 'xThemeSongUserPrefsLink';
            prefsBtn.type = 'button';
            prefsBtn.className = 'listItem-border emby-button';
            prefsBtn.style.display = 'block';
            prefsBtn.style.padding = '0';
            prefsBtn.style.margin = '0';
            prefsBtn.style.width = '100%';
            prefsBtn.style.background = 'none';
            prefsBtn.style.border = 'none';
            prefsBtn.style.cursor = 'pointer';
            prefsBtn.style.textAlign = 'left';
            
            prefsBtn.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                const userMenu = document.querySelector('#myPreferencesMenuPage');
                if (userMenu && window.dialogHelper) {
                    const popover = userMenu.closest('.dialog');
                    if (popover) window.dialogHelper.close(popover);
                }
                showUserPreferencesModal();
            });
            
            prefsBtn.innerHTML = `
                <div class="listItem" style="display:flex; align-items:center; padding: 0.5em 1em;">
                    <span class="material-icons listItemIcon listItemIcon-transparent" aria-hidden="true" style="margin-right:0.5em;">music_note</span>
                    <div class="listItemBody">
                        <div class="listItemBodyText">xThemeSong Preferences</div>
                    </div>
                </div>
            `;

            menuContainer.appendChild(prefsBtn);
            console.log('xThemeSong: User preferences button added to user menu');
            return true;
        };

        if (addLinkToMenu()) return;

        const observer = new MutationObserver(function() {
            if (addLinkToMenu()) observer.disconnect();
        });
        observer.observe(document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ['class'] });
        console.log('xThemeSong: User preferences menu observer installed');
    }

    // ========================
    // FETCH USER PREFERENCES FROM SERVER
    // ========================
    function fetchUserPreferences() {
        if (!window.ApiClient) return Promise.resolve(userPreferences);
        
        var userId = window.ApiClient.getCurrentUserId();
        if (!userId) {
            return new Promise((resolve) => setTimeout(() => resolve(fetchUserPreferences()), 2000));
        }

        var url = window.ApiClient.getUrl('xThemeSong/preferences?userId=' + userId);
        
        return fetch(url, {
            headers: { 'X-Emby-Token': window.ApiClient.accessToken() }
        }).then(response => {
            if (response.ok) return response.json();
            throw new Error('Failed to fetch preferences');
        }).then(prefs => {
            // Handle both camelCase and PascalCase
            userPreferences = {
                enableThemeSongs: prefs.enableThemeSongs !== undefined ? prefs.enableThemeSongs : (prefs.EnableThemeSongs !== false),
                maxDurationSeconds: prefs.maxDurationSeconds !== undefined ? prefs.maxDurationSeconds : (prefs.MaxDurationSeconds || 0),
                volume: prefs.volume !== undefined ? prefs.volume : (prefs.Volume !== undefined ? prefs.Volume : 1.0)
            };
            console.log('xThemeSong: Loaded user preferences', userPreferences);
            return userPreferences;
        }).catch(err => {
            console.warn('xThemeSong: Failed to load user preferences', err);
            return userPreferences;
        });
    }

    // ========================
    // GET/SET JELLYFIN'S NATIVE THEME SONG SETTING (DisplayPreferences)
    // ========================
    function getJellyfinThemeSongEnabled(userId) {
        if (!window.ApiClient) return Promise.resolve(true);
        return window.ApiClient.getDisplayPreferences('usersettings', userId, 'emby')
            .then(function(prefs) {
                const val = prefs.CustomPrefs && prefs.CustomPrefs['enableThemeSong'];
                // Jellyfin stores as string '1'/'0' or boolean
                return val === undefined || val === null ? true : (val === '1' || val === true || val === 'true');
            }).catch(() => true);
    }

    function setJellyfinThemeSongEnabled(userId, enabled) {
        if (!window.ApiClient) return Promise.resolve();
        return window.ApiClient.getDisplayPreferences('usersettings', userId, 'emby')
            .then(function(prefs) {
                if (!prefs.CustomPrefs) prefs.CustomPrefs = {};
                prefs.CustomPrefs['enableThemeSong'] = enabled ? '1' : '0';
                return window.ApiClient.updateDisplayPreferences('usersettings', prefs, userId, 'emby');
            }).catch(err => console.warn('xThemeSong: Failed to update Jellyfin theme song setting', err));
    }

    // ========================
    // INTERCEPT THEME SONG PLAYBACK FOR VOLUME + DURATION CONTROL
    // ========================
    function interceptThemeSongPlayback() {
        console.log('xThemeSong: Setting up playback interceptor...');
        const originalPlay = window.HTMLMediaElement.prototype.play;
        
        window.HTMLMediaElement.prototype.play = function() {
            var url = this.src || this.currentSrc || '';
            if (url.indexOf('/ThemeMedia/') !== -1 || url.indexOf('/Audio/') !== -1 && url.indexOf('theme') !== -1) {
                console.log('xThemeSong: Theme song playback detected.', url);
                themeSongAudioElement = this;

                // If theme songs disabled, block
                if (!userPreferences.enableThemeSongs) {
                    console.log('xThemeSong: Theme songs disabled by user preference, blocking playback.');
                    this.muted = true;
                    return Promise.resolve();
                }

                // More precise check for /ThemeMedia/ which is how Jellyfin serves theme songs
                if (url.indexOf('/ThemeMedia/') !== -1) {
                    this.volume = userPreferences.volume;
                    console.log('xThemeSong: Set theme song volume to', userPreferences.volume);

                    if (userPreferences.maxDurationSeconds > 0) {
                        if (themeSongWatchdogInterval) clearTimeout(themeSongWatchdogInterval);
                        themeSongWatchdogInterval = setTimeout(() => {
                            if (themeSongAudioElement && !themeSongAudioElement.paused) {
                                themeSongAudioElement.pause();
                                console.log('xThemeSong: Max duration reached, stopped theme song');
                            }
                        }, userPreferences.maxDurationSeconds * 1000);
                        this.addEventListener('pause', () => clearTimeout(themeSongWatchdogInterval), { once: true });
                        this.addEventListener('ended', () => clearTimeout(themeSongWatchdogInterval), { once: true });
                    }
                }
            }
            
            return originalPlay.apply(this, arguments);
        };
    }

    // ========================
    // SHOW USER PREFERENCES MODAL
    // ========================
    function showUserPreferencesModal() {
        console.log('xThemeSong: Opening user preferences modal');
        
        if (!window.ApiClient) {
            console.error('xThemeSong: ApiClient not available');
            return;
        }
        
        const userId = window.ApiClient.getCurrentUserId();
        
        // Create a dialog manually since dynamic import may fail in some contexts
        const dlgContainer = document.createElement('div');
        dlgContainer.className = 'dialogContainer';
        dlgContainer.style.cssText = 'position:fixed;top:0;left:0;right:0;bottom:0;z-index:9999;display:flex;align-items:center;justify-content:center;background:rgba(0,0,0,0.7);';
        dlgContainer.id = 'xThemeSongPrefsContainer';
        
        dlgContainer.innerHTML = `
            <div class="dialog formDialog" style="background:#1a1a1a;border-radius:8px;max-width:460px;width:90%;max-height:90vh;overflow:auto;box-shadow:0 4px 20px rgba(0,0,0,0.5);">
                <div class="formDialogHeader" style="display:flex;align-items:center;padding:1em 1.5em;border-bottom:1px solid #333;">
                    <button id="xThemeSongPrefClose" type="button" style="background:none;border:none;color:#ccc;cursor:pointer;margin-right:0.5em;font-size:1.2em;">&#8592;</button>
                    <h3 style="margin:0;color:#fff;flex:1;">xThemeSong Preferences</h3>
                </div>
                <div style="padding:1.5em;">
                    <p style="color:#aaa;margin-top:0;">Control how theme songs play for you personally</p>
                    
                    <div style="margin-bottom:1.5em;">
                        <label style="display:flex;align-items:center;cursor:pointer;color:#fff;">
                            <input id="xPrefEnableThemeSongs" type="checkbox" style="margin-right:0.75em;width:18px;height:18px;cursor:pointer;" />
                            <span><strong>Enable Theme Songs</strong></span>
                        </label>
                        <p style="color:#888;font-size:0.85em;margin:0.25em 0 0 1.75em;">Turn theme songs on or off for your account<br/><em>Also syncs with Jellyfin's native theme songs setting.</em></p>
                    </div>
                    
                    <div style="margin-bottom:1.5em;">
                        <label style="display:block;color:#ccc;margin-bottom:0.5em;">Maximum Playback Duration (seconds)</label>
                        <input id="xPrefMaxDuration" type="number" min="0" max="300" step="5" style="width:100%;box-sizing:border-box;padding:8px;background:#2a2a2a;border:1px solid #444;color:#fff;border-radius:4px;" />
                        <p style="color:#888;font-size:0.85em;margin:0.25em 0 0;">0 = play full theme</p>
                    </div>
                    
                    <div style="margin-bottom:2em;">
                        <label style="display:block;color:#ccc;margin-bottom:0.5em;">Theme Song Volume (%): <span id="xPrefVolDisplay">100%</span></label>
                        <input id="xPrefVolume" type="range" min="0" max="100" step="5" style="width:100%;cursor:pointer;" />
                    </div>
                    
                    <div id="xPrefStatus" style="margin-bottom:1em;min-height:1.5em;"></div>
                    
                    <button id="xThemeSongPrefSave" type="button" style="width:100%;padding:0.75em;background:#00a4dc;color:#fff;border:none;border-radius:4px;cursor:pointer;font-size:1em;font-weight:bold;">Save Preferences</button>
                </div>
            </div>
        `;

        document.body.appendChild(dlgContainer);

        const volInput = dlgContainer.querySelector('#xPrefVolume');
        const volDisplay = dlgContainer.querySelector('#xPrefVolDisplay');
        const enableCheckbox = dlgContainer.querySelector('#xPrefEnableThemeSongs');
        const maxDuration = dlgContainer.querySelector('#xPrefMaxDuration');
        const statusEl = dlgContainer.querySelector('#xPrefStatus');

        // Initialize with cached values
        enableCheckbox.checked = userPreferences.enableThemeSongs;
        maxDuration.value = userPreferences.maxDurationSeconds;
        volInput.value = Math.round(userPreferences.volume * 100);
        volDisplay.textContent = volInput.value + '%';

        // Async: Load fresh preferences from server AND from Jellyfin native settings
        Promise.all([
            fetchUserPreferences(),
            getJellyfinThemeSongEnabled(userId)
        ]).then(function(results) {
            const prefs = results[0];
            const jellyfinEnabled = results[1];
            
            // Use Jellyfin's native setting as the source of truth for enableThemeSongs
            enableCheckbox.checked = jellyfinEnabled;
            maxDuration.value = prefs.maxDurationSeconds;
            volInput.value = Math.round(prefs.volume * 100);
            volDisplay.textContent = volInput.value + '%';
        });

        // Volume range update
        volInput.addEventListener('input', function() {
            volDisplay.textContent = this.value + '%';
        });

        // Close button
        dlgContainer.querySelector('#xThemeSongPrefClose').addEventListener('click', function() {
            document.body.removeChild(dlgContainer);
        });

        // Close on backdrop click
        dlgContainer.addEventListener('click', function(e) {
            if (e.target === dlgContainer) document.body.removeChild(dlgContainer);
        });

        // Save
        dlgContainer.querySelector('#xThemeSongPrefSave').addEventListener('click', function() {
            const saveBtn = this;
            saveBtn.disabled = true;
            saveBtn.textContent = 'Saving...';
            statusEl.textContent = '';
            statusEl.style.color = '';

            const newEnabled = enableCheckbox.checked;
            const newVolume = parseInt(volInput.value) / 100.0;
            const newMaxDuration = parseInt(maxDuration.value) || 0;

            const bodyPayload = {
                enableThemeSongs: newEnabled,
                maxDurationSeconds: newMaxDuration,
                volume: newVolume
            };

            const url = window.ApiClient.getUrl('xThemeSong/preferences?userId=' + userId);

            // Save to our API AND to Jellyfin's native DisplayPreferences
            Promise.all([
                fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Emby-Token': window.ApiClient.accessToken()
                    },
                    body: JSON.stringify(bodyPayload)
                }).then(r => { if (!r.ok) throw new Error('Server error'); return r.json(); }),
                setJellyfinThemeSongEnabled(userId, newEnabled)
            ]).then(function() {
                // Update local cache
                userPreferences = bodyPayload;
                saveBtn.disabled = false;
                saveBtn.textContent = 'Save Preferences';
                statusEl.style.color = '#4caf50';
                statusEl.textContent = '✓ Preferences saved successfully!';
                setTimeout(() => { document.body.removeChild(dlgContainer); }, 1500);
            }).catch(function(err) {
                saveBtn.disabled = false;
                saveBtn.textContent = 'Save Preferences';
                statusEl.style.color = '#f44336';
                statusEl.textContent = '✗ Error saving preferences: ' + err.message;
                console.error('xThemeSong: Save error', err);
            });
        });
    }

    // ========================
    // PLUGIN INITIALIZATION
    // ========================
    function initializePlugin() {
        console.log('xThemeSong: Initializing plugin...');
        
        fetchUserPreferences();
        interceptThemeSongPlayback();
        addUserPreferencesLink();
        addSidebarMenuLink();
        
        const existingActionSheets = document.querySelectorAll('.actionSheet');
        existingActionSheets.forEach(function(actionSheet) { onActionSheetOpened(actionSheet); });
        
        const observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                mutation.addedNodes.forEach(function(node) {
                    if (node.nodeType === 1) {
                        if (node.classList && node.classList.contains('actionSheet')) {
                            onActionSheetOpened(node);
                        } else if (node.querySelectorAll) {
                            node.querySelectorAll('.actionSheet').forEach(function(as) { onActionSheetOpened(as); });
                        }
                    }
                });
                if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                    if (mutation.target.classList && mutation.target.classList.contains('actionSheet')) {
                        onActionSheetOpened(mutation.target);
                    }
                }
            });
        });
        
        observer.observe(document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ['class'] });
        console.log('xThemeSong: Initialized successfully');
    }

    // ========================
    // ACTION SHEET HANDLING (Three-dot menu for assigning theme songs)
    // ========================
    function onActionSheetOpened(actionSheet) {
        if (processedActionSheets.has(actionSheet)) return;
        
        console.log('xThemeSong: Action sheet detected');
        processedActionSheets.add(actionSheet);
        
        const urlParams = new URLSearchParams(window.location.hash.split('?')[1] || '');
        const itemId = urlParams.get('id');
        
        if (!itemId) return;
        if (!window.location.hash.includes('#/details')) return;
        if (actionSheet.querySelector('[data-id="assignthemesong"]')) return;
        
        if (window.ApiClient) {
            var pluginId = '97db7543-d64d-45e1-b2e7-62729b56371f';
            
            Promise.all([
                window.ApiClient.getPluginConfiguration(pluginId),
                window.ApiClient.getCurrentUser ? window.ApiClient.getCurrentUser() : Promise.resolve(null)
            ]).then(function(results) {
                var config = results[0];
                var currentUser = results[1];
                
                // Handle both int and string values of PermissionMode
                var rawMode = config.PermissionMode !== undefined && config.PermissionMode !== null ? config.PermissionMode : 1;
                var permissionMode = NaN;
                if (typeof rawMode === 'number') {
                    permissionMode = rawMode;
                } else if (typeof rawMode === 'string') {
                    if (rawMode === 'AdminsOnly') permissionMode = 0;
                    else if (rawMode === 'LibraryManagers') permissionMode = 1;
                    else if (rawMode === 'Everyone') permissionMode = 2;
                    else permissionMode = parseInt(rawMode);
                }
                if (isNaN(permissionMode)) permissionMode = 1;
                
                var isAdmin = currentUser && currentUser.Policy && currentUser.Policy.IsAdministrator;
                
                var hasPermission = false;
                if (permissionMode === 0) hasPermission = isAdmin;
                else if (permissionMode === 1) hasPermission = isAdmin;
                else if (permissionMode === 2) hasPermission = true;
                else hasPermission = isAdmin;
                
                console.log('xThemeSong: Permission check - Mode:', permissionMode, 'IsAdmin:', isAdmin, 'HasPermission:', hasPermission);
                
                if (!hasPermission) {
                    console.log('xThemeSong: User does not have permission to manage themes');
                    return;
                }
                
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
        const scroller = actionSheet.querySelector('.actionSheetScroller');
        if (!scroller) return;
        
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
        
        menuItem.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            closeActionSheet(actionSheet);
            if (window.xThemeSongDialog && window.xThemeSongDialog.show) {
                window.xThemeSongDialog.show(itemId);
            }
        });
        
        const divider = scroller.querySelector('.actionsheetDivider');
        if (divider) scroller.insertBefore(menuItem, divider);
        else scroller.appendChild(menuItem);
        
        console.log('xThemeSong: Menu item added');
    }

    function closeActionSheet(actionSheet) {
        if (window.dialogHelper) {
            const dialog = actionSheet.closest('.dialog');
            if (dialog) window.dialogHelper.close(dialog);
        }
        actionSheet.style.display = 'none';
        document.querySelectorAll('.backdrop, .backdropFadeIn, .dialogBackdrop, .dialogBackdropOpened').forEach(backdrop => {
            if (backdrop.parentNode) backdrop.parentNode.removeChild(backdrop);
        });
        const parentDialog = actionSheet.closest('.dialog, .dialogContainer');
        if (parentDialog) {
            parentDialog.style.display = 'none';
            if (parentDialog.parentNode) parentDialog.parentNode.removeChild(parentDialog);
        }
    }
})();
