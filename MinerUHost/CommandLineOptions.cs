namespace MinerUHost
{
    /// <summary>
    /// Represents command-line options for configuring the MinerU host application.
    /// </summary>
    public class CommandLineOptions
    {
        /// <summary>
        /// Gets or sets the host address to bind the MinerU API server.
        /// </summary>
        public string Host { get; set; } = "0.0.0.0";

        /// <summary>
        /// Gets or sets the port number to bind the MinerU API server.
        /// </summary>
        public int Port { get; set; } = 8200;

        /// <summary>
        /// Gets or sets the installation path for MinerU.
        /// </summary>
        public string InstallPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the cleanup interval in minutes for the output directory.
        /// </summary>
        public int CleanupIntervalMinutes { get; set; } = 5;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineOptions"/> class with default values.
        /// </summary>
        public CommandLineOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineOptions"/> class with the specified host and port.
        /// </summary>
        /// <param name="host">The host address to bind the MinerU API server.</param>
        /// <param name="port">The port number to bind the MinerU API server.</param>
        public CommandLineOptions(string host, int port)
        {
            Host = host;
            Port = port;
            InstallPath = AppContext.BaseDirectory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineOptions"/> class with the specified host, port, and install path.
        /// </summary>
        /// <param name="host">The host address to bind the MinerU API server.</param>
        /// <param name="port">The port number to bind the MinerU API server.</param>
        /// <param name="installPath">The installation path for MinerU.</param>
        public CommandLineOptions(string host, int port, string installPath)
        {
            Host = host;
            Port = port;
            InstallPath = installPath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineOptions"/> class with the specified host, port, install path, and cleanup interval.
        /// </summary>
        /// <param name="host">The host address to bind the MinerU API server.</param>
        /// <param name="port">The port number to bind the MinerU API server.</param>
        /// <param name="installPath">The installation path for MinerU.</param>
        /// <param name="cleanupIntervalMinutes">The cleanup interval in minutes for the output directory.</param>
        public CommandLineOptions(string host, int port, string installPath, int cleanupIntervalMinutes)
        {
            Host = host;
            Port = port;
            InstallPath = installPath;
            CleanupIntervalMinutes = cleanupIntervalMinutes;
        }

        /// <summary>
        /// Parses command-line arguments and returns a <see cref="CommandLineOptions"/> instance.
        /// </summary>
        /// <param name="args">The command-line arguments to parse.</param>
        /// <returns>A <see cref="CommandLineOptions"/> instance with parsed values.</returns>
        public static CommandLineOptions Parse(string[] args)
        {
            CommandLineOptions options = new CommandLineOptions();
            
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                
                if (arg == "--host" && i + 1 < args.Length)
                {
                    options.Host = args[i + 1];
                    i++;
                }
                else if (arg == "--port" && i + 1 < args.Length)
                {
                    if (int.TryParse(args[i + 1], out int port))
                        options.Port = port;
                    else
                        throw new ArgumentException($"Invalid port value: {args[i + 1]}");
                    i++;
                }
                else if (arg == "--install-path" && i + 1 < args.Length)
                {
                    options.InstallPath = args[i + 1];
                    i++;
                }
                else if (arg == "--cleanup-interval" && i + 1 < args.Length)
                {
                    if (int.TryParse(args[i + 1], out int interval))
                        options.CleanupIntervalMinutes = interval;
                    else
                        throw new ArgumentException($"Invalid cleanup interval value: {args[i + 1]}");
                    i++;
                }
            }

            // Set default install path to application directory if not specified
            if (string.IsNullOrEmpty(options.InstallPath))
                options.InstallPath = AppContext.BaseDirectory;

            return options;
        }
    }
}

