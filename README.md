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

1. Download `fargo-<version>-win-x64.zip` from the latest release.
2. (Optional) Verify the download against its checksum:
   ```powershell
   $expected = (Get-Content fargo-<version>-win-x64.zip.sha256).Split(' ')[0]
   (Get-FileHash fargo-<version>-win-x64.zip -Algorithm SHA256).Hash -ieq $expected
   ```
3. Extract the archive and move `fargo.exe` to a folder on your `PATH`
   (for example `%LOCALAPPDATA%\Programs\fargo`). To add that folder to your
   user `PATH`:
   ```powershell
   $dir = "$env:LOCALAPPDATA\Programs\fargo"
   setx PATH "$env:PATH;$dir"
   ```
   Open a new terminal afterwards so the updated `PATH` takes effect.

### Linux (x64)

1. Download `fargo-<version>-linux-x64.tar.gz` from the latest release.
2. (Optional) Verify the download against its checksum:
   ```bash
   sha256sum -c fargo-<version>-linux-x64.tar.gz.sha256
   ```
3. Extract and install it onto your `PATH`:
   ```bash
   tar -xzf fargo-<version>-linux-x64.tar.gz
   install -m 0755 fargo ~/.local/bin/fargo   # or: sudo install -m 0755 fargo /usr/local/bin/fargo
   ```
   Make sure `~/.local/bin` is on your `PATH`.

### First run

```bash
fargo --version
fargo config set-server --url https://localhost:7563
fargo identity login --username admin
fargo articles list
```

Run any command with `--help` to see its options, e.g. `fargo articles --help`.

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