# MinerUHost
[![Tests](https://github.com/AdamTovatt/mineru-sharp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/AdamTovatt/mineru-sharp/actions/workflows/dotnet.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

A C# wrapper for [MinerU](https://github.com/opendatalab/MinerU) that automates setup and manages the process lifecycle.

Is best used together with the [`MinerUSharp`](https://github.com/AdamTovatt/mineru-sharp) library (available as a [NuGet package](https://www.nuget.org/packages/MinerUSharp)).

Easily installed with [`updaemon`](https://github.com/AdamTovatt/updaemon).

## What It Does

- Automatically creates a Python virtual environment
- Installs MinerU and its dependencies
- Launches and monitors the mineru-api service
- Periodically cleans up the output directory to prevent disk bloat
- Handles graceful shutdown

## Possible future expansion

- Periodically restart the underlying python service that seems to have a memory leak

## Usage

```bash
mineru-host [options]
```

### Options

- `--host <host>` - Host to bind MinerU API (default: 0.0.0.0)
- `--port <port>` - Port to bind MinerU API (default: 8200)
- `--install-path <path>` - Installation directory (default: application directory)
- `--cleanup-interval <minutes>` - Output cleanup interval (default: 5)

### Example

```bash
mineru-host --host 127.0.0.1 --port 9000
```

## Requirements

- .NET 8.0
- Python 3.x available in PATH

## First Run

On first run, the application will:
1. Create a virtual environment
2. Install pip, uv, and MinerU
3. Start the MinerU API service

Subsequent runs will skip setup and start the service immediately.

