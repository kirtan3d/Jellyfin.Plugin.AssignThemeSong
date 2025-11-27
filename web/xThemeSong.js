(function() {
    'use strict';
    
    console.log('xThemeSong: Dialog module loaded');
    
    function showThemeSongDialog(itemId) {
        console.log('xThemeSong: Opening dialog for item', itemId);
        
        // Close any open action sheets first
        closeActionSheets();
        
        // Enhanced dialog system detection
        if (isJellyfinDialogSystemAvailable()) {
            console.log('xThemeSong: Jellyfin dialog system available, using it');
            createJellyfinDialog(itemId);
        } else {
            console.log('xThemeSong: Jellyfin dialog system not available, using fallback');
            createSimpleDialog(itemId);
        }
    }
    
    function isJellyfinDialogSystemAvailable() {
        // Check multiple ways Jellyfin dialog system might be available
        if (window.dialogHelper && window.dialogHelper.createDialog) {
            return true;
        }
        
        // Check if we can access dialogHelper through RequireJS
        if (typeof window.require !== 'undefined') {
            try {
                // Try to require dialogHelper synchronously (if possible)
                const dialogHelper = window.require('dialogHelper');
                if (dialogHelper && dialogHelper.createDialog) {
                    window.dialogHelper = dialogHelper; // Make it globally available
                    return true;
                }
            } catch (e) {
                console.log('xThemeSong: RequireJS dialogHelper not available:', e.message);
            }
        }
        
        // Check if we're in a Jellyfin environment with dialog capabilities
        if (window.Emby && window.Emby.Page) {
            return true;
        }
        
        return false;
    }
    
    function closeActionSheets() {
        console.log('xThemeSong: Closing action sheets');
        // Close any open action sheets (three-dot menus)
        const actionSheets = document.querySelectorAll('.actionSheet');
        actionSheets.forEach(sheet => {
            if (sheet.style.display !== 'none') {
                sheet.style.display = 'none';
            }
        });
        
        // Also close any dialogs that might be overlays
        const dialogs = document.querySelectorAll('.dialog');
        dialogs.forEach(dialog => {
            if (dialog.style.display !== 'none' && dialog.classList.contains('actionSheet')) {
                dialog.style.display = 'none';
            }
        });
        
        // Remove any backdrop overlays
        const backdrops = document.querySelectorAll('.backdrop, .backdropFadeIn');
        backdrops.forEach(backdrop => {
            if (backdrop.parentNode) {
                backdrop.parentNode.removeChild(backdrop);
            }
        });
    }
    
    function createJellyfinDialog(itemId) {
        console.log('xThemeSong: Using Jellyfin dialog system');
        
        // Try with RequireJS first
        if (typeof window.require !== 'undefined') {
            try {
                window.require(['loading', 'dialogHelper'], function(loading, dialogHelper) {
                    if (dialogHelper && dialogHelper.createDialog) {
                        createDialogWithHelper(itemId, dialogHelper, loading);
                    } else {
                        console.log('xThemeSong: dialogHelper not available after require, using fallback');
                        createSimpleDialog(itemId);
                    }
                });
            } catch (e) {
                console.log('xThemeSong: RequireJS error:', e);
                createSimpleDialog(itemId);
            }
        } else if (window.dialogHelper && window.dialogHelper.createDialog) {
            // Use global dialogHelper directly
            createDialogWithHelper(itemId, window.dialogHelper, window.loading);
        } else {
            console.log('xThemeSong: No dialog system available, using fallback');
            createSimpleDialog(itemId);
        }
    }
    
    function createDialogWithHelper(itemId, dialogHelper, loading) {
        if (!dialogHelper || !dialogHelper.createDialog) {
            console.log('xThemeSong: dialogHelper.createDialog not available, using fallback');
            createSimpleDialog(itemId);
            return;
        }
        
        var dlg = dialogHelper.createDialog({
            size: 'medium',
            removeOnClose: true,
            scrollY: false
        });

        dlg.classList.add('formDialog');
        dlg.classList.add('themeSongDialog');
        
        var html = '';
        html += '<div class="formDialogHeader">';
        html += '<button is="paper-icon-button-light" class="btnCancel autoSize" tabindex="-1">';
        html += '<i class="md-icon">arrow_back</i>';
        html += '</button>';
        html += '<h3 class="formDialogHeaderTitle">xTheme Song</h3>';
        html += '</div>';

        html += '<div class="formDialogContent smoothScrollY">';
        html += '<div class="dialogContentInner dialog-content-centered">';
        html += '<form style="margin: auto;">';
        
        // Existing theme song section
        html += '<div id="existingThemeSection" style="margin-bottom: 2em; display: none;">';
        html += '<h4>Current Theme Song</h4>';
        html += '<audio id="existingThemePlayer" controls style="width: 100%; margin-top: 0.5em;"></audio>';
        html += '<div id="themeMetadata" style="margin-top: 0.5em; font-size: 0.9em; color: #888;"></div>';
        html += '</div>';
        
        // YouTube URL input
        html += '<div class="inputContainer" style="margin-bottom: 1.5em;">';
        html += '<label class="inputLabel" for="txtYouTubeUrl">YouTube URL or Video ID</label>';
        html += '<input is="emby-input" type="text" id="txtYouTubeUrl" placeholder="e.g., https://www.youtube.com/watch?v=dQw4w9WgXcQ" style="width: 100%;" />';
        html += '<div class="fieldDescription">Enter a YouTube video URL or video ID to download the theme song</div>';
        html += '</div>';

        // File upload with drag and drop
        html += '<div class="inputContainer" style="margin-bottom: 1.5em;">';
        html += '<label class="inputLabel">Or Upload MP3 File</label>';
        html += '<div id="dropZone" style="border: 2px dashed #555; border-radius: 8px; padding: 2em; text-align: center; cursor: pointer; transition: all 0.3s;">';
        html += '<i class="md-icon" style="font-size: 3em; color: #888;">cloud_upload</i>';
        html += '<p style="margin: 0.5em 0;">Drag and drop an MP3 file here</p>';
        html += '<p style="margin: 0; font-size: 0.9em; color: #888;">or</p>';
        html += '<button type="button" id="btnBrowse" class="raised button-submit" style="margin-top: 1em;">';
        html += '<span>Browse Files</span>';
        html += '</button>';
        html += '<input type="file" id="fileUpload" accept=".mp3,audio/mpeg" style="display: none;" />';
        html += '</div>';
        html += '<div id="fileInfo" style="margin-top: 0.5em; font-size: 0.9em; color: #888; display: none;"></div>';
        html += '</div>';

        html += '<div class="formDialogFooter">';
        html += '<button is="emby-button" type="submit" class="raised button-submit block">';
        html += '<span>Save Theme Song</span>';
        html += '</button>';
        html += '</div>';
        
        html += '</form>';
        html += '</div>';
        html += '</div>';

        dlg.innerHTML = html;

        // CSS for drag and drop highlight
        var style = document.createElement('style');
        style.textContent = `
            .themeSongDialog .dropZoneActive {
                border-color: #00a4dc !important;
                background-color: rgba(0, 164, 220, 0.1);
            }
            .themeSongDialog .dropZone:hover {
                border-color: #777;
                background-color: rgba(255, 255, 255, 0.05);
           }
        `;
        document.head.appendChild(style);

        dialogHelper.open(dlg);

        // Load existing theme song if it exists
        loadExistingTheme(itemId, dlg);

        // File upload elements
        var dropZone = dlg.querySelector('#dropZone');
        var fileInput = dlg.querySelector('#fileUpload');
        var browseBtn = dlg.querySelector('#btnBrowse');
        var fileInfo = dlg.querySelector('#fileInfo');
        var selectedFile = null;

        // Browse button click
        browseBtn.addEventListener('click', function (e) {
            e.preventDefault();
            fileInput.click();
        });

        // File input change
        fileInput.addEventListener('change', function () {
            if (this.files.length > 0) {
                selectedFile = this.files[0];
                showFileInfo(selectedFile, fileInfo);
            }
        });

        // Drag and drop events
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(function (eventName) {
            dropZone.addEventListener(eventName, preventDefaults, false);
        });

        function preventDefaults(e) {
            e.preventDefault();
            e.stopPropagation();
        }

        ['dragenter', 'dragover'].forEach(function (eventName) {
            dropZone.addEventListener(eventName, function () {
                dropZone.classList.add('dropZoneActive');
            }, false);
        });

        ['dragleave', 'drop'].forEach(function (eventName) {
            dropZone.addEventListener(eventName, function () {
                dropZone.classList.remove('dropZoneActive');
            }, false);
        });

        dropZone.addEventListener('drop', function (e) {
            var dt = e.dataTransfer;
            var files = dt.files;
            if (files.length > 0) {
                var file = files[0];
                if (file.type === 'audio/mpeg' || file.name.endsWith('.mp3')) {
                    selectedFile = file;
                    fileInput.files = files;
                    showFileInfo(file, fileInfo);
                } else {
                    alert('Please drop an MP3 file.');
                }
            }
        });

        // Form submit
        dlg.querySelector('form').addEventListener('submit', function (e) {
            e.preventDefault();
            
            var youtubeUrl = dlg.querySelector('#txtYouTubeUrl').value.trim();

            if (!youtubeUrl && !selectedFile) {
                alert('Please provide either a YouTube URL or upload an MP3 file.');
                return;
            }

            if (loading && loading.show) {
                loading.show();
            }

            var formData = new FormData();
            
            if (youtubeUrl) {
                formData.append('YouTubeUrl', youtubeUrl);
            }
            if (selectedFile) {
                formData.append('UploadedFile', selectedFile);
            }

            // Use the correct API endpoint
            var apiUrl = ApiClient.getUrl('xThemeSong/' + itemId);

            fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'X-Emby-Token': ApiClient.accessToken()
                },
                body: formData
            }).then(function (response) {
                if (loading && loading.hide) {
                    loading.hide();
                }
                if (response.ok) {
                    dialogHelper.close(dlg);
                    if (typeof window.require !== 'undefined') {
                        window.require(['toast'], function (toast) {
                            toast('Theme song assigned successfully!');
                        });
                    } else {
                        alert('Theme song assigned successfully!');
                    }
                } else {
                    return response.text().then(function (text) {
                        throw new Error(text || 'Failed to assign theme song');
                    });
                }
            }).catch(function (error) {
                if (loading && loading.hide) {
                    loading.hide();
                }
                alert('Error: ' + (error.message || 'Failed to assign theme song'));
            });
        });

        // Cancel button
        dlg.querySelector('.btnCancel').addEventListener('click', function () {
            dialogHelper.close(dlg);
        });
    }

    function createSimpleDialog(itemId) {
        console.log('xThemeSong: Using simple fallback dialog');
        
        // Create a simple modal dialog as fallback
        var overlay = document.createElement('div');
        overlay.style.cssText = 'position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.7);z-index:10000;display:flex;align-items:center;justify-content:center;';
        
        var dialog = document.createElement('div');
        dialog.style.cssText = 'background:#2b2b2b;padding:20px;border-radius:8px;max-width:500px;width:90%;max-height:80vh;overflow-y:auto;';
        
        dialog.innerHTML = `
            <h3 style="margin-top:0;color:#fff;">xTheme Song - Fallback Mode</h3>
            <p style="color:#ccc;">Jellyfin dialog system not available. Please use the API directly or try refreshing the page.</p>
            <div style="margin:15px 0;">
                <label style="color:#fff;display:block;margin-bottom:5px;">YouTube URL or Video ID:</label>
                <input type="text" id="simpleYoutubeUrl" placeholder="e.g., https://www.youtube.com/watch?v=dQw4w9WgXcQ" style="width:100%;padding:8px;border-radius:4px;border:1px solid #555;background:#333;color:#fff;">
            </div>
            <div style="margin:15px 0;">
                <label style="color:#fff;display:block;margin-bottom:5px;">Or upload MP3 file:</label>
                <input type="file" id="simpleFileUpload" accept=".mp3,audio/mpeg" style="width:100%;padding:8px;border-radius:4px;border:1px solid #555;background:#333;color:#fff;">
            </div>
            <div style="display:flex;gap:10px;justify-content:flex-end;">
                <button id="simpleCancel" style="padding:8px 16px;background:#555;color:#fff;border:none;border-radius:4px;cursor:pointer;">Cancel</button>
                <button id="simpleSubmit" style="padding:8px 16px;background:#00a4dc;color:#fff;border:none;border-radius:4px;cursor:pointer;">Save Theme Song</button>
            </div>
        `;
        
        overlay.appendChild(dialog);
        document.body.appendChild(overlay);
        
        // Event handlers for simple dialog
        document.getElementById('simpleCancel').addEventListener('click', function() {
            document.body.removeChild(overlay);
        });
        
        document.getElementById('simpleSubmit').addEventListener('click', function() {
            var youtubeUrl = document.getElementById('simpleYoutubeUrl').value.trim();
            var fileInput = document.getElementById('simpleFileUpload');
            var selectedFile = fileInput.files[0];
            
            if (!youtubeUrl && !selectedFile) {
                alert('Please provide either a YouTube URL or upload an MP3 file.');
                return;
            }
            
            var formData = new FormData();
            if (youtubeUrl) formData.append('YouTubeUrl', youtubeUrl);
            if (selectedFile) formData.append('UploadedFile', selectedFile);
            
            var apiUrl = ApiClient.getUrl('xThemeSong/' + itemId);
            
            fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'X-Emby-Token': ApiClient.accessToken()
                },
                body: formData
            }).then(function (response) {
                if (response.ok) {
                    document.body.removeChild(overlay);
                    alert('Theme song assigned successfully!');
                } else {
                    return response.text().then(function (text) {
                        throw new Error(text || 'Failed to assign theme song');
                    });
                }
            }).catch(function (error) {
                alert('Error: ' + (error.message || 'Failed to assign theme song'));
            });
        });
    }

    function showFileInfo(file, fileInfoElement) {
        var sizeInMB = (file.size / 1024 / 1024).toFixed(2);
        fileInfoElement.textContent = 'Selected: ' + file.name + ' (' + sizeInMB + ' MB)';
        fileInfoElement.style.display = 'block';
    }

    function loadExistingTheme(itemId, dlg) {
        // Try to get the item and check for existing theme
        ApiClient.getItem(ApiClient.getCurrentUserId(), itemId).then(function (item) {
            // Check if theme.mp3 exists by trying to load it
            var themePath = ApiClient.getUrl('Items/' + itemId + '/ThemeMedia');
            
            fetch(themePath, {
                headers: {
                    'X-Emby-Token': ApiClient.accessToken()
                }
            }).then(function (response) {
                if (response.ok) {
                    return response.json();
                }
                throw new Error('No theme');
            }).then(function (data) {
                if (data && data.ThemeVideosResult && data.ThemeVideosResult.Items && data.ThemeVideosResult.Items.length > 0) {
                    var themeItem = data.ThemeVideosResult.Items[0];
                    var audioUrl = ApiClient.getUrl('Audio/' + themeItem.Id + '/stream');
                    
                    var existingSection = dlg.querySelector('#existingThemeSection');
                    var audioPlayer = dlg.querySelector('#existingThemePlayer');
                    var metadataDiv = dlg.querySelector('#themeMetadata');
                    
                    existingSection.style.display = 'block';
                    audioPlayer.src = audioUrl;
                    
                    if (themeItem.Name) {
                        metadataDiv.textContent = 'Title: ' + themeItem.Name;
                    }
                }
            }).catch(function () {
                // No existing theme, that's okay
            });
        });
    }
    
    // Export to global scope
    window.xThemeSongDialog = {
        show: showThemeSongDialog
    };
    
    console.log('xThemeSong: Dialog exported to window.xThemeSongDialog');
})();
