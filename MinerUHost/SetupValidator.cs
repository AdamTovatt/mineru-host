namespace MinerUHost
{
    /// <summary>
    /// Provides functionality to validate whether the MinerU setup is complete.
    /// </summary>
    public class SetupValidator : ISetupValidator
    {
        private const string VenvDirectoryName = "mineru-venv";
        private const string SetupMarkerFileName = ".mineru-setup-complete";

        /// <summary>
        /// Determines whether the MinerU setup is complete at the specified installation path.
        /// </summary>
        /// <param name="installPath">The installation path to check.</param>
        /// <returns><c>true</c> if the setup is complete; otherwise, <c>false</c>.</returns>
        public bool IsSetupComplete(string installPath)
        {
            string venvPath = Path.Combine(installPath, VenvDirectoryName);
            string markerPath = Path.Combine(installPath, SetupMarkerFileName);

            return Directory.Exists(venvPath) && File.Exists(markerPath);
        }
    }
}

