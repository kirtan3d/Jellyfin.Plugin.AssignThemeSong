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
            prefsLink.href = '#/configurationpage?name=xThemeSong%20User%20Preferences';
            prefsLink.className = 'listItem-border emby-button';
            prefsLink.style.display = 'block';
            prefsLink.style.padding = '0';
            prefsLink.style.margin = '0';
            
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
    
    function initializePlugin() {
        console.log('xThemeSong: Initializing plugin...');
        
        // Add link to user preferences menu
        addUserPreferencesLink();
        
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
        
        // Get item details to verify it's a Movie or Series
        if (window.ApiClient) {
            window.ApiClient.getItem(window.ApiClient.getCurrentUserId(), itemId).then(function(item) {
                if (item && (item.Type === 'Movie' || item.Type === 'Series')) {
                    addMenuItemToActionSheet(actionSheet, itemId);
                }
            }).catch(function(err) {
                console.error('xThemeSong: Error getting item', err);
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
