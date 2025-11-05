namespace MinerUHost
{
    /// <summary>
    /// Defines the interface for validating whether the MinerU setup is complete.
    /// </summary>
    public interface ISetupValidator
    {
        /// <summary>
        /// Determines whether the MinerU setup is complete at the specified installation path.
        /// </summary>
        /// <param name="installPath">The installation path to check.</param>
        /// <returns><c>true</c> if the setup is complete; otherwise, <c>false</c>.</returns>
        bool IsSetupComplete(string installPath);
    }
}

