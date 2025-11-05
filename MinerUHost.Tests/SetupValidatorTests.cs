using FluentAssertions;

namespace MinerUHost.Tests
{
    public class SetupValidatorTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly SetupValidator _validator;

        public SetupValidatorTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), $"mineru-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
            _validator = new SetupValidator();
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
        }

        [Fact]
        public void IsSetupComplete_WhenVenvAndMarkerExist_ReturnsTrue()
        {
            // Arrange
            string venvPath = Path.Combine(_testDirectory, "mineru-venv");
            string markerPath = Path.Combine(_testDirectory, ".mineru-setup-complete");

            Directory.CreateDirectory(venvPath);
            File.WriteAllText(markerPath, "");

            // Act
            bool result = _validator.IsSetupComplete(_testDirectory);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsSetupComplete_WhenVenvDoesNotExist_ReturnsFalse()
        {
            // Arrange
            string markerPath = Path.Combine(_testDirectory, ".mineru-setup-complete");
            File.WriteAllText(markerPath, "");

            // Act
            bool result = _validator.IsSetupComplete(_testDirectory);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsSetupComplete_WhenMarkerDoesNotExist_ReturnsFalse()
        {
            // Arrange
            string venvPath = Path.Combine(_testDirectory, "mineru-venv");
            Directory.CreateDirectory(venvPath);

            // Act
            bool result = _validator.IsSetupComplete(_testDirectory);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsSetupComplete_WhenNothingExists_ReturnsFalse()
        {
            // Act
            bool result = _validator.IsSetupComplete(_testDirectory);

            // Assert
            result.Should().BeFalse();
        }
    }
}

