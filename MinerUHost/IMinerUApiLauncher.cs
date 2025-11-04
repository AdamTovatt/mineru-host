namespace MinerUHost
{
    public interface IMinerUApiLauncher
    {
        Task RunAsync(CommandLineOptions options, CancellationToken cancellationToken);
    }
}

