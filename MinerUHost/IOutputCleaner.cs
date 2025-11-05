namespace MinerUHost
{
    /// <summary>
    /// Defines the interface for cleaning output directories.
    /// </summary>
    public interface IOutputCleaner
    {
        /// <summary>
        /// Cleans the output directory at the specified installation path.
        /// </summary>
        /// <param name="installPath">The installation path where the output directory is located.</param>
        void CleanOutputDirectory(string installPath);
    }
}

