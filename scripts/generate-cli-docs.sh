#!/usr/bin/env bash

# Generates the CLI command reference for the docs site from the `fargo` CLI's
# own `--help` output, so the reference always matches the shipped commands.
#
# For each command group it writes docs/cli/reference/<group>.md containing the
# group's help and each subcommand's help, plus a reference/toc.yml.
#
# Run it before `docfx docfx.json`. The docs CI workflow runs it automatically.

set -e

CLI_PROJECT="src/client/Fargo.Cli/Fargo.Cli.csproj"
BUILD_DIR="$(pwd)/artifacts/cli-docs"
CLI_DLL="$BUILD_DIR/fargo.dll"
REFERENCE_DIR="docs/cli/reference"

# Command groups and their subcommands. Keep in sync with CommandFactory.
# Format: "group|Title|sub1 sub2 ..."
CLI_GROUPS=(
    "config|Config|set-server get-server show"
    "identity|Identity|login logout change-password"
    "article|Article|list tree get get-by-barcode inventory create update update-by-barcode delete delete-by-barcode"
    "item|Item|list get create update delete location moviments inventory"
    "partition|Partition|list tree get create update delete"
    "user-group|User Group|list tree get create update delete"
    "user|User|list get create update delete"
    "audit|Audit|list get"
)

echo "Building the Fargo CLI..."
dotnet build "$CLI_PROJECT" -c Release --nologo -o "$BUILD_DIR"

# Invoke the CLI and normalize localized System.CommandLine section headers to
# English so the generated docs are identical regardless of build-machine locale.
cli_help() {
    dotnet "$CLI_DLL" "$@" --help 2>/dev/null \
        | sed -e 's/^Uso:/Usage:/' \
              -e 's/^Opções:/Options:/' \
              -e 's/^Comandos:/Commands:/' \
              -e 's/^Argumentos:/Arguments:/' \
              -e 's/^Mostrar informações de versão/Show version information/'
}

mkdir -p "$REFERENCE_DIR"

TOC_FILE="$REFERENCE_DIR/toc.yml"
{
    echo "- name: Overview"
    echo "  href: index.md"
} > "$TOC_FILE"

for entry in "${CLI_GROUPS[@]}"; do
    IFS='|' read -r group title subs <<< "$entry"

    page="$REFERENCE_DIR/$group.md"
    echo "Generating $page"

    {
        echo "# $title"
        echo
        echo "Command reference for \`fargo $group\`, generated from the CLI's help output."
        echo
        echo "## \`fargo $group\`"
        echo
        echo '```text'
        cli_help "$group"
        echo '```'
        echo

        for sub in $subs; do
            echo "## \`fargo $group $sub\`"
            echo
            echo '```text'
            cli_help "$group" "$sub"
            echo '```'
            echo
        done
    } > "$page"

    echo "- name: $title" >> "$TOC_FILE"
    echo "  href: $group.md" >> "$TOC_FILE"
done

echo "Done. CLI reference written to $REFERENCE_DIR."
