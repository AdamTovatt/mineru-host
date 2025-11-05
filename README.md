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

### As a Standalone Application

```bash
mineru-host [options]
```

### Options

- `--host <host>` - Host to bind MinerU API (default: 0.0.0.0)
- `--port <port>` - Port to bind MinerU API (default: 8200)
- `--install-path <path>` - Installation directory (default: application directory)
- `--cleanup-interval <minutes>` - Output cleanup interval in minutes (default: 5, set to 0 or negative to disable)

### Example

```bash
mineru-host --host 127.0.0.1 --port 9000
```

### As a Library (NuGet Package)

Install the package:
```bash
dotnet add package MinerUHost
```

Use in your C# application:
```csharp
using MinerUHost;

// Simple usage with default settings
var host = new MinerUProcessHost("127.0.0.1", 8200);
await host.RunAsync(cancellationToken);

// Or with custom install path
var host = new MinerUProcessHost("127.0.0.1", 8200, "/path/to/install");
await host.RunAsync(cancellationToken);

// Or with full configuration
var host = new MinerUProcessHost("127.0.0.1", 8200, "/path/to/install", cleanupIntervalMinutes: 10);
await host.RunAsync(cancellationToken);

// To disable cleanup, set interval to 0 or negative
var hostNoCleanup = new MinerUProcessHost("127.0.0.1", 8200, "/path/to/install", cleanupIntervalMinutes: 0);
await hostNoCleanup.RunAsync(cancellationToken);

// Or using CommandLineOptions
var options = new CommandLineOptions("127.0.0.1", 8200, "/path/to/install", 10);
var host = new MinerUProcessHost(options);
await host.RunAsync(cancellationToken);

// With custom logging (integrating with your existing ILoggerFactory)
var host = new MinerUProcessHost(options, yourLoggerFactory);
await host.RunAsync(cancellationToken);
```

#### Example: Running in ASP.NET Core as a Background Service

```csharp
public class MinerUBackgroundService : BackgroundService
{
    private readonly ILogger<MinerUBackgroundService> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public MinerUBackgroundService(ILogger<MinerUBackgroundService> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting MinerU host service");

        try
        {
            var options = new CommandLineOptions("127.0.0.1", 8200);
            var host = new MinerUProcessHost(options, _loggerFactory);
            await host.RunAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("MinerU host service stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MinerU host service failed");
        }
    }
}

// Register in Program.cs
builder.Services.AddHostedService<MinerUBackgroundService>();
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

