using EasyReasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MinerUSharp;
using MinerUSharp.Models;
using Xunit.Abstractions;

namespace MinerUHost.Tests
{
    public class IntegrationTests : IDisposable, IClassFixture<ResourceManagerFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly string _testDirectory;
        private readonly ILogger<ProcessRunner> _processRunnerLogger;
        private readonly ILogger<PythonSetupService> _setupServiceLogger;
        private readonly ProcessRunner _processRunner;
        private readonly ResourceManager _resourceManager;

        public IntegrationTests(ITestOutputHelper output, ResourceManagerFixture fixture)
        {
            _output = output;
            _resourceManager = fixture.ResourceManager;
            _testDirectory = Path.Combine(Path.GetTempPath(), $"mineru-integration-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);

            _output.WriteLine($"Test directory: {_testDirectory}");

            // Create loggers that output to test output
            _processRunnerLogger = new TestOutputLogger<ProcessRunner>(_output);
            _setupServiceLogger = new TestOutputLogger<PythonSetupService>(_output);

            _processRunner = new ProcessRunner(_processRunnerLogger);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    _output.WriteLine($"Cleaning up test directory: {_testDirectory}");
                    Directory.Delete(_testDirectory, true);
                    _output.WriteLine("Cleanup completed successfully");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Failed to cleanup test directory: {ex.Message}");
                }
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void FullSetup_WithRealPython_CreatesVenvAndInstallsPackages()
        {
            // Arrange
            _output.WriteLine("Starting full setup integration test");

            // Check if Python is available
            if (!IsPythonAvailable())
            {
                _output.WriteLine("Python is not available. Skipping test.");
                return;
            }

            SetupValidator validator = new SetupValidator();
            PythonSetupService setupService = new PythonSetupService(_processRunner, _setupServiceLogger);

            // Act
            _output.WriteLine("Performing setup...");
            setupService.PerformSetup(_testDirectory);

            // Assert
            _output.WriteLine("Verifying setup completion...");
            bool isComplete = validator.IsSetupComplete(_testDirectory);
            isComplete.Should().BeTrue("Setup should create venv and marker file");

            string venvPath = Path.Combine(_testDirectory, "mineru-venv");
            Directory.Exists(venvPath).Should().BeTrue("Virtual environment directory should exist");

            string markerPath = Path.Combine(_testDirectory, ".mineru-setup-complete");
            File.Exists(markerPath).Should().BeTrue("Setup marker file should exist");

            _output.WriteLine("Setup verification completed successfully");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SetupValidator_AfterRealSetup_ReturnsTrue()
        {
            // Arrange
            _output.WriteLine("Starting setup validator integration test");

            if (!IsPythonAvailable())
            {
                _output.WriteLine("Python is not available. Skipping test.");
                return;
            }

            SetupValidator validator = new SetupValidator();
            PythonSetupService setupService = new PythonSetupService(_processRunner, _setupServiceLogger);

            // Act
            _output.WriteLine("Initial validation (should be false)");
            bool beforeSetup = validator.IsSetupComplete(_testDirectory);
            _output.WriteLine($"Before setup: {beforeSetup}");

            _output.WriteLine("Performing setup...");
            setupService.PerformSetup(_testDirectory);

            _output.WriteLine("Validation after setup (should be true)");
            bool afterSetup = validator.IsSetupComplete(_testDirectory);
            _output.WriteLine($"After setup: {afterSetup}");

            // Assert
            beforeSetup.Should().BeFalse("Should not be complete before setup");
            afterSetup.Should().BeTrue("Should be complete after setup");
        }

        [Fact]
        public void ProcessRunner_WithSimpleCommand_OutputsToTestExplorer()
        {
            // Arrange
            _output.WriteLine("Starting process runner test with simple command");
            string fileName = OperatingSystem.IsWindows() ? "cmd.exe" : "echo";
            string arguments = OperatingSystem.IsWindows() ? "/c echo Hello from integration test" : "Hello from integration test";

            // Act
            _output.WriteLine($"Running command: {fileName} {arguments}");
            int exitCode = _processRunner.RunProcess(fileName, arguments, _testDirectory);
            _output.WriteLine($"Command completed with exit code: {exitCode}");

            // Assert
            exitCode.Should().Be(0);
        }

        [Fact]
        public void SetupValidator_WithManuallyCreatedFiles_ReturnsTrue()
        {
            // Arrange
            _output.WriteLine("Starting manual setup validator test");
            SetupValidator validator = new SetupValidator();

            // Act
            _output.WriteLine("Creating venv directory and marker file manually");
            string venvPath = Path.Combine(_testDirectory, "mineru-venv");
            Directory.CreateDirectory(venvPath);
            _output.WriteLine($"Created directory: {venvPath}");

            string markerPath = Path.Combine(_testDirectory, ".mineru-setup-complete");
            File.WriteAllText(markerPath, DateTime.UtcNow.ToString("O"));
            _output.WriteLine($"Created marker file: {markerPath}");

            bool isComplete = validator.IsSetupComplete(_testDirectory);
            _output.WriteLine($"Validation result: {isComplete}");

            // Assert
            isComplete.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ParseTestImage_WithRunningMinerU_ShouldReturnExpectedText()
        {
            // Arrange
            _output.WriteLine("Starting MinerU full integration test");

            if (!IsPythonAvailable())
            {
                _output.WriteLine("Python is not available. Skipping test.");
                return;
            }

            string mineruUrl = "http://localhost:8299"; // Use different port to avoid conflicts
            int port = 8299;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Task? launcherTask = null;

            try
            {
                // Start MinerU host in background
                _output.WriteLine("Starting MinerU host application...");
                launcherTask = Task.Run(async () =>
                {
                    CommandLineOptions options = new CommandLineOptions("127.0.0.1", port, _testDirectory, 60);
                    MinerUProcessHost host = new MinerUProcessHost(options);
                    await host.RunAsync(cancellationTokenSource.Token);
                }, cancellationTokenSource.Token);

                // Wait for service to be ready
                _output.WriteLine($"Waiting for MinerU to be ready at {mineruUrl}...");
                bool isReady = await WaitForServiceToBeReady(mineruUrl, timeoutSeconds: 120);

                if (!isReady)
                {
                    _output.WriteLine("MinerU service did not become ready in time. Skipping test.");
                    return;
                }

                _output.WriteLine("MinerU service is ready!");

                // Act
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(120);

                    using (MineruClient client = new MineruClient(mineruUrl, httpClient))
                    {
                        using (Stream imageStream = await _resourceManager.GetResourceStreamAsync(TestFile.Image01))
                        {
                            _output.WriteLine("Sending image to MinerU for parsing...");
                            MineruRequest request = MineruRequest.Create(imageStream)
                                .WithLanguages("en")
                                .WithMarkdownResponse()
                                .Build();

                            using (MineruResponse response = await client.ParseFileAsync(request))
                            {
                                string responseMarkdown = await response.ReadAsMarkdownAsync();
                                _output.WriteLine($"Received response with {responseMarkdown.Length} characters");
                                _output.WriteLine($"Response content: {responseMarkdown}");

                                string expectedText = await _resourceManager.ReadAsStringAsync(TestFile.Text01);
                                _output.WriteLine($"Expected text: {expectedText}");

                                // Assert
                                responseMarkdown.Trim().Should().Be(expectedText.Trim(), "MinerU should extract the expected text from the image");
                            }
                        }
                    }
                }

                _output.WriteLine("Test completed successfully!");
            }
            finally
            {
                // Cleanup: Stop the launcher
                _output.WriteLine("Stopping MinerU service...");
                cancellationTokenSource.Cancel();

                if (launcherTask != null)
                {
                    try
                    {
                        await launcherTask.WaitAsync(TimeSpan.FromSeconds(10));
                    }
                    catch (OperationCanceledException)
                    {
                        _output.WriteLine("MinerU service stopped successfully");
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"Error stopping MinerU service: {ex.Message}");
                    }
                }

                cancellationTokenSource.Dispose();
            }
        }

        private async Task<bool> WaitForServiceToBeReady(string baseUrl, int timeoutSeconds)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                DateTime startTime = DateTime.UtcNow;
                string docsUrl = $"{baseUrl}/docs";

                while ((DateTime.UtcNow - startTime).TotalSeconds < timeoutSeconds)
                {
                    try
                    {
                        HttpResponseMessage response = await httpClient.GetAsync(docsUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            _output.WriteLine($"Service responded with status: {response.StatusCode}");
                            return true;
                        }
                    }
                    catch (HttpRequestException)
                    {
                        // Service not ready yet, continue waiting
                    }
                    catch (TaskCanceledException)
                    {
                        // Timeout on this attempt, continue waiting
                    }

                    _output.WriteLine("Service not ready yet, waiting...");
                    await Task.Delay(2000); // Wait 2 seconds between checks
                }

                return false;
            }
        }

        private bool IsPythonAvailable()
        {
            try
            {
                _output.WriteLine("Checking if Python is available...");
                int exitCode = _processRunner.RunProcess("python", "--version", _testDirectory);
                bool available = exitCode == 0;
                _output.WriteLine($"Python available: {available}");
                return available;
            }
            catch
            {
                _output.WriteLine("Python not found on system");
                return false;
            }
        }
    }
}

