using Microsoft.Extensions.Logging;

namespace MinerUHost
{
    /// <summary>
    /// Provides functionality to clean output directories.
    /// </summary>
    public class OutputCleaner : IOutputCleaner
    {
        private readonly ILogger<OutputCleaner> _logger;
        private const string OutputDirectoryName = "output";

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputCleaner"/> class.
        /// </summary>
        /// <param name="logger">The logger for recording events.</param>
        public OutputCleaner(ILogger<OutputCleaner> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Cleans the output directory at the specified installation path.
        /// </summary>
        /// <param name="installPath">The installation path where the output directory is located.</param>
        public void CleanOutputDirectory(string installPath)
        {
            string outputPath = Path.Combine(installPath, OutputDirectoryName);

            if (!Directory.Exists(outputPath))
            {
                _logger.LogDebug("Output directory does not exist. Skipping cleanup.");
                return;
            }

            try
            {
                _logger.LogInformation("Cleaning up output directory: {OutputPath}", outputPath);

                DirectoryInfo directoryInfo = new DirectoryInfo(outputPath);

                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    try
                    {
                        file.Delete();
                        _logger.LogDebug("Deleted file: {FileName}", file.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete file: {FileName}", file.Name);
                    }
                }

                foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
                {
                    try
                    {
                        directory.Delete(recursive: true);
                        _logger.LogDebug("Deleted directory: {DirectoryName}", directory.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete directory: {DirectoryName}", directory.Name);
                    }
                }

                _logger.LogInformation("Output directory cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during output directory cleanup");
            }
        }
    }
}

