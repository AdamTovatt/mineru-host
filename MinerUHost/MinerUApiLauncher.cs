using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MinerUHost
{
    /// <summary>
    /// Provides functionality to launch and manage the MinerU API process.
    /// </summary>
    public class MinerUApiLauncher : IMinerUApiLauncher
    {
        private readonly ISetupValidator _setupValidator;
        private readonly IPythonSetupService _pythonSetupService;
        private readonly IProcessRunner _processRunner;
        private readonly IOutputCleaner _outputCleaner;
        private readonly ILogger<MinerUApiLauncher> _logger;
        private const string VenvDirectoryName = "mineru-venv";

        /// <summary>
        /// Initializes a new instance of the <see cref="MinerUApiLauncher"/> class.
        /// </summary>
        /// <param name="setupValidator">The validator to check if setup is complete.</param>
        /// <param name="pythonSetupService">The service to perform Python environment setup.</param>
        /// <param name="processRunner">The process runner to execute external processes.</param>
        /// <param name="outputCleaner">The cleaner to clean output directories.</param>
        /// <param name="logger">The logger for recording events.</param>
        public MinerUApiLauncher(
            ISetupValidator setupValidator,
            IPythonSetupService pythonSetupService,
            IProcessRunner processRunner,
            IOutputCleaner outputCleaner,
            ILogger<MinerUApiLauncher> logger)
        {
            _setupValidator = setupValidator;
            _pythonSetupService = pythonSetupService;
            _processRunner = processRunner;
            _outputCleaner = outputCleaner;
            _logger = logger;
        }

        /// <summary>
        /// Runs the MinerU API process asynchronously with the specified options.
        /// </summary>
        /// <param name="options">The command-line options for configuring the MinerU API.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RunAsync(CommandLineOptions options, CancellationToken cancellationToken)
        {
            _logger.LogInformation("MinerU Host starting...");
            _logger.LogInformation("Install Path: {InstallPath}", options.InstallPath);
            _logger.LogInformation("Host: {Host}, Port: {Port}", options.Host, options.Port);
            _logger.LogInformation("Cleanup Interval: {CleanupInterval} minutes", options.CleanupIntervalMinutes);

            // Ensure setup is complete
            if (!_setupValidator.IsSetupComplete(options.InstallPath))
            {
                _logger.LogInformation("Setup not complete. Running setup...");
                _pythonSetupService.PerformSetup(options.InstallPath);
            }
            else
            {
                _logger.LogInformation("Setup already complete. Skipping setup.");
            }

            // Start cleanup timer if interval is greater than 0
            Timer? cleanupTimer = null;
            if (options.CleanupIntervalMinutes > 0)
            {
                _logger.LogInformation("Starting cleanup timer with interval of {CleanupInterval} minutes", options.CleanupIntervalMinutes);
                cleanupTimer = new Timer(
                    callback: _ => _outputCleaner.CleanOutputDirectory(options.InstallPath),
                    state: null,
                    dueTime: TimeSpan.FromMinutes(options.CleanupIntervalMinutes),
                    period: TimeSpan.FromMinutes(options.CleanupIntervalMinutes));
            }
            else
            {
                _logger.LogInformation("Cleanup timer disabled (interval set to {CleanupInterval})", options.CleanupIntervalMinutes);
            }

            try
            {
                // Start mineru-api process
                string mineruExecutable = GetMinerUExecutablePath(options.InstallPath);
                string arguments = $"--host {options.Host} --port {options.Port}";

                _logger.LogInformation("Starting MinerU API...");
                using (Process mineruProcess = _processRunner.StartProcess(
                    fileName: mineruExecutable,
                    arguments: arguments,
                    workingDirectory: options.InstallPath))
                {
                    // Wait for process to exit or cancellation
                    try
                    {
                        await WaitForProcessOrCancellationAsync(mineruProcess, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Shutdown requested. Stopping MinerU API...");

                        if (!mineruProcess.HasExited)
                        {
                            mineruProcess.Kill(entireProcessTree: true);
                            mineruProcess.WaitForExit(5000);
                        }

                        throw;
                    }

                    if (mineruProcess.HasExited)
                    {
                        int exitCode = mineruProcess.ExitCode;
                        _logger.LogWarning("MinerU API process exited with code {ExitCode}", exitCode);
                        throw new InvalidOperationException($"MinerU API process exited unexpectedly with code {exitCode}");
                    }
                }
            }
            finally
            {
                cleanupTimer?.Dispose();
            }
        }

        private async Task WaitForProcessOrCancellationAsync(Process process, CancellationToken cancellationToken)
        {
            TaskCompletionSource<bool> processExitedTcs = new TaskCompletionSource<bool>();

            void ProcessExited(object? sender, EventArgs e)
            {
                processExitedTcs.TrySetResult(true);
            }

            process.Exited += ProcessExited;
            process.EnableRaisingEvents = true;

            try
            {
                // Check if process already exited
                if (process.HasExited)
                {
                    return;
                }

                // Wait for either process exit or cancellation
                Task completedTask = await Task.WhenAny(
                    processExitedTcs.Task,
                    Task.Delay(Timeout.Infinite, cancellationToken)
                );

                if (completedTask == processExitedTcs.Task)
                {
                    return;
                }
                else
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }
            finally
            {
                process.Exited -= ProcessExited;
            }
        }

        private string GetMinerUExecutablePath(string installPath)
        {
            string venvPath = Path.Combine(installPath, VenvDirectoryName);

            if (OperatingSystem.IsWindows())
            {
                return Path.Combine(venvPath, "Scripts", "mineru-api.exe");
            }
            else
            {
                return Path.Combine(venvPath, "bin", "mineru-api");
            }
        }
    }
}

