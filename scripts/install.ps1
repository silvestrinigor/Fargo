<#
.SYNOPSIS
    Installs or uninstalls the Fargo CLI (`fargo`) for the current user on Windows.

.DESCRIPTION
    Downloads a self-contained `fargo.exe` from the project's GitHub Releases,
    verifies its SHA-256 checksum, installs it to %LOCALAPPDATA%\Programs\fargo,
    and adds that folder to the user PATH (no admin required). Re-running the
    script upgrades an existing install.

    Use -Uninstall to remove the binary and the PATH entry.

.PARAMETER Version
    The release version to install (e.g. 0.1.0-alpha.1). Defaults to the latest
    release (including pre-releases).

.PARAMETER Uninstall
    Removes the installed binary and the PATH entry instead of installing.

.EXAMPLE
    irm https://github.com/silvestrinigor/Fargo/releases/latest/download/install.ps1 | iex

.EXAMPLE
    .\install.ps1 -Version 0.1.0-alpha.1

.EXAMPLE
    .\install.ps1 -Uninstall
#>
[CmdletBinding()]
param(
    [string]$Version,
    [switch]$Uninstall
)

$ErrorActionPreference = 'Stop'

$Repo       = 'silvestrinigor/Fargo'
$Rid        = 'win-x64'
$InstallDir = Join-Path $env:LOCALAPPDATA 'Programs\fargo'

function Remove-FromUserPath([string]$dir) {
    $userPath = [Environment]::GetEnvironmentVariable('PATH', 'User')
    if ([string]::IsNullOrEmpty($userPath)) { return }

    $entries = $userPath -split ';' | Where-Object {
        $_ -and ($_.TrimEnd('\') -ine $dir.TrimEnd('\'))
    }
    [Environment]::SetEnvironmentVariable('PATH', ($entries -join ';'), 'User')
}

function Add-ToUserPath([string]$dir) {
    $userPath = [Environment]::GetEnvironmentVariable('PATH', 'User')
    $present  = ($userPath -split ';') | Where-Object {
        $_.TrimEnd('\') -ieq $dir.TrimEnd('\')
    }
    if (-not $present) {
        $newPath = if ([string]::IsNullOrEmpty($userPath)) { $dir } else { "$userPath;$dir" }
        [Environment]::SetEnvironmentVariable('PATH', $newPath, 'User')
    }
    # Make it usable in the current session too.
    if (($env:PATH -split ';' | Where-Object { $_.TrimEnd('\') -ieq $dir.TrimEnd('\') }).Count -eq 0) {
        $env:PATH = "$env:PATH;$dir"
    }
}

if ($Uninstall) {
    Write-Host "Uninstalling Fargo CLI..."
    if (Test-Path $InstallDir) {
        Remove-Item -Recurse -Force $InstallDir
        Write-Host "Removed $InstallDir"
    } else {
        Write-Host "Nothing to remove at $InstallDir"
    }
    Remove-FromUserPath $InstallDir
    Write-Host "Removed $InstallDir from your user PATH."
    Write-Host "Done. Open a new terminal for the PATH change to take effect."
    return
}

# Resolve the version to install. GitHub's /releases/latest skips pre-releases,
# so query the list and take the newest entry (which may be a pre-release).
if (-not $Version) {
    Write-Host "Resolving latest release..."
    $headers  = @{ 'User-Agent' = 'fargo-installer'; 'Accept' = 'application/vnd.github+json' }
    $releases = Invoke-RestMethod -Uri "https://api.github.com/repos/$Repo/releases" -Headers $headers
    $latest   = $releases | Where-Object { -not $_.draft } | Select-Object -First 1
    if (-not $latest) { throw "No releases found for $Repo." }
    $Version = $latest.tag_name -replace '^v', ''
}

$asset     = "fargo-$Version-$Rid.zip"
$baseUrl   = "https://github.com/$Repo/releases/download/v$Version"
$zipUrl    = "$baseUrl/$asset"
$sha256Url = "$baseUrl/$asset.sha256"

$tmp = Join-Path ([System.IO.Path]::GetTempPath()) ("fargo-install-" + [guid]::NewGuid())
New-Item -ItemType Directory -Path $tmp | Out-Null

try {
    Write-Host "Downloading $asset (version $Version)..."
    $zipPath = Join-Path $tmp $asset
    Invoke-WebRequest -Uri $zipUrl -OutFile $zipPath -UseBasicParsing

    # Verify the checksum when the .sha256 asset is available.
    try {
        $shaPath = "$zipPath.sha256"
        Invoke-WebRequest -Uri $sha256Url -OutFile $shaPath -UseBasicParsing
        $expected = (Get-Content $shaPath -Raw).Trim().Split(' ')[0]
        $actual   = (Get-FileHash $zipPath -Algorithm SHA256).Hash
        if ($actual -ine $expected) {
            throw "Checksum verification failed for $asset (expected $expected, got $actual)."
        }
        Write-Host "Checksum verified."
    } catch [System.Net.WebException] {
        Write-Warning "No checksum file found; skipping verification."
    }

    Write-Host "Installing to $InstallDir..."
    New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
    Expand-Archive -Path $zipPath -DestinationPath $tmp -Force
    Copy-Item -Path (Join-Path $tmp 'fargo.exe') -Destination (Join-Path $InstallDir 'fargo.exe') -Force

    Add-ToUserPath $InstallDir

    Write-Host ""
    Write-Host "Fargo CLI $Version installed to $InstallDir."
    Write-Host "Open a new terminal, then run: fargo --version"
} finally {
    Remove-Item -Recurse -Force $tmp -ErrorAction SilentlyContinue
}
