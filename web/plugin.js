(function() {
    'use strict';
    
    console.log('xThemeSong: Plugin script loaded');
    
    // Load xThemeSong module
    const script = document.createElement('script');
    script.src = '/xThemeSong/xThemeSong';
    script.onload = function() {
        console.log('xThemeSong: Module loaded, initializing...');
        initializePlugin();
    };
    script.onerror = function() {
        console.error('xThemeSong: Failed to load xThemeSong module');
    };
    document.head.appendChild(script);
    
    function initializePlugin() {
        console.log('xThemeSong: Initializing menu observer...');
        
        // Check for existing action sheets first
        const existingActionSheets = document.querySelectorAll('.actionSheet');
        console.log('xThemeSong: Found ' + existingActionSheets.length + ' existing action sheets');
        existingActionSheets.forEach(function(actionSheet) {
            onActionSheetOpened(actionSheet);
        });
        
        // Observe for new action sheet menus appearing
        const observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                mutation.addedNodes.forEach(function(node) {
                    if (node.nodeType === 1 && node.classList && node.classList.contains('actionSheet')) {
                        console.log('xThemeSong: New action sheet detected via MutationObserver');
                        onActionSheetOpened(node);
                    }
                });
            });
        });
        
        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
        
        console.log('xThemeSong: Menu observer installed');
    }
    
    function onActionSheetOpened(actionSheet) {
        console.log('xThemeSong: Action sheet detected');
        
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
            
            // Close the action sheet
            if (window.dialogHelper) {
                const dialog = actionSheet.closest('.dialog');
                if (dialog) {
                    window.dialogHelper.close(dialog);
                }
            }
            
            // Show our dialog
            if (window.xThemeSongDialog && window.xThemeSongDialog.show) {
                window.xThemeSongDialog.show(itemId);
            } else {
                console.error('xThemeSong: Dialog not available');
            }
        });
        
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
