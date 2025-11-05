using Microsoft.Extensions.Logging;

namespace MinerUHost
{
    /// <summary>
    /// Provides functionality to perform Python environment setup for MinerU.
    /// </summary>
    public class PythonSetupService : IPythonSetupService
    {
        private readonly IProcessRunner _processRunner;
        private readonly ILogger<PythonSetupService> _logger;
        private const string VenvDirectoryName = "mineru-venv";
        private const string SetupMarkerFileName = ".mineru-setup-complete";

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonSetupService"/> class.
        /// </summary>
        /// <param name="processRunner">The process runner to execute external processes.</param>
        /// <param name="logger">The logger for recording events.</param>
        public PythonSetupService(IProcessRunner processRunner, ILogger<PythonSetupService> logger)
        {
            _processRunner = processRunner;
            _logger = logger;
        }

        /// <summary>
        /// Performs the complete setup process including creating a virtual environment and installing dependencies.
        /// </summary>
        /// <param name="installPath">The installation path where the setup should be performed.</param>
        public void PerformSetup(string installPath)
        {
            _logger.LogInformation("Starting MinerU setup in {InstallPath}", installPath);

            CreateVirtualEnvironment(installPath);
            UpgradePip(installPath);
            InstallUv(installPath);
            InstallMinerU(installPath);
            CreateSetupMarker(installPath);

            _logger.LogInformation("MinerU setup completed successfully");
        }

        private void CreateVirtualEnvironment(string installPath)
        {
            _logger.LogInformation("Creating virtual environment...");
            
            int exitCode = _processRunner.RunProcess(
                fileName: "python",
                arguments: $"-m venv {VenvDirectoryName}",
                workingDirectory: installPath
            );

            if (exitCode != 0)
                throw new InvalidOperationException($"Failed to create virtual environment. Exit code: {exitCode}");
        }

        private void UpgradePip(string installPath)
        {
            _logger.LogInformation("Upgrading pip...");
            
            string pythonExecutable = GetVenvExecutablePath(installPath, "python");
            
            int exitCode = _processRunner.RunProcess(
                fileName: pythonExecutable,
                arguments: "-m pip install --upgrade pip",
                workingDirectory: installPath
            );

            if (exitCode != 0)
                throw new InvalidOperationException($"Failed to upgrade pip. Exit code: {exitCode}");
        }

        private void InstallUv(string installPath)
        {
            _logger.LogInformation("Installing uv...");
            
            string pipExecutable = GetVenvExecutablePath(installPath, "pip");
            
            int exitCode = _processRunner.RunProcess(
                fileName: pipExecutable,
                arguments: "install uv",
                workingDirectory: installPath
            );

            if (exitCode != 0)
                throw new InvalidOperationException($"Failed to install uv. Exit code: {exitCode}");
        }

        private void InstallMinerU(string installPath)
        {
            _logger.LogInformation("Installing MinerU...");
            
            string uvExecutable = GetVenvExecutablePath(installPath, "uv");
            string pythonExecutable = GetVenvExecutablePath(installPath, "python");
            
            int exitCode = _processRunner.RunProcess(
                fileName: uvExecutable,
                arguments: $"pip install -U \"mineru[core]\" --python \"{pythonExecutable}\"",
                workingDirectory: installPath
            );

            if (exitCode != 0)
                throw new InvalidOperationException($"Failed to install MinerU. Exit code: {exitCode}");
        }

        private void CreateSetupMarker(string installPath)
        {
            string markerPath = Path.Combine(installPath, SetupMarkerFileName);
            File.WriteAllText(markerPath, DateTime.UtcNow.ToString("O"));
            _logger.LogInformation("Created setup marker file at {MarkerPath}", markerPath);
        }

        private string GetVenvExecutablePath(string installPath, string executableName)
        {
            string venvPath = Path.Combine(installPath, VenvDirectoryName);
            
            if (OperatingSystem.IsWindows())
            {
                return Path.Combine(venvPath, "Scripts", $"{executableName}.exe");
            }
            else
            {
                return Path.Combine(venvPath, "bin", executableName);
            }
        }
    }
}

