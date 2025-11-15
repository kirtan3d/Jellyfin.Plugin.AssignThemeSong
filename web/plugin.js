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
        console.log('xThemeSong: Initializing three-dot menu integration...');
        
        // Hook into Jellyfin's itemMenu system
        if (!window.itemHelper) {
            console.warn('xThemeSong: itemHelper not available yet, retrying...');
            setTimeout(initializePlugin, 500);
            return;
        }
        
        // Store original menu function
        const originalShowItemMenu = window.showItemMenu;
        
        // Override showItemMenu to add our custom menu item
        window.showItemMenu = function(item, options) {
            console.log('xThemeSong: showItemMenu called for item:', item?.Type);
            
            // Check if this is a Movie or Series
            if (item && (item.Type === 'Movie' || item.Type === 'Series')) {
                // Add our custom menu item to the options
                if (!options) options = {};
                if (!options.commands) options.commands = [];
                
                // Add theme song menu item
                options.commands.push({
                    name: 'Assign Theme Song',
                    id: 'assignthemesong',
                    icon: 'music_note',
                    callback: function() {
                        console.log('xThemeSong: Menu item clicked for', item.Id);
                        if (window.xThemeSongDialog && window.xThemeSongDialog.show) {
                            window.xThemeSongDialog.show(item.Id);
                        } else {
                            console.error('xThemeSong: Dialog not available');
                        }
                    }
                });
                
                console.log('xThemeSong: Added menu item to three-dot menu');
            }
            
            // Call original function
            if (originalShowItemMenu) {
                return originalShowItemMenu.call(this, item, options);
            }
        };
        
        console.log('xThemeSong: Three-dot menu hook installed successfully');
    }
})();
