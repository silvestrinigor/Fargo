#!/usr/bin/env sh

# Installs or uninstalls the Fargo CLI (`fargo`) for the current user on Linux.
#
# Downloads a self-contained `fargo` binary from the project's GitHub Releases,
# verifies its SHA-256 checksum, and installs it to ~/.local/bin (override with
# --prefix <dir>). Re-running upgrades an existing install.
#
# Usage:
#   install.sh [--version <version>] [--prefix <dir>]
#   install.sh --uninstall [--prefix <dir>]
#
# One-line install (latest release, including pre-releases):
#   curl -fsSL https://github.com/silvestrinigor/Fargo/releases/latest/download/install.sh | sh
#
# Install a specific version:
#   curl -fsSL .../install.sh | sh -s -- --version 0.1.0-alpha.1
#
# Uninstall:
#   curl -fsSL .../install.sh | sh -s -- --uninstall

set -eu

REPO="silvestrinigor/Fargo"
RID="linux-x64"
PREFIX="${HOME}/.local/bin"
VERSION=""
UNINSTALL=0

while [ $# -gt 0 ]; do
    case "$1" in
        --version) VERSION="$2"; shift 2 ;;
        --prefix)  PREFIX="$2";  shift 2 ;;
        --uninstall) UNINSTALL=1; shift ;;
        *) echo "Unknown argument: $1" >&2; exit 2 ;;
    esac
done

BIN="$PREFIX/fargo"

if [ "$UNINSTALL" -eq 1 ]; then
    echo "Uninstalling Fargo CLI..."
    if [ -f "$BIN" ]; then
        rm -f "$BIN"
        echo "Removed $BIN"
    else
        echo "Nothing to remove at $BIN"
    fi
    exit 0
fi

# A download helper that works with either curl or wget.
download() {
    url="$1"; out="$2"
    if command -v curl >/dev/null 2>&1; then
        curl -fsSL "$url" -o "$out"
    elif command -v wget >/dev/null 2>&1; then
        wget -qO "$out" "$url"
    else
        echo "Error: need curl or wget to download." >&2
        exit 1
    fi
}

# Resolve the version. GitHub's /releases/latest skips pre-releases, so read the
# releases list and take the first (newest) tag, which may be a pre-release.
if [ -z "$VERSION" ]; then
    echo "Resolving latest release..."
    api="$(mktemp)"
    download "https://api.github.com/repos/$REPO/releases" "$api"
    # First "tag_name" in the list = newest release.
    tag="$(grep -m1 '"tag_name"' "$api" | sed -E 's/.*"tag_name"[[:space:]]*:[[:space:]]*"([^"]+)".*/\1/')"
    rm -f "$api"
    if [ -z "$tag" ]; then
        echo "Error: could not determine the latest release." >&2
        exit 1
    fi
    VERSION="${tag#v}"
fi

ASSET="fargo-$VERSION-$RID.tar.gz"
BASE_URL="https://github.com/$REPO/releases/download/v$VERSION"

TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

echo "Downloading $ASSET (version $VERSION)..."
download "$BASE_URL/$ASSET" "$TMP/$ASSET"

# Verify the checksum when the .sha256 asset is available and a tool exists.
if download "$BASE_URL/$ASSET.sha256" "$TMP/$ASSET.sha256" 2>/dev/null; then
    if command -v sha256sum >/dev/null 2>&1; then
        expected="$(cut -d' ' -f1 < "$TMP/$ASSET.sha256")"
        actual="$(sha256sum "$TMP/$ASSET" | cut -d' ' -f1)"
        if [ "$expected" != "$actual" ]; then
            echo "Error: checksum verification failed for $ASSET." >&2
            exit 1
        fi
        echo "Checksum verified."
    else
        echo "Note: sha256sum not found; skipping verification."
    fi
else
    echo "Note: no checksum file found; skipping verification."
fi

echo "Installing to $PREFIX..."
mkdir -p "$PREFIX"
tar -xzf "$TMP/$ASSET" -C "$TMP"
install -m 0755 "$TMP/fargo" "$BIN"

echo ""
echo "Fargo CLI $VERSION installed to $BIN."

# Remind the user if the install dir is not on PATH.
case ":$PATH:" in
    *":$PREFIX:"*) : ;;
    *)
        echo ""
        echo "NOTE: $PREFIX is not on your PATH. Add this to your shell profile:"
        echo "  export PATH=\"$PREFIX:\$PATH\""
        ;;
esac

echo "Run: fargo --version"
