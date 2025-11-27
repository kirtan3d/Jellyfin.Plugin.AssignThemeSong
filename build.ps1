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
    exit 1
}

# Prepare zip content
$zipContentDir = "$releaseDir\zip_content"
New-Item -ItemType Directory -Path $zipContentDir | Out-Null

# Copy DLLs and resources
Copy-Item "$releaseDir\bin\*" $zipContentDir -Recurse

# Remove unnecessary files from zip content
Get-ChildItem $zipContentDir -Include *.pdb,*.xml,*.runtimeconfig.json,*.deps.json -Recurse | Remove-Item -Force

# Remove Jellyfin host assemblies that cause version conflicts
$jellyfinAssemblies = @(
    "Jellyfin.Data.dll",
    "Jellyfin.Extensions.dll", 
    "MediaBrowser.Common.dll",
    "MediaBrowser.Controller.dll",
    "MediaBrowser.Model.dll",
    "Emby.Naming.dll",
    "Microsoft.Extensions.*.dll",
    "Newtonsoft.Json.dll",
    "ICU4N*.dll",
    "J2N.dll",
    "Diacritics.dll"
)

foreach ($pattern in $jellyfinAssemblies) {
    Get-ChildItem $zipContentDir -Include $pattern -Recurse | Remove-Item -Force -ErrorAction SilentlyContinue
}

Write-Host "Cleaned up Jellyfin host assemblies from zip" -ForegroundColor Yellow

# Create Zip
$zipFileName = "xThemeSong_v$version.zip"
$zipPath = "$releaseDir\$zipFileName"
Write-Host "Creating zip: $zipPath" -ForegroundColor Yellow

Compress-Archive -Path "$zipContentDir\*" -DestinationPath $zipPath -Force

# Calculate MD5
$md5 = (Get-FileHash $zipPath -Algorithm MD5).Hash.ToLower()
Write-Host "MD5: $md5" -ForegroundColor Green

# Get metadata for changelog
$meta = Get-Content $metaPath | ConvertFrom-Json

# Update manifest.json - it's an array of plugins
Write-Host "Updating manifest.json..." -ForegroundColor Yellow

# Read manifest content as string and parse manually to preserve array
$manifestContent = Get-Content $manifestPath -Raw
$manifestArray = $manifestContent | ConvertFrom-Json

$newVersionObj = [PSCustomObject]@{
    version = $version
    checksum = $md5
    changelog = $meta.changelog
    targetAbi = $meta.targetAbi
    sourceUrl = "https://github.com/kirtan3d/Jellyfin.Plugin.AssignThemeSong/releases/download/v$version/$zipFileName"
    timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
}

# Access the first plugin in the array and update its versions
$manifestArray[0].versions = @($newVersionObj) + $manifestArray[0].versions

# Write back as proper JSON array - using @() to ensure array output
$jsonOutput = @($manifestArray) | ConvertTo-Json -Depth 10
$jsonOutput | Set-Content $manifestPath -Encoding UTF8

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "Build Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host "Version: $version" -ForegroundColor Cyan
Write-Host "MD5: $md5" -ForegroundColor Cyan
Write-Host "Zip: $zipPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. git add -A && git commit -m 'Release v$version'"
Write-Host "2. git push"
Write-Host "3. gh release create v$version '$zipPath' --title 'v$version' --notes-file RELEASE_NOTES.md"
