using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MinerUHost.Tests
{
    public class PythonSetupServiceTests : IDisposable
    {
        private readonly Mock<IProcessRunner> _processRunnerMock;
        private readonly Mock<ILogger<PythonSetupService>> _loggerMock;
        private readonly PythonSetupService _setupService;
        private readonly string _testDirectory;

        public PythonSetupServiceTests()
        {
            _processRunnerMock = new Mock<IProcessRunner>();
            _loggerMock = new Mock<ILogger<PythonSetupService>>();
            _setupService = new PythonSetupService(_processRunnerMock.Object, _loggerMock.Object);
            _testDirectory = Path.Combine(Path.GetTempPath(), $"mineru-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
        }

        [Fact]
        public void PerformSetup_WithSuccessfulCommands_CompletesSuccessfully()
        {
            // Arrange
            _processRunnerMock
                .Setup(x => x.RunProcess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(0);

            // Act
            _setupService.PerformSetup(_testDirectory);

            // Assert
            _processRunnerMock.Verify(x => x.RunProcess("python", "-m venv mineru-venv", _testDirectory), Times.Once);
            
            string markerPath = Path.Combine(_testDirectory, ".mineru-setup-complete");
            File.Exists(markerPath).Should().BeTrue();
        }

        [Fact]
        public void PerformSetup_WhenVenvCreationFails_ThrowsException()
        {
            // Arrange
            _processRunnerMock
                .Setup(x => x.RunProcess("python", "-m venv mineru-venv", _testDirectory))
                .Returns(1);

            // Act
            Action act = () => _setupService.PerformSetup(_testDirectory);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Failed to create virtual environment. Exit code: 1");
        }

        [Fact]
        public void PerformSetup_CallsAllSetupSteps_InCorrectOrder()
        {
            // Arrange
            List<string> callOrder = new List<string>();
            
            _processRunnerMock
                .Setup(x => x.RunProcess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string fileName, string args, string workingDir) =>
                {
                    callOrder.Add($"{fileName} {args}");
                    return 0;
                });

            // Act
            _setupService.PerformSetup(_testDirectory);

            // Assert
            callOrder.Should().HaveCount(4);
            callOrder[0].Should().StartWith("python -m venv");
            callOrder[1].Should().Contain("install --upgrade pip");
            callOrder[2].Should().Contain("install uv");
            callOrder[3].Should().Contain("pip install -U \"mineru[core]\"");
        }
    }
}

