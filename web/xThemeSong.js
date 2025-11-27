(function() {
    'use strict';
    
    console.log('xThemeSong: Dialog module loaded');
    
    // CSS for dialogs and loading
    var styleElement = document.createElement('style');
    styleElement.textContent = `
        .xthemesong-overlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.85);
            z-index: 10000;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        .xthemesong-dialog {
            background: #1e1e1e;
            padding: 24px;
            border-radius: 12px;
            max-width: 500px;
            width: 90%;
            max-height: 85vh;
            overflow-y: auto;
            box-shadow: 0 8px 32px rgba(0,0,0,0.5);
        }
        .xthemesong-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
            padding-bottom: 12px;
            border-bottom: 1px solid #333;
        }
        .xthemesong-title {
            margin: 0;
            color: #fff;
            font-size: 1.4em;
        }
        .xthemesong-close {
            background: none;
            border: none;
            color: #888;
            font-size: 24px;
            cursor: pointer;
            padding: 4px 8px;
            border-radius: 4px;
        }
        .xthemesong-close:hover {
            background: #333;
            color: #fff;
        }
        .xthemesong-section {
            margin-bottom: 20px;
        }
        .xthemesong-label {
            color: #aaa;
            display: block;
            margin-bottom: 8px;
            font-size: 0.9em;
        }
        .xthemesong-input {
            width: 100%;
            padding: 12px;
            border-radius: 6px;
            border: 1px solid #444;
            background: #2a2a2a;
            color: #fff;
            font-size: 14px;
            box-sizing: border-box;
        }
        .xthemesong-input:focus {
            outline: none;
            border-color: #00a4dc;
        }
        .xthemesong-dropzone {
            border: 2px dashed #444;
            border-radius: 8px;
            padding: 30px;
            text-align: center;
            cursor: pointer;
            transition: all 0.3s;
        }
        .xthemesong-dropzone:hover, .xthemesong-dropzone.active {
            border-color: #00a4dc;
            background: rgba(0, 164, 220, 0.1);
        }
        .xthemesong-dropzone-icon {
            font-size: 48px;
            color: #555;
            margin-bottom: 12px;
        }
        .xthemesong-dropzone-text {
            color: #888;
            margin: 8px 0;
        }
        .xthemesong-file-info {
            color: #00a4dc;
            margin-top: 12px;
            font-size: 0.9em;
        }
        .xthemesong-btn {
            padding: 12px 24px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-size: 14px;
            font-weight: 500;
            transition: all 0.2s;
        }
        .xthemesong-btn-primary {
            background: #00a4dc;
            color: #fff;
        }
        .xthemesong-btn-primary:hover {
            background: #0090c4;
        }
        .xthemesong-btn-primary:disabled {
            background: #555;
            cursor: not-allowed;
        }
        .xthemesong-btn-secondary {
            background: #444;
            color: #fff;
        }
        .xthemesong-btn-secondary:hover {
            background: #555;
        }
        .xthemesong-footer {
            display: flex;
            gap: 12px;
            justify-content: flex-end;
            margin-top: 24px;
            padding-top: 16px;
            border-top: 1px solid #333;
        }
        .xthemesong-player {
            width: 100%;
            margin-top: 12px;
            border-radius: 6px;
        }
        .xthemesong-loading {
            display: flex;
            flex-direction: column;
            align-items: center;
            padding: 40px;
        }
        .xthemesong-spinner {
            width: 50px;
            height: 50px;
            border: 4px solid #333;
            border-top-color: #00a4dc;
            border-radius: 50%;
            animation: xthemesong-spin 1s linear infinite;
        }
        @keyframes xthemesong-spin {
            to { transform: rotate(360deg); }
        }
        .xthemesong-loading-text {
            color: #aaa;
            margin-top: 16px;
            font-size: 14px;
        }
        .xthemesong-message {
            text-align: center;
            padding: 20px;
        }
        .xthemesong-message-icon {
            font-size: 48px;
            margin-bottom: 16px;
        }
        .xthemesong-message-icon.success { color: #4caf50; }
        .xthemesong-message-icon.error { color: #f44336; }
        .xthemesong-message-text {
            color: #fff;
            font-size: 16px;
            margin-bottom: 8px;
        }
        .xthemesong-message-detail {
            color: #888;
            font-size: 14px;
        }
        .xthemesong-existing {
            background: #252525;
            border-radius: 8px;
            padding: 16px;
        }
        .xthemesong-existing-title {
            color: #fff;
            margin: 0 0 8px 0;
            font-size: 1em;
        }
        .xthemesong-existing-meta {
            color: #888;
            font-size: 0.85em;
            margin-top: 8px;
        }
    `;
    document.head.appendChild(styleElement);
    
    function showThemeSongDialog(itemId) {
        console.log('xThemeSong: Opening dialog for item', itemId);
        closeActionSheets();
        createDialog(itemId);
    }
    
    function closeActionSheets() {
        var actionSheets = document.querySelectorAll('.actionSheet, .dialogContainer, .dialog-container');
        actionSheets.forEach(function(el) {
            el.style.display = 'none';
            if (el.parentNode) el.parentNode.removeChild(el);
        });
        var backdrops = document.querySelectorAll('.backdrop, .backdropFadeIn');
        backdrops.forEach(function(el) {
            if (el.parentNode) el.parentNode.removeChild(el);
        });
    }
    
    function createDialog(itemId) {
        var overlay = document.createElement('div');
        overlay.className = 'xthemesong-overlay';
        
        var dialog = document.createElement('div');
        dialog.className = 'xthemesong-dialog';
        dialog.innerHTML = getDialogHTML();
        
        overlay.appendChild(dialog);
        document.body.appendChild(overlay);
        
        // Load existing theme
        loadExistingTheme(itemId, dialog);
        
        // Setup event handlers
        setupDialogEvents(overlay, dialog, itemId);
    }
    
    function getDialogHTML() {
        return `
            <div class="xthemesong-header">
                <h2 class="xthemesong-title">🎵 xThemeSong</h2>
                <button class="xthemesong-close" id="xthemesongClose">&times;</button>
            </div>
            
            <div id="xthemesongContent">
                <div id="xthemesongExisting" class="xthemesong-section" style="display:none;">
                    <div class="xthemesong-existing">
                        <h4 class="xthemesong-existing-title">🎧 Current Theme Song</h4>
                        <audio id="xthemesongPlayer" class="xthemesong-player" controls></audio>
                        <div id="xthemesongMeta" class="xthemesong-existing-meta"></div>
                    </div>
                </div>
                
                <div class="xthemesong-section">
                    <label class="xthemesong-label">YouTube URL or Video ID</label>
                    <input type="text" id="xthemesongYouTube" class="xthemesong-input" 
                           placeholder="https://www.youtube.com/watch?v=... or video ID">
                </div>
                
                <div class="xthemesong-section">
                    <label class="xthemesong-label">Or Upload MP3 File</label>
                    <div id="xthemesongDropzone" class="xthemesong-dropzone">
                        <div class="xthemesong-dropzone-icon">📁</div>
                        <div class="xthemesong-dropzone-text">Drag & drop an MP3 file here</div>
                        <div class="xthemesong-dropzone-text" style="font-size:0.9em;">or click to browse</div>
                        <input type="file" id="xthemesongFile" accept=".mp3,audio/mpeg" style="display:none;">
                    </div>
                    <div id="xthemesongFileInfo" class="xthemesong-file-info" style="display:none;"></div>
                </div>
                
                <div class="xthemesong-footer">
                    <button id="xthemesongCancel" class="xthemesong-btn xthemesong-btn-secondary">Cancel</button>
                    <button id="xthemesongSubmit" class="xthemesong-btn xthemesong-btn-primary">Save Theme Song</button>
                </div>
            </div>
        `;
    }
    
    function showLoading(dialog, message) {
        var content = dialog.querySelector('#xthemesongContent');
        content.innerHTML = `
            <div class="xthemesong-loading">
                <div class="xthemesong-spinner"></div>
                <div class="xthemesong-loading-text">${message || 'Processing...'}</div>
            </div>
        `;
    }
    
    function showMessage(dialog, type, title, detail, overlay) {
        var icon = type === 'success' ? '✅' : '❌';
        var content = dialog.querySelector('#xthemesongContent');
        content.innerHTML = `
            <div class="xthemesong-message">
                <div class="xthemesong-message-icon ${type}">${icon}</div>
                <div class="xthemesong-message-text">${title}</div>
                <div class="xthemesong-message-detail">${detail || ''}</div>
            </div>
            <div class="xthemesong-footer">
                <button id="xthemesongOk" class="xthemesong-btn xthemesong-btn-primary">OK</button>
            </div>
        `;
        
        dialog.querySelector('#xthemesongOk').addEventListener('click', function() {
            document.body.removeChild(overlay);
        });
    }
    
    function setupDialogEvents(overlay, dialog, itemId) {
        var selectedFile = null;
        
        // Close button
        dialog.querySelector('#xthemesongClose').addEventListener('click', function() {
            document.body.removeChild(overlay);
        });
        
        // Cancel button
        dialog.querySelector('#xthemesongCancel').addEventListener('click', function() {
            document.body.removeChild(overlay);
        });
        
        // Click outside to close
        overlay.addEventListener('click', function(e) {
            if (e.target === overlay) {
                document.body.removeChild(overlay);
            }
        });
        
        // Dropzone events
        var dropzone = dialog.querySelector('#xthemesongDropzone');
        var fileInput = dialog.querySelector('#xthemesongFile');
        var fileInfo = dialog.querySelector('#xthemesongFileInfo');
        
        dropzone.addEventListener('click', function() {
            fileInput.click();
        });
        
        ['dragenter', 'dragover'].forEach(function(e) {
            dropzone.addEventListener(e, function(ev) {
                ev.preventDefault();
                dropzone.classList.add('active');
            });
        });
        
        ['dragleave', 'drop'].forEach(function(e) {
            dropzone.addEventListener(e, function(ev) {
                ev.preventDefault();
                dropzone.classList.remove('active');
            });
        });
        
        dropzone.addEventListener('drop', function(e) {
            e.preventDefault();
            var files = e.dataTransfer.files;
            if (files.length > 0 && (files[0].type === 'audio/mpeg' || files[0].name.endsWith('.mp3'))) {
                selectedFile = files[0];
                showSelectedFile(selectedFile, fileInfo);
            }
        });
        
        fileInput.addEventListener('change', function() {
            if (this.files.length > 0) {
                selectedFile = this.files[0];
                showSelectedFile(selectedFile, fileInfo);
            }
        });
        
        // Submit button
        dialog.querySelector('#xthemesongSubmit').addEventListener('click', function() {
            var youtubeUrl = dialog.querySelector('#xthemesongYouTube').value.trim();
            
            if (!youtubeUrl && !selectedFile) {
                showMessage(dialog, 'error', 'Missing Input', 'Please enter a YouTube URL or upload an MP3 file.', overlay);
                return;
            }
            
            // Show loading
            showLoading(dialog, youtubeUrl ? 'Downloading from YouTube...' : 'Uploading file...');
            
            // Prepare form data
            var formData = new FormData();
            if (youtubeUrl) formData.append('YouTubeUrl', youtubeUrl);
            if (selectedFile) formData.append('UploadedFile', selectedFile);
            
            // API call
            var apiUrl = ApiClient.getUrl('xThemeSong/' + itemId);
            
            fetch(apiUrl, {
                method: 'POST',
                headers: { 'X-Emby-Token': ApiClient.accessToken() },
                body: formData
            }).then(function(response) {
                if (response.ok) {
                    showMessage(dialog, 'success', 'Theme Song Saved!', 'The theme song has been successfully assigned.', overlay);
                } else {
                    return response.text().then(function(text) {
                        throw new Error(text || 'Failed to assign theme song');
                    });
                }
            }).catch(function(error) {
                showMessage(dialog, 'error', 'Error', error.message || 'Failed to assign theme song', overlay);
            });
        });
    }
    
    function showSelectedFile(file, fileInfoElement) {
        var sizeInMB = (file.size / 1024 / 1024).toFixed(2);
        fileInfoElement.textContent = '📎 Selected: ' + file.name + ' (' + sizeInMB + ' MB)';
        fileInfoElement.style.display = 'block';
    }
    
    function loadExistingTheme(itemId, dialog) {
        // Try to load existing theme.json from media folder
        var themeJsonUrl = ApiClient.getUrl('xThemeSong/' + itemId + '/metadata');
        
        fetch(themeJsonUrl, {
            headers: { 'X-Emby-Token': ApiClient.accessToken() }
        }).then(function(response) {
            if (response.ok) return response.json();
            throw new Error('No theme');
        }).then(function(metadata) {
            if (metadata) {
                var existingSection = dialog.querySelector('#xthemesongExisting');
                var player = dialog.querySelector('#xthemesongPlayer');
                var metaDiv = dialog.querySelector('#xthemesongMeta');
                
                // Get audio URL
                var audioUrl = ApiClient.getUrl('xThemeSong/' + itemId + '/audio');
                player.src = audioUrl + '?api_key=' + ApiClient.accessToken();
                
                // Show metadata
                var metaText = [];
                if (metadata.Title) metaText.push('Title: ' + metadata.Title);
                if (metadata.Uploader) metaText.push('By: ' + metadata.Uploader);
                if (metadata.DateAdded) metaText.push('Added: ' + new Date(metadata.DateAdded).toLocaleDateString());
                metaDiv.textContent = metaText.join(' • ');
                
                existingSection.style.display = 'block';
            }
        }).catch(function() {
            // No existing theme, that's fine
        });
    }
    
    // Export to global scope
    window.xThemeSongDialog = {
        show: showThemeSongDialog
    };
    
    console.log('xThemeSong: Dialog exported to window.xThemeSongDialog');
})();
