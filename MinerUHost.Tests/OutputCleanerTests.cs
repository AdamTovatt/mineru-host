using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MinerUHost.Tests
{
    public class OutputCleanerTests : IDisposable
    {
        private readonly Mock<ILogger<OutputCleaner>> _loggerMock;
        private readonly OutputCleaner _outputCleaner;
        private readonly string _testDirectory;

        public OutputCleanerTests()
        {
            _loggerMock = new Mock<ILogger<OutputCleaner>>();
            _outputCleaner = new OutputCleaner(_loggerMock.Object);
            _testDirectory = Path.Combine(Path.GetTempPath(), $"output-cleaner-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, recursive: true);
                }
                catch
                {
                    // Best effort cleanup
                }
            }
        }

        [Fact]
        public void CleanOutputDirectory_WithFilesAndDirectories_DeletesAllContent()
        {
            // Arrange
            string outputDirectory = Path.Combine(_testDirectory, "output");
            Directory.CreateDirectory(outputDirectory);

            string testFile1 = Path.Combine(outputDirectory, "test1.txt");
            string testFile2 = Path.Combine(outputDirectory, "test2.pdf");
            File.WriteAllText(testFile1, "test content 1");
            File.WriteAllText(testFile2, "test content 2");

            string subDirectory = Path.Combine(outputDirectory, "subdir");
            Directory.CreateDirectory(subDirectory);
            string nestedFile = Path.Combine(subDirectory, "nested.txt");
            File.WriteAllText(nestedFile, "nested content");

            // Act
            _outputCleaner.CleanOutputDirectory(_testDirectory);

            // Assert
            Directory.Exists(outputDirectory).Should().BeTrue("output directory itself should still exist");
            Directory.GetFiles(outputDirectory).Should().BeEmpty("all files should be deleted");
            Directory.GetDirectories(outputDirectory).Should().BeEmpty("all subdirectories should be deleted");
        }

        [Fact]
        public void CleanOutputDirectory_WithNonExistentDirectory_DoesNotThrow()
        {
            // Arrange
            string nonExistentPath = Path.Combine(_testDirectory, "nonexistent");

            // Act
            Action act = () => _outputCleaner.CleanOutputDirectory(nonExistentPath);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void CleanOutputDirectory_WithNonExistentDirectory_LogsDebugMessage()
        {
            // Arrange
            string nonExistentPath = Path.Combine(_testDirectory, "nonexistent");

            // Act
            _outputCleaner.CleanOutputDirectory(nonExistentPath);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("does not exist")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanOutputDirectory_WithEmptyDirectory_CompletesSuccessfully()
        {
            // Arrange
            string outputDirectory = Path.Combine(_testDirectory, "output");
            Directory.CreateDirectory(outputDirectory);

            // Act
            _outputCleaner.CleanOutputDirectory(_testDirectory);

            // Assert
            Directory.Exists(outputDirectory).Should().BeTrue("output directory should still exist");
            Directory.GetFiles(outputDirectory).Should().BeEmpty();
            Directory.GetDirectories(outputDirectory).Should().BeEmpty();
        }

        [Fact]
        public void CleanOutputDirectory_WhenCompletes_LogsInformationMessages()
        {
            // Arrange
            string outputDirectory = Path.Combine(_testDirectory, "output");
            Directory.CreateDirectory(outputDirectory);
            File.WriteAllText(Path.Combine(outputDirectory, "test.txt"), "test");

            // Act
            _outputCleaner.CleanOutputDirectory(_testDirectory);

            // Assert - Should log start and completion messages
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cleaning up")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("cleanup completed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanOutputDirectory_WithReadOnlyFile_LogsWarningAndContinues()
        {
            // Skip on non-Windows platforms where ReadOnly doesn't prevent deletion
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            // Arrange
            string outputDirectory = Path.Combine(_testDirectory, "output");
            Directory.CreateDirectory(outputDirectory);

            string readOnlyFile = Path.Combine(outputDirectory, "readonly.txt");
            File.WriteAllText(readOnlyFile, "readonly content");
            File.SetAttributes(readOnlyFile, FileAttributes.ReadOnly);

            string normalFile = Path.Combine(outputDirectory, "normal.txt");
            File.WriteAllText(normalFile, "normal content");

            // Act
            _outputCleaner.CleanOutputDirectory(_testDirectory);

            // Assert - Should log warning about readonly file
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            // Cleanup - remove readonly attribute for disposal
            if (File.Exists(readOnlyFile))
                File.SetAttributes(readOnlyFile, FileAttributes.Normal);
        }

        [Fact]
        public void CleanOutputDirectory_WithMultipleFiles_DeletesAllFiles()
        {
            // Arrange
            string outputDirectory = Path.Combine(_testDirectory, "output");
            Directory.CreateDirectory(outputDirectory);

            for (int i = 0; i < 10; i++)
            {
                File.WriteAllText(Path.Combine(outputDirectory, $"file{i}.txt"), $"content {i}");
            }

            // Act
            _outputCleaner.CleanOutputDirectory(_testDirectory);

            // Assert
            Directory.GetFiles(outputDirectory).Should().BeEmpty();
        }

        [Fact]
        public void CleanOutputDirectory_WithNestedDirectories_DeletesAllRecursively()
        {
            // Arrange
            string outputDirectory = Path.Combine(_testDirectory, "output");
            Directory.CreateDirectory(outputDirectory);

            string level1 = Path.Combine(outputDirectory, "level1");
            Directory.CreateDirectory(level1);
            string level2 = Path.Combine(level1, "level2");
            Directory.CreateDirectory(level2);
            string level3 = Path.Combine(level2, "level3");
            Directory.CreateDirectory(level3);
            File.WriteAllText(Path.Combine(level3, "deep.txt"), "deep file");

            // Act
            _outputCleaner.CleanOutputDirectory(_testDirectory);

            // Assert
            Directory.GetDirectories(outputDirectory).Should().BeEmpty("all nested directories should be deleted");
        }
    }
}

