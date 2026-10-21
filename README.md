# Fargo

<img src="img/under_construction.png" width="400" alt="Under construction" />

## Status

Fargo is currently under active development and is not yet considered production-ready.
Features, APIs, database structures, and internal architecture may change without notice.

## Documentation

See the [Fargo Documentation](https://silvestrinigor.github.io/Fargo/) for the API reference and project documentation.

## Installing the CLI

The `fargo` command-line client ships as a self-contained native executable, so
no .NET runtime needs to be installed. Pre-built binaries for **Windows (x64)**
and **Linux (x64)** are attached to each
[GitHub Release](https://github.com/silvestrinigor/Fargo/releases).

> Fargo is still in early development, so releases are published as
> **pre-releases** (versions start at `0.x`). Expect breaking changes.

### Windows (x64)

Install (or upgrade) the latest release for the current user. This downloads
`fargo.exe`, installs it to `%LOCALAPPDATA%\Programs\fargo`, and adds it to your
user `PATH` — no admin rights required:

```powershell
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
irm https://github.com/silvestrinigor/Fargo/releases/latest/download/install.ps1 | iex
```

> The first line enables TLS 1.2, which Windows PowerShell 5.1 does not use by
> default — without it the download fails with "the connection was closed
> unexpectedly". It is harmless on PowerShell 7+.

Open a **new** terminal afterwards so the updated `PATH` takes effect, then run
`fargo --version`.

To install a specific version, or to uninstall, download the script and run it
with arguments:

```powershell
# Install a specific version
$s = "$env:TEMP\fargo-install.ps1"
irm https://github.com/silvestrinigor/Fargo/releases/latest/download/install.ps1 -OutFile $s
& $s -Version 0.1.0-alpha.1

# Uninstall (removes the binary and the PATH entry)
& $s -Uninstall
```

### Linux (x64)

Install (or upgrade) the latest release for the current user. This downloads the
`fargo` binary, verifies its checksum, and installs it to `~/.local/bin`:

```bash
curl -fsSL https://github.com/silvestrinigor/Fargo/releases/latest/download/install.sh | sh
```

Make sure `~/.local/bin` is on your `PATH` (the script warns you if it is not),
then run `fargo --version`.

Install a specific version, pick a different install directory, or uninstall:

```bash
# Install a specific version
curl -fsSL https://github.com/silvestrinigor/Fargo/releases/latest/download/install.sh | sh -s -- --version 0.1.0-alpha.1

# Install system-wide (needs sudo)
curl -fsSL https://github.com/silvestrinigor/Fargo/releases/latest/download/install.sh | sudo sh -s -- --prefix /usr/local/bin

# Uninstall
curl -fsSL https://github.com/silvestrinigor/Fargo/releases/latest/download/install.sh | sh -s -- --uninstall
```

### Manual install

Prefer to do it by hand? Download `fargo-<version>-win-x64.zip` (Windows) or
`fargo-<version>-linux-x64.tar.gz` (Linux) from the
[releases page](https://github.com/silvestrinigor/Fargo/releases), verify it
against its `.sha256` file, extract it, and place `fargo`/`fargo.exe` on a folder
in your `PATH`.

### First run

```bash
fargo --version
fargo config set-server --url https://localhost:7563
fargo identity login --username admin
fargo articles list
```

Run any command with `--help` to see its options, e.g. `fargo articles --help`.

Add `--verbose` (or `-v`) to any command to print detailed diagnostics when an
error occurs (exception types, inner exceptions, and stack traces), which is
useful when reporting issues:

```bash
fargo articles list --verbose
```

## Running the Application

```bash
dotnet run --project src/server/Fargo.AppHost
```

## Running Tests

```bash
dotnet test
```

## License

Licensed under the Apache License 2.0.

See [LICENSE](LICENSE) for details.