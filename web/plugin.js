(function() {
    'use strict';
    
    console.log('Assign Theme Song: Plugin script loaded');
    
    // Load assignThemeSong module
    const script = document.createElement('script');
    script.src = '/AssignThemeSong/assignThemeSong';
    script.onload = function() {
        console.log('Assign Theme Song: Module loaded, initializing...');
        initializePlugin();
    };
    script.onerror = function() {
        console.error('Assign Theme Song: Failed to load assignThemeSong module');
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
            console.log('Assign Theme Song: Jellyfin API ready');
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
        if (detailButtons.querySelector('.btnAssignThemeSong')) {
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
            console.error('Assign Theme Song: Error getting item', error);
        });
    }
    
    function addThemeSongButton(container, itemId) {
        console.log('Assign Theme Song: Adding button for item', itemId);
        
        // Create the button
        const button = document.createElement('button');
        button.setAttribute('is', 'emby-button');
        button.setAttribute('type', 'button');
        button.className = 'button-flat btnAssignThemeSong detailButton emby-button';
        button.setAttribute('title', 'Assign Theme Song');
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
            
            if (window.AssignThemeSongDialog && window.AssignThemeSongDialog.show) {
                window.AssignThemeSongDialog.show(itemId);
            } else {
                console.error('Assign Theme Song: Dialog not available');
            }
        });
        
        // Insert button
        const moreButton = container.querySelector('.btnMoreCommands');
        if (moreButton) {
            container.insertBefore(button, moreButton);
        } else {
            container.appendChild(button);
        }
        
        console.log('Assign Theme Song: Button added successfully');
    }
})();
