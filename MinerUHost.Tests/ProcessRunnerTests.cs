using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MinerUHost.Tests
{
    public class ProcessRunnerTests
    {
        private readonly Mock<ILogger<ProcessRunner>> _loggerMock;
        private readonly ProcessRunner _processRunner;

        public ProcessRunnerTests()
        {
            _loggerMock = new Mock<ILogger<ProcessRunner>>();
            _processRunner = new ProcessRunner(_loggerMock.Object);
        }

        [Fact]
        public void RunProcess_WithValidCommand_ReturnsExitCode()
        {
            // Arrange
            string fileName = OperatingSystem.IsWindows() ? "cmd.exe" : "echo";
            string arguments = OperatingSystem.IsWindows() ? "/c echo test" : "test";
            string workingDirectory = AppContext.BaseDirectory;

            // Act
            int exitCode = _processRunner.RunProcess(fileName, arguments, workingDirectory);

            // Assert
            exitCode.Should().Be(0);
        }

        [Fact]
        public void StartProcess_WithValidCommand_ReturnsRunningProcess()
        {
            // Arrange
            string fileName = OperatingSystem.IsWindows() ? "cmd.exe" : "sleep";
            string arguments = OperatingSystem.IsWindows() ? "/c timeout /t 1 /nobreak" : "1";
            string workingDirectory = AppContext.BaseDirectory;

            // Act
            using (System.Diagnostics.Process process = _processRunner.StartProcess(fileName, arguments, workingDirectory))
            {
                // Assert
                process.Should().NotBeNull();
                process.HasExited.Should().BeFalse();

                // Cleanup
                process.WaitForExit(2000);
            }
        }
    }
}

