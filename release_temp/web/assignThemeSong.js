(function() {
    'use strict';
    
    console.log('Assign Theme Song: Dialog module loaded');
    
    function showThemeSongDialog(itemId) {
        require(['globalize', 'loading', 'dialogHelper', 'formDialogStyle', 'emby-input', 'emby-button'], function(globalize, loading, dialogHelper) {
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
            html += '<h3 class="formDialogHeaderTitle">Assign Theme Song</h3>';
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

                loading.show();

                var formData = new FormData();
                
                if (youtubeUrl) {
                    formData.append('YouTubeUrl', youtubeUrl);
                }
                if (selectedFile) {
                    formData.append('UploadedFile', selectedFile);
                }

                // Use the correct API endpoint
                var apiUrl = ApiClient.getUrl('AssignThemeSong/' + itemId);

                fetch(apiUrl, {
                    method: 'POST',
                    headers: {
                        'X-Emby-Token': ApiClient.accessToken()
                    },
                    body: formData
                }).then(function (response) {
                    loading.hide();
                    if (response.ok) {
                        dialogHelper.close(dlg);
                        require(['toast'], function (toast) {
                            toast('Theme song assigned successfully!');
                        });
                    } else {
                        return response.text().then(function (text) {
                            throw new Error(text || 'Failed to assign theme song');
                        });
                    }
                }).catch(function (error) {
                    loading.hide();
                    alert('Error: ' + (error.message || 'Failed to assign theme song'));
                });
            });

            // Cancel button
            dlg.querySelector('.btnCancel').addEventListener('click', function () {
                dialogHelper.close(dlg);
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
    window.AssignThemeSongDialog = {
        show: showThemeSongDialog
    };
    
    console.log('Assign Theme Song: Dialog exported to window.AssignThemeSongDialog');
})();
