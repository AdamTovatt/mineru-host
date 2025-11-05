using System.Diagnostics;

namespace MinerUHost
{
    /// <summary>
    /// Defines the interface for running external processes.
    /// </summary>
    public interface IProcessRunner
    {
        /// <summary>
        /// Runs a process synchronously and waits for it to complete.
        /// </summary>
        /// <param name="fileName">The name or path of the executable file to run.</param>
        /// <param name="arguments">The command-line arguments to pass to the process.</param>
        /// <param name="workingDirectory">The working directory for the process.</param>
        /// <returns>The exit code of the process.</returns>
        int RunProcess(string fileName, string arguments, string workingDirectory);

        /// <summary>
        /// Starts a process asynchronously and returns the process object.
        /// </summary>
        /// <param name="fileName">The name or path of the executable file to run.</param>
        /// <param name="arguments">The command-line arguments to pass to the process.</param>
        /// <param name="workingDirectory">The working directory for the process.</param>
        /// <returns>The started <see cref="Process"/> instance.</returns>
        Process StartProcess(string fileName, string arguments, string workingDirectory);
    }
}

