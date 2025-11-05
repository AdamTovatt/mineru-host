using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace MinerUHost.Tests
{
    public class TestOutputLogger<T> : ILogger<T>
    {
        private readonly ITestOutputHelper _output;

        public TestOutputLogger(ITestOutputHelper output)
        {
            _output = output;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            string message = formatter(state, exception);
            _output.WriteLine($"[{logLevel}] {message}");

            if (exception != null)
            {
                _output.WriteLine($"Exception: {exception}");
            }
        }
    }
}

