$ErrorActionPreference = "Stop"

# Configuration
$projectPath = "$PSScriptRoot\Jellyfin.Plugin.AssignThemeSong.csproj"
$manifestPath = "$PSScriptRoot\manifest.json"
$metaPath = "$PSScriptRoot\meta.json"
$releaseDir = "$PSScriptRoot\publish"
$binDir = "$PSScriptRoot\bin"
$objDir = "$PSScriptRoot\obj"

# Get version from csproj
$xml = [xml](Get-Content $projectPath)
$version = $xml.Project.PropertyGroup.Version
Write-Host "Building version $version..." -ForegroundColor Cyan

# Clean
Write-Host "Cleaning..." -ForegroundColor Yellow
if (Test-Path $binDir) { Remove-Item $binDir -Recurse -Force }
if (Test-Path $objDir) { Remove-Item $objDir -Recurse -Force }
if (Test-Path $releaseDir) { Remove-Item $releaseDir -Recurse -Force }

# Build
Write-Host "Building..." -ForegroundColor Yellow
dotnet build $projectPath -c Release -o "$releaseDir\bin"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed!"
}

# Prepare zip content
$zipContentDir = "$releaseDir\zip_content"
New-Item -ItemType Directory -Path $zipContentDir | Out-Null

# Copy DLLs and resources
# We need to exclude some system assemblies that might conflict, but the csproj should handle most of it.
# Let's copy everything from the build output first.
Copy-Item "$releaseDir\bin\*" $zipContentDir -Recurse

# Remove unnecessary files from zip content if any remain (pdb, xml, etc if not needed)
Get-ChildItem $zipContentDir -Include *.pdb,*.xml,*.runtimeconfig.json -Recurse | Remove-Item -Force

# Create Zip
$zipFileName = "xThemeSong_v$version.zip"
$zipPath = "$releaseDir\$zipFileName"
Write-Host "Creating zip: $zipPath" -ForegroundColor Yellow

# Important: Jellyfin plugins need the dlls at the root of the zip or in a folder? 
# Usually they are just flat in the zip or in a folder named after the plugin.
# The error "End of Central Directory record could not be found" suggests a corrupted zip or empty zip.
# Let's use Compress-Archive but ensure we are zipping the *contents* of the folder, not the folder itself.
Compress-Archive -Path "$zipContentDir\*" -DestinationPath $zipPath -Force

# Calculate MD5
$md5 = (Get-FileHash $zipPath -Algorithm MD5).Hash.ToLower()
Write-Host "MD5: $md5" -ForegroundColor Green

# Update meta.json
Write-Host "Updating meta.json..." -ForegroundColor Yellow
$meta = Get-Content $metaPath | ConvertFrom-Json
$meta.version = $version
$meta | ConvertTo-Json -Depth 10 | Set-Content $metaPath

# Update manifest.json
Write-Host "Updating manifest.json..." -ForegroundColor Yellow
$manifest = Get-Content $manifestPath | ConvertFrom-Json
# Assuming the first package is ours
$package = $manifest[0]
$newVersionObj = [PSCustomObject]@{
    version = $version
    checksum = $md5
    changelog = $meta.changelog # Use changelog from meta.json or you can prompt for it
    targetAbi = $meta.targetAbi
    sourceUrl = "https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/releases/download/v$version/$zipFileName"
    timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
}

# Add new version to the beginning of the list
$package.versions = @($newVersionObj) + $package.versions
$manifest | ConvertTo-Json -Depth 10 | Set-Content $manifestPath

Write-Host "Done! Release ready in $releaseDir" -ForegroundColor Cyan
Write-Host "Don't forget to:"
Write-Host "1. Commit and push changes"
Write-Host "2. Create a tag v$version"
Write-Host "3. Upload $zipFileName to GitHub Releases"
