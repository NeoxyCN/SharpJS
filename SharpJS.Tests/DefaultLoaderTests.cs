using System;
using System.IO;
using Xunit;
using SharpJS.Core;

namespace SharpJS.Tests
{
    public class DefaultLoaderTests : IDisposable
    {
        private readonly string _testPath;

        public DefaultLoaderTests()
        {
            _testPath = Path.Combine(Path.GetTempPath(), $"loader_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testPath);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testPath))
            {
                Directory.Delete(_testPath, true);
            }
        }

        [Fact]
        public void DefaultLoader_CanBeCreated()
        {
            // Arrange & Act
            var loader = new DefaultLoader();

            // Assert
            Assert.NotNull(loader);
        }

        [Fact]
        public void DefaultLoader_CanReadExistingFile()
        {
            // Arrange
            var testFile = Path.Combine(_testPath, "test.js");
            File.WriteAllText(testFile, "console.log('test');");
            var loader = new DefaultLoader(_testPath);

            // Act
            var content = loader.ReadFile("test.js", out var debugPath);

            // Assert
            Assert.Equal("console.log('test');", content);
            Assert.Contains("test.js", debugPath);
        }

        [Fact]
        public void DefaultLoader_ThrowsForNonExistentFile()
        {
            // Arrange
            var loader = new DefaultLoader(_testPath);

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => 
                loader.ReadFile("nonexistent.js", out var _));
        }

        [Fact]
        public void DefaultLoader_CanCheckFileExists()
        {
            // Arrange
            var testFile = Path.Combine(_testPath, "exists.js");
            File.WriteAllText(testFile, "// test");
            var loader = new DefaultLoader(_testPath);

            // Act
            var exists = loader.FileExists("exists.js");
            var notExists = loader.FileExists("notexists.js");

            // Assert
            Assert.True(exists);
            Assert.False(notExists);
        }

        [Fact]
        public void DefaultLoader_AddsJsExtension()
        {
            // Arrange
            var testFile = Path.Combine(_testPath, "module.js");
            File.WriteAllText(testFile, "export default {}");
            var loader = new DefaultLoader(_testPath);

            // Act
            var content = loader.ReadFile("module", out var _);

            // Assert
            Assert.Equal("export default {}", content);
        }

        [Fact]
        public void DefaultLoader_IdentifiesESMFiles()
        {
            // Arrange
            var loader = new DefaultLoader();

            // Act
            var isESM1 = loader.IsESM("module.mjs");
            var isESM2 = loader.IsESM("module.js");

            // Assert
            Assert.True(isESM1);
            Assert.False(isESM2);
        }
    }
}
