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
        // Wait for Jellyfin to be ready
        function waitForJellyfin(callback) {
            if (window.ApiClient && window.ApiClient.getCurrentUserId) {
                callback();
            } else {
                setTimeout(() => waitForJellyfin(callback), 100);
            }
        }
        
        waitForJellyfin(function() {
            console.log('xThemeSong: Jellyfin API ready');
            setupPageObserver();
        });
    }
    
    function setupPageObserver() {
        // Observe for page changes
        const observer = new MutationObserver(function() {
            checkAndAddButton();
        });
        
        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
        
        // Also check on initial load
        checkAndAddButton();
        
        // Listen for Jellyfin's view events
        document.addEventListener('viewshow', checkAndAddButton);
    }
    
    function checkAndAddButton() {
        // Check if we're on a detail page
        const detailButtons = document.querySelector('.mainDetailButtons');
        
        if (!detailButtons) {
            return;
        }
        
        // Check if button already exists
        if (detailButtons.querySelector('.btnxThemeSong')) {
            return;
        }
        
        // Get item ID from URL
        const urlParams = new URLSearchParams(window.location.search);
        const itemId = urlParams.get('id');
        
        if (!itemId) {
            return;
        }
        
        // Get item to check if it's a Movie or Series
        ApiClient.getItem(ApiClient.getCurrentUserId(), itemId).then(function(item) {
            if (item.Type === 'Movie' || item.Type === 'Series') {
                addThemeSongButton(detailButtons, itemId);
            }
        }).catch(function(error) {
            console.error('xThemeSong: Error getting item', error);
        });
    }
    
    function addThemeSongButton(container, itemId) {
        console.log('xThemeSong: Adding button for item', itemId);
        
        // Create the button
        const button = document.createElement('button');
        button.setAttribute('is', 'emby-button');
        button.setAttribute('type', 'button');
        button.className = 'button-flat btnxThemeSong detailButton emby-button';
        button.setAttribute('title', 'xTheme Song');
        button.innerHTML = `
            <div class="detailButton-content">
                <span class="material-icons detailButton-icon music_note" aria-hidden="true"></span>
                <span>Theme Song</span>
            </div>
        `;
        
        // Add click handler
        button.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
        if (window.xThemeSongDialog && window.xThemeSongDialog.show) {
            window.xThemeSongDialog.show(itemId);
        } else {
            console.error('xThemeSong: Dialog not available');
        }
        });
        
        // Insert button
        const moreButton = container.querySelector('.btnMoreCommands');
        if (moreButton) {
            container.insertBefore(button, moreButton);
        } else {
            container.appendChild(button);
        }
        
        console.log('xThemeSong: Button added successfully');
    }
})();
