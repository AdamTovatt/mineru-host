using System.Diagnostics;

namespace MinerUHost
{
    public interface IProcessRunner
    {
        int RunProcess(string fileName, string arguments, string workingDirectory);
        Process StartProcess(string fileName, string arguments, string workingDirectory);
    }
}

