using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace MinerUHost.Tests
{
    public class MinerUApiLauncherTests : IDisposable
    {
        private readonly Mock<ISetupValidator> _setupValidatorMock;
        private readonly Mock<IPythonSetupService> _pythonSetupServiceMock;
        private readonly Mock<IProcessRunner> _processRunnerMock;
        private readonly Mock<ILogger<MinerUApiLauncher>> _loggerMock;
        private readonly MinerUApiLauncher _launcher;
        private readonly string _testDirectory;

        public MinerUApiLauncherTests()
        {
            _setupValidatorMock = new Mock<ISetupValidator>();
            _pythonSetupServiceMock = new Mock<IPythonSetupService>();
            _processRunnerMock = new Mock<IProcessRunner>();
            _loggerMock = new Mock<ILogger<MinerUApiLauncher>>();
            _launcher = new MinerUApiLauncher(
                _setupValidatorMock.Object,
                _pythonSetupServiceMock.Object,
                _processRunnerMock.Object,
                _loggerMock.Object
            );
            _testDirectory = Path.Combine(Path.GetTempPath(), $"mineru-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
        }

        [Fact]
        public async Task RunAsync_WhenSetupNotComplete_RunsSetup()
        {
            // Arrange
            CommandLineOptions options = new CommandLineOptions { InstallPath = _testDirectory };
            CancellationTokenSource cts = new CancellationTokenSource();
            
            _setupValidatorMock.Setup(x => x.IsSetupComplete(_testDirectory)).Returns(false);
            
            Process mockProcess = CreateMockProcess();
            _processRunnerMock
                .Setup(x => x.StartProcess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockProcess);

            // Cancel after a short delay to allow the test to complete
            cts.CancelAfter(100);

            // Act
            Func<Task> act = async () => await _launcher.RunAsync(options, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
            _pythonSetupServiceMock.Verify(x => x.PerformSetup(_testDirectory), Times.Once);
            
            mockProcess.Dispose();
        }

        [Fact]
        public async Task RunAsync_WhenSetupComplete_SkipsSetup()
        {
            // Arrange
            CommandLineOptions options = new CommandLineOptions { InstallPath = _testDirectory };
            CancellationTokenSource cts = new CancellationTokenSource();
            
            _setupValidatorMock.Setup(x => x.IsSetupComplete(_testDirectory)).Returns(true);
            
            Process mockProcess = CreateMockProcess();
            _processRunnerMock
                .Setup(x => x.StartProcess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockProcess);

            cts.CancelAfter(100);

            // Act
            Func<Task> act = async () => await _launcher.RunAsync(options, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
            _pythonSetupServiceMock.Verify(x => x.PerformSetup(It.IsAny<string>()), Times.Never);
            
            mockProcess.Dispose();
        }

        [Fact]
        public async Task RunAsync_StartsProcessWithCorrectArguments()
        {
            // Arrange
            CommandLineOptions options = new CommandLineOptions 
            { 
                InstallPath = _testDirectory,
                Host = "127.0.0.1",
                Port = 9000,
            };
            CancellationTokenSource cts = new CancellationTokenSource();
            
            _setupValidatorMock.Setup(x => x.IsSetupComplete(_testDirectory)).Returns(true);
            
            Process mockProcess = CreateMockProcess();
            _processRunnerMock
                .Setup(x => x.StartProcess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockProcess);

            cts.CancelAfter(100);

            // Act
            Func<Task> act = async () => await _launcher.RunAsync(options, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
            _processRunnerMock.Verify(
                x => x.StartProcess(
                    It.IsAny<string>(),
                    "--host 127.0.0.1 --port 9000",
                    _testDirectory
                ),
                Times.Once
            );
            
            mockProcess.Dispose();
        }

        private Process CreateMockProcess()
        {
            // Create a long-running process for testing
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "powershell.exe" : "sleep",
                Arguments = OperatingSystem.IsWindows() ? "-Command \"Start-Sleep -Seconds 3600\"" : "3600",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();
            
            // Give the process a moment to fully start
            Thread.Sleep(50);
            
            return process;
        }
    }
}

