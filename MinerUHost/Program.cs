using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MinerUHost
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            CommandLineOptions options;
            try
            {
                options = CommandLineOptions.Parse(args);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error parsing arguments: {ex.Message}");
                PrintUsage();
                return 1;
            }

            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);

            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                IMinerUApiLauncher launcher = serviceProvider.GetRequiredService<IMinerUApiLauncher>();
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                // Handle Ctrl+C gracefully
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    cancellationTokenSource.Cancel();
                };

                try
                {
                    await launcher.RunAsync(options, cancellationTokenSource.Token);
                    return 0;
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Application stopped.");
                    return 0;
                }
                catch (Exception ex)
                {
                    ILogger<Program> logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Application failed with error: {Message}", ex.Message);
                    return 1;
                }
            }
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddLogging(configure =>
            {
                configure.AddConsole();
                configure.SetMinimumLevel(LogLevel.Information);
            });

            services.AddSingleton<IProcessRunner, ProcessRunner>();
            services.AddSingleton<ISetupValidator, SetupValidator>();
            services.AddSingleton<IPythonSetupService, PythonSetupService>();
            services.AddSingleton<IOutputCleaner, OutputCleaner>();
            services.AddSingleton<IMinerUApiLauncher, MinerUApiLauncher>();
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: MinerUHost [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --host <host>               Host to bind MinerU API (default: 0.0.0.0)");
            Console.WriteLine("  --port <port>               Port to bind MinerU API (default: 8200)");
            Console.WriteLine("  --install-path <path>       Path to install MinerU (default: application directory)");
            Console.WriteLine("  --cleanup-interval <mins>   Cleanup interval in minutes (default: 5)");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  MinerUHost --host 127.0.0.1 --port 9000 --install-path /opt/mineru");
        }
    }
}
