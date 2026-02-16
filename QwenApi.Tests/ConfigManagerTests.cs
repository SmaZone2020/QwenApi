using QwenApi.Common;
using Xunit;
using System.IO;
using System.Text.Json;

namespace QwenApi.Tests
{
    public class ConfigManagerTests
    {
        [Fact]
        public void Load_WhenFileDoesNotExist_CreatesNewFile()
        {
            // Arrange
            string testConfigPath = Path.Combine(Environment.CurrentDirectory, "config", "test-config.json");
            if (File.Exists(testConfigPath))
            {
                File.Delete(testConfigPath);
            }

            // Act
            var configManager = new ConfigManager("test-config.json");

            // Assert
            Assert.True(File.Exists(testConfigPath));
            
            // Clean up
            File.Delete(testConfigPath);
        }

        [Fact]
        public void Save_WhenCalled_WritesConfigToFile()
        {
            // Arrange
            string testConfigPath = Path.Combine(Environment.CurrentDirectory, "config", "test-config.json");
            if (File.Exists(testConfigPath))
            {
                File.Delete(testConfigPath);
            }

            var configManager = new ConfigManager("test-config.json");
            configManager.LoadString("test-bxua", "test-cookie", "test-umidtoken");

            // Act
            bool saveResult = configManager.Save();

            // Assert
            Assert.True(saveResult);
            Assert.True(File.Exists(testConfigPath));

            string jsonContent = File.ReadAllText(testConfigPath);
            var config = JsonSerializer.Deserialize<ConfigManagerTestsConfig>(jsonContent);
            Assert.Equal("test-bxua", config.BxUa);
            Assert.Equal("test-cookie", config.Cookie);
            Assert.Equal("test-umidtoken", config.BxUmidtoken);
            
            // Clean up
            File.Delete(testConfigPath);
        }

        [Fact]
        public void IsConfigured_WhenAllFieldsAreSet_ReturnsTrue()
        {
            // Arrange
            var configManager = new ConfigManager("test-config.json");
            configManager.LoadString("test-bxua", "test-cookie", "test-umidtoken");

            // Act
            bool isConfigured = configManager.IsConfigured;

            // Assert
            Assert.True(isConfigured);
        }

        [Fact]
        public void IsConfigured_WhenSomeFieldsAreMissing_ReturnsFalse()
        {
            // Arrange
            var configManager = new ConfigManager("test-config.json");
            configManager.LoadString("", "test-cookie", "test-umidtoken");

            // Act
            bool isConfigured = configManager.IsConfigured;

            // Assert
            Assert.False(isConfigured);
        }

        private class ConfigManagerTestsConfig
        {
            public string BxUa { get; set; } = string.Empty;
            public string Cookie { get; set; } = string.Empty;
            public string BxUmidtoken { get; set; } = string.Empty;
        }
    }
}
