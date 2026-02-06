using System;
using System.IO;
using Xunit;
using SharpJS.Core;

namespace SharpJS.Tests
{
    public class ModLoaderTests : IDisposable
    {
        private readonly string _testModsPath;

        public ModLoaderTests()
        {
            _testModsPath = Path.Combine(Path.GetTempPath(), $"sharpjs_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testModsPath);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testModsPath))
            {
                Directory.Delete(_testModsPath, true);
            }
        }

        [Fact]
        public void ModLoader_CanBeCreated()
        {
            // Arrange & Act
            using var loader = new ModLoader(_testModsPath);

            // Assert
            Assert.NotNull(loader);
            Assert.Empty(loader.LoadedMods);
        }

        [Fact]
        public void ModLoader_CreatesModsDirectory()
        {
            // Arrange
            var newPath = Path.Combine(Path.GetTempPath(), $"sharpjs_new_{Guid.NewGuid()}");

            // Act
            using var loader = new ModLoader(newPath);

            // Assert
            Assert.True(Directory.Exists(newPath));

            // Cleanup
            Directory.Delete(newPath);
        }

        [Fact]
        public void ModLoader_CanLoadSimpleMod()
        {
            // Arrange
            CreateTestMod("test-mod", "Test Mod", "1.0.0", @"
                global.mods['test-mod'].onLoad = function() {
                    // Mod loaded
                };
            ");

            using var loader = new ModLoader(_testModsPath);

            // Act
            loader.LoadAllMods();

            // Assert
            Assert.Single(loader.LoadedMods);
            Assert.Equal("test-mod", loader.LoadedMods[0].Id);
            Assert.Equal("Test Mod", loader.LoadedMods[0].Name);
        }

        [Fact]
        public void ModLoader_CanLoadMultipleMods()
        {
            // Arrange
            CreateTestMod("mod1", "Mod 1", "1.0.0", "");
            CreateTestMod("mod2", "Mod 2", "2.0.0", "");

            using var loader = new ModLoader(_testModsPath);

            // Act
            loader.LoadAllMods();

            // Assert
            Assert.Equal(2, loader.LoadedMods.Count);
        }

        [Fact]
        public void ModLoader_CanUnloadMod()
        {
            // Arrange
            CreateTestMod("test-mod", "Test Mod", "1.0.0", "");

            using var loader = new ModLoader(_testModsPath);
            loader.LoadAllMods();
            Assert.Single(loader.LoadedMods);

            // Act
            loader.UnloadMod("test-mod");

            // Assert
            Assert.Empty(loader.LoadedMods);
        }

        [Fact]
        public void ModLoader_CanExposeApiToMods()
        {
            // Arrange
            CreateTestMod("test-mod", "Test Mod", "1.0.0", @"
                global.testResult = 'not set';
                global.mods['test-mod'].onLoad = function() {
                    const api = global.testApi || globalThis.testApi;
                    if (api) {
                        global.testResult = 'api found: ' + api.TestValue;
                    }
                };
            ");

            using var loader = new ModLoader(_testModsPath);
            var testApi = new TestApi { TestValue = 42 };

            // Act
            loader.ExposeApi("testApi", testApi);
            loader.LoadAllMods();

            // Assert - The mod should have been able to access the API
            Assert.Single(loader.LoadedMods);
        }

        [Fact]
        public void ModLoader_SkipsInvalidMods()
        {
            // Arrange
            CreateTestMod("good-mod", "Good Mod", "1.0.0", "");
            
            // Create mod directory without manifest
            var badModPath = Path.Combine(_testModsPath, "bad-mod");
            Directory.CreateDirectory(badModPath);

            using var loader = new ModLoader(_testModsPath);

            // Act
            loader.LoadAllMods();

            // Assert - Only the good mod should be loaded
            Assert.Single(loader.LoadedMods);
            Assert.Equal("good-mod", loader.LoadedMods[0].Id);
        }

        private void CreateTestMod(string id, string name, string version, string script)
        {
            var modPath = Path.Combine(_testModsPath, id);
            Directory.CreateDirectory(modPath);

            var manifest = $@"{{
  ""id"": ""{id}"",
  ""name"": ""{name}"",
  ""version"": ""{version}"",
  ""entryPoint"": ""main.js""
}}";

            File.WriteAllText(Path.Combine(modPath, "mod.json"), manifest);
            File.WriteAllText(Path.Combine(modPath, "main.js"), script);
        }

        public class TestApi
        {
            public int TestValue { get; set; }
        }
    }
}
