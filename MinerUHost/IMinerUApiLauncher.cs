namespace MinerUHost
{
    /// <summary>
    /// Defines the interface for launching and managing the MinerU API process.
    /// </summary>
    public interface IMinerUApiLauncher
    {
        /// <summary>
        /// Runs the MinerU API process asynchronously with the specified options.
        /// </summary>
        /// <param name="options">The command-line options for configuring the MinerU API.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RunAsync(CommandLineOptions options, CancellationToken cancellationToken);
    }
}

