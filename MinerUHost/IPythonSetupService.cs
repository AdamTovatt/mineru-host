namespace MinerUHost
{
    /// <summary>
    /// Defines the interface for performing Python environment setup for MinerU.
    /// </summary>
    public interface IPythonSetupService
    {
        /// <summary>
        /// Performs the complete setup process including creating a virtual environment and installing dependencies.
        /// </summary>
        /// <param name="installPath">The installation path where the setup should be performed.</param>
        void PerformSetup(string installPath);
    }
}

