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
    irm https://raw.githubusercontent.com/silvestrinigor/Fargo/master/scripts/install.ps1 | iex

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

# Windows PowerShell 5.1 defaults to TLS 1.0/1.1, which GitHub rejects (the
# connection is closed during the handshake). Ensure TLS 1.2 is enabled before
# any web request. Harmless on PowerShell 7+, which already uses modern TLS.
try {
    [Net.ServicePointManager]::SecurityProtocol =
        [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
} catch {
    # Older runtimes may not expose Tls12 by name; ignore and proceed.
}

$Repo       = 'silvestrinigor/Fargo'
$Rid        = 'win-x64'
$InstallDir = Join-Path $env:LOCALAPPDATA 'Programs\fargo'

# The user PATH lives in HKCU\Environment as a REG_EXPAND_SZ value that often
# contains unexpanded entries such as %USERPROFILE%\bin. [Environment]::
# SetEnvironmentVariable would expand them and store a plain REG_SZ, silently
# rewriting the user's other PATH entries. Read the raw value and write it back
# with its expandable type instead.
function Get-RawUserPath {
    $key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Environment')
    try {
        if ($null -eq $key) { return '' }
        $value = $key.GetValue('Path', '', [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)
        if ($null -eq $value) { return '' }
        return [string]$value
    } finally {
        if ($key) { $key.Dispose() }
    }
}

function Set-RawUserPath([string]$value) {
    $key = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey('Environment')
    try {
        $key.SetValue('Path', $value, [Microsoft.Win32.RegistryValueKind]::ExpandString)
    } finally {
        $key.Dispose()
    }
    # Tell running programs (Explorer, new terminals) the environment changed.
    # Setting and clearing a throwaway variable broadcasts WM_SETTINGCHANGE.
    $name = "FARGO_PATH_REFRESH_$PID"
    [Environment]::SetEnvironmentVariable($name, '1', 'User')
    [Environment]::SetEnvironmentVariable($name, $null, 'User')
}

function Test-SamePath([string]$a, [string]$b) {
    $a = [Environment]::ExpandEnvironmentVariables($a).TrimEnd('\')
    $b = [Environment]::ExpandEnvironmentVariables($b).TrimEnd('\')
    return $a -ieq $b
}

function Remove-FromUserPath([string]$dir) {
    $userPath = Get-RawUserPath
    if ([string]::IsNullOrEmpty($userPath)) { return }

    $entries = @($userPath -split ';' | Where-Object { $_ -and -not (Test-SamePath $_ $dir) })
    Set-RawUserPath ($entries -join ';')
}

function Add-ToUserPath([string]$dir) {
    $userPath = Get-RawUserPath
    $present  = @($userPath -split ';' | Where-Object { $_ -and (Test-SamePath $_ $dir) })
    if ($present.Count -eq 0) {
        $newPath = if ([string]::IsNullOrEmpty($userPath)) { $dir } else { "$($userPath.TrimEnd(';'));$dir" }
        Set-RawUserPath $newPath
    }
    # Make it usable in the current session too.
    if (@($env:PATH -split ';' | Where-Object { $_ -and (Test-SamePath $_ $dir) }).Count -eq 0) {
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

    # Verify the checksum when the .sha256 asset is available. A missing asset
    # only skips verification; a mismatch always aborts the install.
    $shaPath = "$zipPath.sha256"
    $haveSha = $true
    try {
        Invoke-WebRequest -Uri $sha256Url -OutFile $shaPath -UseBasicParsing
    } catch {
        $haveSha = $false
        Write-Warning "No checksum file found; skipping verification."
    }
    if ($haveSha) {
        $expected = (Get-Content $shaPath -Raw).Trim().Split(' ')[0]
        $actual   = (Get-FileHash $zipPath -Algorithm SHA256).Hash
        if ($actual -ine $expected) {
            throw "Checksum verification failed for $asset (expected $expected, got $actual)."
        }
        Write-Host "Checksum verified."
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
