using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MinerUHost
{
    /// <summary>
    /// Provides functionality to run external processes.
    /// </summary>
    public class ProcessRunner : IProcessRunner
    {
        private readonly ILogger<ProcessRunner> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRunner"/> class.
        /// </summary>
        /// <param name="logger">The logger for recording events.</param>
        public ProcessRunner(ILogger<ProcessRunner> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Runs a process synchronously and waits for it to complete.
        /// </summary>
        /// <param name="fileName">The name or path of the executable file to run.</param>
        /// <param name="arguments">The command-line arguments to pass to the process.</param>
        /// <param name="workingDirectory">The working directory for the process.</param>
        /// <returns>The exit code of the process.</returns>
        public int RunProcess(string fileName, string arguments, string workingDirectory)
        {
            _logger.LogInformation("Running: {FileName} {Arguments}", fileName, arguments);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        _logger.LogInformation("{Output}", e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        _logger.LogError("{Error}", e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                return process.ExitCode;
            }
        }

        /// <summary>
        /// Starts a process asynchronously and returns the process object.
        /// </summary>
        /// <param name="fileName">The name or path of the executable file to run.</param>
        /// <param name="arguments">The command-line arguments to pass to the process.</param>
        /// <param name="workingDirectory">The working directory for the process.</param>
        /// <returns>The started <see cref="Process"/> instance.</returns>
        public Process StartProcess(string fileName, string arguments, string workingDirectory)
        {
            _logger.LogInformation("Starting: {FileName} {Arguments}", fileName, arguments);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            Process process = new Process { StartInfo = startInfo };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    _logger.LogInformation("[MinerU] {Output}", e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    _logger.LogError("[MinerU] {Error}", e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }
    }
}

