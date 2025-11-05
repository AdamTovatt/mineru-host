namespace MinerUHost
{
    /// <summary>
    /// The main entry point for the MinerU Host application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>The exit code of the application (0 for success, non-zero for failure).</returns>
        public static async Task<int> Main(string[] args)
        {
            // Check for help flag
            if (args.Any(arg => arg == "--help" || arg == "-h"))
            {
                PrintUsage();
                return 0;
            }

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

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            // Handle Ctrl+C gracefully
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            try
            {
                MinerUProcessHost host = new MinerUProcessHost(options);
                await host.RunAsync(cancellationTokenSource.Token);
                return 0;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Application stopped.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Application failed with error: {ex.Message}");
                return 1;
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: MinerUHost [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -h, --help                  Show this help message");
            Console.WriteLine("  --host <host>               Host to bind MinerU API (default: 0.0.0.0)");
            Console.WriteLine("  --port <port>               Port to bind MinerU API (default: 8200)");
            Console.WriteLine("  --install-path <path>       Path to install MinerU (default: application directory)");
            Console.WriteLine("  --cleanup-interval <mins>   Cleanup interval in minutes (default: 5, 0 or negative to disable)");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  MinerUHost --host 127.0.0.1 --port 9000 --install-path /opt/mineru");
            Console.WriteLine("  MinerUHost --cleanup-interval 0  # Disable cleanup");
        }
    }
}
