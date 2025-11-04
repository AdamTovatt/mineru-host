namespace MinerUHost
{
    public class SetupValidator : ISetupValidator
    {
        private const string VenvDirectoryName = "mineru-venv";
        private const string SetupMarkerFileName = ".mineru-setup-complete";

        public bool IsSetupComplete(string installPath)
        {
            string venvPath = Path.Combine(installPath, VenvDirectoryName);
            string markerPath = Path.Combine(installPath, SetupMarkerFileName);

            return Directory.Exists(venvPath) && File.Exists(markerPath);
        }
    }
}

