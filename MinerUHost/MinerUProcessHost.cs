using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MinerUHost
{
    /// <summary>
    /// A facade class for hosting and managing the MinerU Python process.
    /// Can be used as a library in other C# applications or as a standalone service.
    /// </summary>
    public class MinerUProcessHost
    {
        private readonly CommandLineOptions _options;
        private readonly ILoggerFactory? _loggerFactory;

        /// <summary>
        /// Creates a new instance of MinerUProcessHost with the specified options.
        /// </summary>
        /// <param name="options">Configuration options for the MinerU host.</param>
        public MinerUProcessHost(CommandLineOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Creates a new instance of MinerUProcessHost with the specified options and logger factory.
        /// </summary>
        /// <param name="options">Configuration options for the MinerU host.</param>
        /// <param name="loggerFactory">Optional logger factory for custom logging integration.</param>
        public MinerUProcessHost(CommandLineOptions options, ILoggerFactory loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Creates a new instance of MinerUProcessHost with minimal configuration.
        /// </summary>
        /// <param name="host">Host to bind MinerU API.</param>
        /// <param name="port">Port to bind MinerU API.</param>
        public MinerUProcessHost(string host, int port)
        {
            _options = new CommandLineOptions(host, port);
        }

        /// <summary>
        /// Creates a new instance of MinerUProcessHost with custom install path.
        /// </summary>
        /// <param name="host">Host to bind MinerU API.</param>
        /// <param name="port">Port to bind MinerU API.</param>
        /// <param name="installPath">Path to install MinerU.</param>
        public MinerUProcessHost(string host, int port, string installPath)
        {
            _options = new CommandLineOptions(host, port, installPath);
        }

        /// <summary>
        /// Creates a new instance of MinerUProcessHost with full configuration.
        /// </summary>
        /// <param name="host">Host to bind MinerU API.</param>
        /// <param name="port">Port to bind MinerU API.</param>
        /// <param name="installPath">Path to install MinerU.</param>
        /// <param name="cleanupIntervalMinutes">Cleanup interval in minutes.</param>
        public MinerUProcessHost(string host, int port, string installPath, int cleanupIntervalMinutes)
        {
            _options = new CommandLineOptions(host, port, installPath, cleanupIntervalMinutes);
        }

        /// <summary>
        /// Runs the MinerU host process. This method will block until the cancellation token is triggered
        /// or the process exits unexpectedly.
        /// </summary>
        /// <param name="cancellationToken">Token to signal graceful shutdown.</param>
        /// <returns>A task representing the running host process.</returns>
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);

            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                IMinerUApiLauncher launcher = serviceProvider.GetRequiredService<IMinerUApiLauncher>();
                await launcher.RunAsync(_options, cancellationToken);
            }
        }

        private void ConfigureServices(ServiceCollection services)
        {
            if (_loggerFactory != null)
            {
                services.AddSingleton<ILoggerFactory>(_loggerFactory);
                services.AddLogging();
            }
            else
            {
                services.AddLogging(configure =>
                {
                    configure.AddConsole();
                    configure.SetMinimumLevel(LogLevel.Information);
                });
            }

            services.AddSingleton<IProcessRunner, ProcessRunner>();
            services.AddSingleton<ISetupValidator, SetupValidator>();
            services.AddSingleton<IPythonSetupService, PythonSetupService>();
            services.AddSingleton<IOutputCleaner, OutputCleaner>();
            services.AddSingleton<IMinerUApiLauncher, MinerUApiLauncher>();
        }
    }
}

