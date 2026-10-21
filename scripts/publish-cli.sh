#!/usr/bin/env bash

# Publishes the Fargo CLI as self-contained, single-file native executables for
# every supported platform and packages each one for distribution.
#
# Output (under artifacts/):
#   artifacts/release/<rid>/fargo[.exe]          the published binary
#   artifacts/dist/fargo-<version>-<rid>.<ext>   the distributable archive
#   artifacts/dist/fargo-<version>-<rid>.<ext>.sha256
#
# Windows builds are zipped; Linux builds are tar.gz'd. A SHA-256 checksum is
# written next to each archive so downloaders can verify integrity.
#
# Usage:
#   scripts/publish-cli.sh [version]
#
# If [version] is given (e.g. 0.1.0-alpha.1) it overrides the version baked into
# the binary; otherwise the <Version> from Fargo.Cli.csproj is used. The CI
# release workflow passes the git tag (without the leading "v").

set -euo pipefail

CLI_PROJECT="src/client/Fargo.Cli/Fargo.Cli.csproj"
RIDS=("win-x64" "linux-x64")

RELEASE_DIR="artifacts/release"
DIST_DIR="artifacts/dist"

VERSION="${1:-}"

# The version is passed to MSBuild as a single <Version> property. When no
# override is supplied, read the default back from the CLI project so archive
# names match the baked-in version.
if [[ -z "$VERSION" ]]; then
    VERSION="$(grep -oPm1 '(?<=<Version>)[^<]+' "$CLI_PROJECT" || true)"
fi

VERSION_ARGS=("-p:Version=$VERSION")

echo "Publishing Fargo CLI version: $VERSION"

rm -rf "$RELEASE_DIR" "$DIST_DIR"
mkdir -p "$DIST_DIR"

# Choose a SHA-256 tool that exists on the host (Linux: sha256sum; macOS: shasum).
sha256() {
    if command -v sha256sum >/dev/null 2>&1; then
        sha256sum "$1"
    else
        shasum -a 256 "$1"
    fi
}

for rid in "${RIDS[@]}"; do
    out="$RELEASE_DIR/$rid"
    echo
    echo "==> Publishing $rid"
    dotnet publish "$CLI_PROJECT" \
        -c Release \
        -r "$rid" \
        --self-contained true \
        -o "$out" \
        --nologo \
        "${VERSION_ARGS[@]}"

    # Stage the archive contents: the executable plus the license.
    staging="$(mktemp -d)"
    if [[ "$rid" == win-* ]]; then
        cp "$out/fargo.exe" "$staging/"
    else
        cp "$out/fargo" "$staging/"
        chmod +x "$staging/fargo"
    fi
    cp LICENSE "$staging/"

    archive_base="fargo-$VERSION-$rid"
    echo "==> Packaging $archive_base"
    if [[ "$rid" == win-* ]]; then
        archive="$DIST_DIR/$archive_base.zip"
        # Prefer zip; fall back to PowerShell on Windows build agents without zip.
        if command -v zip >/dev/null 2>&1; then
            (cd "$staging" && zip -q -r - .) > "$archive"
        else
            powershell -NoProfile -Command \
                "Compress-Archive -Path '$staging/*' -DestinationPath '$archive' -Force"
        fi
    else
        archive="$DIST_DIR/$archive_base.tar.gz"
        tar -czf "$archive" -C "$staging" .
    fi

    # Write a checksum file next to the archive (basename only inside the file).
    (cd "$DIST_DIR" && sha256 "$(basename "$archive")" > "$(basename "$archive").sha256")

    rm -rf "$staging"
    echo "    -> $archive"
done

echo
echo "Done. Distributable archives:"
ls -1 "$DIST_DIR"
