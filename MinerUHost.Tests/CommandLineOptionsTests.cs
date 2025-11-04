using FluentAssertions;

namespace MinerUHost.Tests
{
    public class CommandLineOptionsTests
    {
        [Fact]
        public void Parse_WithNoArguments_ReturnsDefaultValues()
        {
            // Arrange
            string[] args = Array.Empty<string>();

            // Act
            CommandLineOptions options = CommandLineOptions.Parse(args);

            // Assert
            options.Host.Should().Be("0.0.0.0");
            options.Port.Should().Be(8200);
            options.InstallPath.Should().Be(AppContext.BaseDirectory);
            options.CleanupIntervalMinutes.Should().Be(5);
        }

        [Fact]
        public void Parse_WithHostArgument_SetsHost()
        {
            // Arrange
            string[] args = new[] { "--host", "127.0.0.1" };

            // Act
            CommandLineOptions options = CommandLineOptions.Parse(args);

            // Assert
            options.Host.Should().Be("127.0.0.1");
        }

        [Fact]
        public void Parse_WithPortArgument_SetsPort()
        {
            // Arrange
            string[] args = new[] { "--port", "9000" };

            // Act
            CommandLineOptions options = CommandLineOptions.Parse(args);

            // Assert
            options.Port.Should().Be(9000);
        }

        [Fact]
        public void Parse_WithInstallPathArgument_SetsInstallPath()
        {
            // Arrange
            string[] args = new[] { "--install-path", "/custom/path" };

            // Act
            CommandLineOptions options = CommandLineOptions.Parse(args);

            // Assert
            options.InstallPath.Should().Be("/custom/path");
        }

        [Fact]
        public void Parse_WithCleanupIntervalArgument_SetsCleanupInterval()
        {
            // Arrange
            string[] args = new[] { "--cleanup-interval", "10" };

            // Act
            CommandLineOptions options = CommandLineOptions.Parse(args);

            // Assert
            options.CleanupIntervalMinutes.Should().Be(10);
        }

        [Fact]
        public void Parse_WithAllArguments_SetsAllValues()
        {
            // Arrange
            string[] args = new[] { "--host", "localhost", "--port", "8080", "--install-path", "/app", "--cleanup-interval", "15" };

            // Act
            CommandLineOptions options = CommandLineOptions.Parse(args);

            // Assert
            options.Host.Should().Be("localhost");
            options.Port.Should().Be(8080);
            options.InstallPath.Should().Be("/app");
            options.CleanupIntervalMinutes.Should().Be(15);
        }

        [Fact]
        public void Parse_WithInvalidPort_ThrowsArgumentException()
        {
            // Arrange
            string[] args = new[] { "--port", "invalid" };

            // Act
            Action act = () => CommandLineOptions.Parse(args);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Invalid port value: invalid");
        }

        [Fact]
        public void Parse_WithInvalidCleanupInterval_ThrowsArgumentException()
        {
            // Arrange
            string[] args = new[] { "--cleanup-interval", "invalid" };

            // Act
            Action act = () => CommandLineOptions.Parse(args);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Invalid cleanup interval value: invalid");
        }
    }
}

