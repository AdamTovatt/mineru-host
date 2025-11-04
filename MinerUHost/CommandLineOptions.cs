namespace MinerUHost
{
    public class CommandLineOptions
    {
        public string Host { get; set; } = "0.0.0.0";
        public int Port { get; set; } = 8200;
        public string InstallPath { get; set; } = string.Empty;
        public int CleanupIntervalMinutes { get; set; } = 5;

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

