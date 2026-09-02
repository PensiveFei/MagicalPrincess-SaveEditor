# Local packaging script: builds a self-contained single-file exe and zips it.
param(
    [string]$Version = "0.1.0"
)
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$proj = Join-Path $root "src\MagicalPrincess.SaveEditor\MagicalPrincess.SaveEditor.csproj"
$dist = Join-Path $root "dist"
$zip = Join-Path $root "MagicalPrincess-SaveEditor-v$Version-win-x64.zip"

dotnet publish $proj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o $dist

if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path (Join-Path $dist "MagicalPrincess.SaveEditor.exe") -DestinationPath $zip
Write-Host "Packaged: $zip"
