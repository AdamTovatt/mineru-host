# MinerUHost
[![Tests](https://github.com/AdamTovatt/mineru-sharp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/AdamTovatt/mineru-sharp/actions/workflows/dotnet.yml)
[![NuGet Version](https://img.shields.io/nuget/v/MinerUHost.svg)](https://www.nuget.org/packages/MinerUHost/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MinerUHost.svg)](https://www.nuget.org/packages/MinerUHost/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

A C# wrapper for [MinerU](https://github.com/opendatalab/MinerU) that automates setup and manages the process lifecycle. Available both as a standalone application and as a NuGet package that can be used from another .NET process.

Is best used together with the [`MinerUSharp`](https://github.com/AdamTovatt/mineru-sharp) library (available as a [NuGet package](https://www.nuget.org/packages/MinerUSharp)).

Easily installed with [`updaemon`](https://github.com/AdamTovatt/updaemon).

## What It Does

- Automatically creates a Python virtual environment
- Installs MinerU and its dependencies
- Launches and monitors the mineru-api service
- Periodically cleans up the output directory to prevent disk bloat (optional)
- Handles graceful shutdown

Creation of the virtual environment and installation only happens the first time, then it detects that it's already installed and just starts it directly.

## Possible future expansion

- Periodically restart the underlying python service that seems to have a memory leak

## Usage

You can use `MinerUHost` either as a [standalone application](#as-a-standalone-application) or as a [library (NuGet package)](#as-a-library-nuget-package).

## As a Standalone Application

```bash
mineru-host [options]
```

### Options

- `-h, --help` - Show help message
- `--host <host>` - Host to bind MinerU API (default: 0.0.0.0)
- `--port <port>` - Port to bind MinerU API (default: 8200)
- `--install-path <path>` - Installation directory (default: application directory)
- `--cleanup-interval <minutes>` - Output cleanup interval in minutes (default: 5, set to 0 or negative to disable)

### Example

```bash
mineru-host --host 127.0.0.1 --port 9000
```

## As a Library (NuGet Package)

Install the package:
```bash
dotnet add package MinerUHost
```

Use in your C# application:
```csharp
using MinerUHost;

MinerUProcessHost host = new MinerUProcessHost("127.0.0.1", 8200);
await host.RunAsync(cancellationToken);
```

### More Detailed Library Examples
Here are some more detailed examples of how the library can be used from code:

```csharp
using MinerUHost;

// With custom install path and a specified cleanup interval in minutes (default is 5)
MinerUProcessHost host = new MinerUProcessHost("127.0.0.1", 8200, "/path/to/install", 10);
await host.RunAsync(cancellationToken);
```

> [!TIP]
> To disable automatic cleanup you can set the cleanupIntervalMinutes-parameter to 0 (or a negative value if you prefer that for some reason)

You can also use an `CommandLineOptions` object if you want:
```csharp
CommandLineOptions options = new CommandLineOptions("127.0.0.1", 8200, "/path/to/install", 10);
MinerUProcessHost host = new MinerUProcessHost(options);
await host.RunAsync(cancellationToken);
```

If you want to use some custom logging you can send a `CommandLineOptions` object followed by an `ILoggerFactory`:
```csharp
// With custom logging (integrating with your existing ILoggerFactory)
MinerUProcessHost host = new MinerUProcessHost(options, yourLoggerFactory);
await host.RunAsync(cancellationToken);
```

### Example: Running in ASP.NET as a Background Service
Here is a longer code snippet showing how it could be used together with an ASP.NET application as a background service.

Create the background service:
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
            CommandLineOptions options = new CommandLineOptions("127.0.0.1", 8200);
            MinerUProcessHost host = new MinerUProcessHost(options, _loggerFactory);
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
```

Then register it at startup (probably in `Program.cs` if you haven't changed the name of that file)
```csharp
// Register at startup
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

